
class Program
{
    public static async Task Main()
    {
        string serverAddress = "127.0.0.1";
        int serverPort = 2222;
        string username = "admin";
        string password = "password";
        string filePath = @"C:\Users\Administration\Desktop\WCR\LICENSE";

        WrsClient wrsClient = new WrsClient(serverAddress, serverPort, username, password);
        await wrsClient.SendFile(filePath);
    }
}
