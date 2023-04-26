using System;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

public class WrsClient
{
    private readonly string _serverAddress;
    private readonly int _serverPort;
    private readonly string _username;
    private readonly string _password;

    public WrsClient(string serverAddress, int serverPort, string username, string password)
    {
        _serverAddress = serverAddress;
        _serverPort = serverPort;
        _username = username;
        _password = password;
    }

    public async Task SendFile(string filePath)
    {
        try
        {
            using (TcpClient client = new TcpClient(_serverAddress, _serverPort))
            using (NetworkStream networkStream = client.GetStream())
            {
                // Send credentials
                await SendCredentials(networkStream).ConfigureAwait(false);

                using (Aes aes = CreateAesInstance())
                {
                    // Send IV to the server
                    await networkStream.WriteAsync(aes.IV, 0, aes.IV.Length).ConfigureAwait(false);

                    // Encrypt and send the file
                    using (FileStream fileStream = File.OpenRead(filePath))
                    {
                        // Send the file size
                        byte[] fileSizeBuffer = BitConverter.GetBytes(fileStream.Length);
                        await networkStream.WriteAsync(fileSizeBuffer, 0, fileSizeBuffer.Length).ConfigureAwait(false);

                        using (CryptoStream cryptoStream = new CryptoStream(networkStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            await fileStream.CopyToAsync(cryptoStream).ConfigureAwait(false);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error sending file: {ex.Message}");
        }
    }

    private async Task SendCredentials(NetworkStream networkStream)
    {
        byte[] credentialsBuffer = Encoding.UTF8.GetBytes(_username + ":" + _password);

        // Send the length of the credentials string
        byte[] credentialsLengthBuffer = BitConverter.GetBytes(credentialsBuffer.Length);
        await networkStream.WriteAsync(credentialsLengthBuffer, 0, credentialsLengthBuffer.Length).ConfigureAwait(false);

        // Send the credentials to the server
        await networkStream.WriteAsync(credentialsBuffer, 0, credentialsBuffer.Length).ConfigureAwait(false);
    }

    private Aes CreateAesInstance()
    {
        Aes aes = Aes.Create();
        using (Rfc2898DeriveBytes keyDerivation = new Rfc2898DeriveBytes(_username + ":" + _password, Encoding.UTF8.GetBytes("your_salt_here"), 10000, HashAlgorithmName.SHA256))
        {
            aes.Key = keyDerivation.GetBytes(32);
            aes.IV = keyDerivation.GetBytes(16);
        }
        return aes;
    }
}
