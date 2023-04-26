// Description: A secure file transfer server.
// Author: Izoulet Aurélien, contact: aurelien.izoulet@epita.fr
// License: GPL-3.0


using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

public static class TaskExtensions
{

    /// <summary>
    /// Adds cancellation support to a Task.
    /// </summary>
    /// <typeparam name="T">The type of the Task result.</typeparam>
    /// <param name="task">The Task to add cancellation support to.</param>
    /// <param name="cancellationToken">The cancellation token to use.</param>
    /// <returns>A Task that will complete with the result of the original Task or throw an OperationCanceledException if the cancellation token is triggered.</returns>

    public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
    {
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        using (cancellationToken.Register(() => tcs.TrySetResult(true)))
        {
            if (await Task.WhenAny(task, tcs.Task).ConfigureAwait(false) == tcs.Task)
            {
                throw new OperationCanceledException(cancellationToken);
            }
            return await task.ConfigureAwait(false);
        }
    }
}

/// <summary>
/// Represents a secure file transfer server.
/// </summary>

public class WrsServer
{

    private readonly int _serverPort;
    private readonly Func<string, string, bool> _authenticateUser;
    private readonly IPBlacklist _ipBlacklist = new IPBlacklist();
    private readonly int _maxConcurrentConnections;
    private readonly long _maxFileSize;
    private readonly SemaphoreSlim _semaphore;

    /// <summary>
    /// Initializes a new instance of the <see cref="WrsServer"/> class.
    /// </summary>
    /// <param name="serverPort">The port on which the server will listen for incoming connections.</param>
    /// <param name="authenticateUser">A function that will be called to authenticate users. The function should return true if the user is authenticated, false otherwise.</param>
    /// <param name="maxConcurrentConnections">The maximum number of concurrent connections the server will accept.</param>
    /// <param name="maxFileSize">The maximum size of the files that can be sent to the server.</param>

    public WrsServer(int serverPort, Func<string, string, bool> authenticateUser, int maxConcurrentConnections = 100, int maxFileSize = 52428800)
    {
        _serverPort = serverPort;
        _authenticateUser = authenticateUser;
        _maxConcurrentConnections = maxConcurrentConnections;
        _maxFileSize = maxFileSize;
        _semaphore = new SemaphoreSlim(_maxConcurrentConnections, _maxConcurrentConnections);
    }

    /// <summary>
    /// Starts the server and listens for incoming connections.
    /// </summary>
    /// <param name="cancellationToken">A token to signal cancellation of the server.</param>
    /// <returns>An awaitable Task.</returns>

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        TcpListener listener = new TcpListener(IPAddress.Any, _serverPort);
        listener.Start();

