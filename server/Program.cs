class Program
{
    public static async Task Main()
    {
        int serverPort = 2222;
        Func<string, string, bool> authenticateUser = (username, password) =>
        {
            return username == "admin" && password == "password";
        };

        WrsServer wrsServer = new WrsServer(serverPort, authenticateUser);

        CancellationTokenSource cts = new CancellationTokenSource();
        Console.CancelKeyPress += (sender, e) =>
        {
            System.Console.WriteLine("Stopping server...");
            e.Cancel = true;
            cts.Cancel();
        };

        await wrsServer.StartAsync(cts.Token);
    }
}

