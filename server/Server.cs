using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

public static class TaskExtensions
{
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

public class WrsServer
{

    private readonly int _serverPort;
    private readonly Func<string, string, bool> _authenticateUser;
    private readonly IPBlacklist _ipBlacklist = new IPBlacklist();

    public WrsServer(int serverPort, Func<string, string, bool> authenticateUser)
    {
        _serverPort = serverPort;
        _authenticateUser = authenticateUser;
    }

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
        catch (OperationCanceledException)
        {
            // This exception is expected when the cancellationToken is canceled.
        }
        finally
        {
            listener.Stop();
        }
    }

    private async Task ProcessClient(TcpClient client)
    {
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
    }

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

    private Aes ConfigureAes(string username, string password, NetworkStream networkStream)
    {
        Aes aes = Aes.Create();
        byte[] iv = new byte[16];
        networkStream.Read(iv, 0, iv.Length);

        using (Rfc2898DeriveBytes keyDerivation = new Rfc2898DeriveBytes(username + ":" + password, Encoding.UTF8.GetBytes("your_salt_here"), 10000, HashAlgorithmName.SHA256))
        {
            aes.Key = keyDerivation.GetBytes(32);
            aes.IV = iv;
        }

        return aes;
    }


    private async Task DecryptAndSaveReceivedFile(NetworkStream networkStream, Aes aes)
    {
        // Read the file size from the client
        byte[] fileSizeBuffer = new byte[8];
        await networkStream.ReadAsync(fileSizeBuffer, 0, fileSizeBuffer.Length);
        long fileSize = BitConverter.ToInt64(fileSizeBuffer, 0);
        string receivedFilePath = $"received_file_{DateTime.Now.Ticks}.bin";
        using (CryptoStream cryptoStream = new CryptoStream(networkStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
        using (FileStream fileStream = File.Create(receivedFilePath))
        {
            // Copy the encrypted file content from the client
            await CopyWithProgressAsync(cryptoStream, fileStream, fileSize);
        }
    }

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