        System.Console.WriteLine($"Server listening on port {_serverPort}.");

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                TcpClient client = await listener.AcceptTcpClientAsync().WithCancellation(cancellationToken).ConfigureAwait(false);
                _ = ProcessClient(client).ConfigureAwait(false);
            }
        }
        catch (SocketException ex)
        {
            Console.Error.WriteLine($"Error in the listener: {ex.Message}");
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
        }
        finally
        {
            listener.Stop();
        }
    }

    /// <summary>
    /// Processes a client connection.
    /// </summary>
    /// <param name="client">The client to process.</param>
    /// <returns>An awaitable Task.</returns>

    private async Task ProcessClient(TcpClient client)
    {
        await _semaphore.WaitAsync().ConfigureAwait(false);
        IPAddress clientIpAddress = ((IPEndPoint)client.Client.RemoteEndPoint!).Address;
        if (!_ipBlacklist.IsAllowed(clientIpAddress))
        {
            Console.WriteLine($"IP {clientIpAddress} is blacklisted. Disconnecting.");
            client.Close();
            return;
        }

        try
        {
            Console.WriteLine($"Client connected: {client.Client.RemoteEndPoint}");
            using (client)
            using (NetworkStream networkStream = client.GetStream())
            {
                string[]? credentials = ReadCredentials(networkStream);
                if (credentials == null || !_authenticateUser(credentials[0], credentials[1]))
                {
                    Console.WriteLine($"Invalid credentials from {client.Client.RemoteEndPoint}. Disconnecting.");
                    _ipBlacklist.RegisterFailedAttempt(clientIpAddress);
                    return;
                }

                using (Aes aes = ConfigureAes(credentials[0], credentials[1], networkStream))
                {
                    await DecryptAndSaveReceivedFile(networkStream, aes).ConfigureAwait(false);
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error processing client: {ex.Message}");
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Reads the credentials from the network stream.
    /// </summary>
    /// <param name="networkStream">The network stream to read from.</param>
    /// <returns>The credentials, or null if the credentials are invalid.</returns>

    private string[]? ReadCredentials(NetworkStream networkStream)
    {
        byte[] credentialsLengthBuffer = new byte[4];
        networkStream.Read(credentialsLengthBuffer, 0, credentialsLengthBuffer.Length);
        int credentialsLength = BitConverter.ToInt32(credentialsLengthBuffer, 0);

        byte[] credentialsBuffer = new byte[credentialsLength];
        networkStream.Read(credentialsBuffer, 0, credentialsBuffer.Length);
        string credentialsString = Encoding.UTF8.GetString(credentialsBuffer, 0, credentialsLength);
        string[] credentials = credentialsString.Split(':', StringSplitOptions.RemoveEmptyEntries);

        if (credentials.Length != 2)
        {
            Console.WriteLine($"Invalid credential format. Disconnecting.");
            return null;
        }

        return credentials;
    }

    /// <summary>
    /// Configures the AES algorithm.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="password">The password.</param>
    /// <param name="networkStream">The network stream to read from.</param>
    /// <returns>The configured AES algorithm.</returns>

    private Aes ConfigureAes(string username, string password, NetworkStream networkStream)
    {
        Aes aes = Aes.Create();
        aes.KeySize = 256;
        aes.BlockSize = 128;
        aes.Mode = CipherMode.CBC;

        byte[] iv = new byte[16];
        networkStream.Read(iv, 0, iv.Length);

        using (Rfc2898DeriveBytes keyDerivation = new Rfc2898DeriveBytes(username + ":" + password, Encoding.UTF8.GetBytes("your_salt_here"), 10000, HashAlgorithmName.SHA256))
        {
            aes.Key = keyDerivation.GetBytes(32);
            aes.IV = iv;
        }

        return aes;
    }

    /// <summary>
    /// Decrypts and saves the received file.
    /// </summary>
    /// <param name="networkStream">The network stream to read from.</param>
    /// <param name="aes">The AES algorithm to use.</param>
    /// <returns>An awaitable Task.</returns>

    private async Task DecryptAndSaveReceivedFile(NetworkStream networkStream, Aes aes)
    {
        byte[] fileSizeBuffer = new byte[8];
        await networkStream.ReadAsync(fileSizeBuffer, 0, fileSizeBuffer.Length);
        long fileSize = BitConverter.ToInt64(fileSizeBuffer, 0);

        if (fileSize > _maxFileSize)
        {
            Console.WriteLine($"File size exceeds the allowed limit ({_maxFileSize} bytes). Disconnecting.");
            return;
        }

        string receivedFilePath = $"received_file_{DateTime.Now.Ticks}.bin";
        using (CryptoStream cryptoStream = new CryptoStream(networkStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
        using (FileStream fileStream = File.Create(receivedFilePath))
        {
            // Copy the encrypted file content from the client
            await CopyWithProgressAsync(cryptoStream, fileStream, fileSize);
        }
    }

    /// <summary>
    /// Copies the content of a stream to another stream.
    /// </summary>
    /// <param name="source">The source stream.</param>
    /// <param name="destination">The destination stream.</param>
    /// <param name="totalBytes">The total number of bytes to copy.</param>
    /// <returns>An awaitable Task.</returns>

    private static async Task CopyWithProgressAsync(Stream source, Stream destination, long totalBytes)
    {
        byte[] buffer = new byte[81920];
        int bytesRead;
        long totalBytesRead = 0;
        while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            await destination.WriteAsync(buffer, 0, bytesRead);
            totalBytesRead += bytesRead;

            // Show progress
            Console.WriteLine($"Received {totalBytesRead} of {totalBytes} bytes ({(double)totalBytesRead / totalBytes:P})");
        }
    }
}