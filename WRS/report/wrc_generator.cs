using System.Text.Json;

class wrc_generator
{
    struct Report
    {
        public List<wrs_accounts.UserAccount> accounts { get; set; }
        public List<wrs_av.AntivirusInfo> av { get; set; }
        public List<wrs_bios.BiosInfo> bios { get; set; }
        public List<wrs_network.TcpConnectionInfo> tcpconnections { get; set; }
        public List<wrs_products.ProductInfo> products { get; set; }
        public wrs_sysinfo.SystemInfo sysinfo { get; set; }
        public List<wrs_volumes.VolumeInfo> volumes { get; set; }
        public List<wrs_winupdates.WinUpdateInfo> winupdates { get; set; }
    }

    public static void GenerateReport()
    {
        Report report = new Report();

        Thread accountsThread = new Thread(() => LoadAccountsInfo(ref report));
        Thread avThread = new Thread(() => LoadAntivirusInfo(ref report));
        Thread biosThread = new Thread(() => LoadBiosInfo(ref report));
        Thread tcpConnectionsThread = new Thread(() => LoadTcpConnectionsInfo(ref report));
        Thread productsThread = new Thread(() => LoadProductsInfo(ref report));
        Thread sysinfoThread = new Thread(() => LoadSystemInfo(ref report));
        Thread volumesThread = new Thread(() => LoadVolumesInfo(ref report));
        Thread winUpdatesThread = new Thread(() => LoadWinUpdatesInfo(ref report));

        accountsThread.Start();
        avThread.Start();
        biosThread.Start();
        tcpConnectionsThread.Start();
        productsThread.Start();
        sysinfoThread.Start();
        volumesThread.Start();
        winUpdatesThread.Start();

        accountsThread.Join();
        avThread.Join();
        biosThread.Join();
        tcpConnectionsThread.Join();
        productsThread.Join();
        sysinfoThread.Join();
        volumesThread.Join();
        winUpdatesThread.Join();

        // Serialize report to JSON
        string json = JsonSerializer.Serialize(report);

        // Write report to file
        string reportFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (!string.IsNullOrEmpty(reportFolderPath))
        {
            string reportPath = Path.Combine(reportFolderPath, "wrc", "report.json");

            try
            {
                // Create directory if it doesn't exist
                string? directoryPath = Path.GetDirectoryName(reportPath);
                if (!string.IsNullOrEmpty(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);

                    // Write report to file
                    File.WriteAllText(reportPath, json);
                }
            }
            catch (Exception)
            {
                Console.Error.WriteLine("Failed to generate report: Could not write to file or create directory.");
            }
            System.Console.WriteLine("Report path is: " + reportPath);
        }
        else
        {
            Console.Error.WriteLine("Failed to generate report: Could not get application data folder path.");
        }
        Console.WriteLine("Report generated");
    }

    private static void LoadAccountsInfo(ref Report report)
    {
        try
        {
            report.accounts = wrs_accounts.GetAccountsInfo();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error loading accounts info: {ex.Message}");
        }
    }

    private static void LoadAntivirusInfo(ref Report report)
    {
        try
        {
            report.av = wrs_av.GetAntivirusInfo();
        }
        catch (Exception ex)
        {
           Console.Error.WriteLine($"Error loading antivirus info: {ex.Message}");
        }
    }

    private static void LoadBiosInfo(ref Report report)
    {
        try
        {
            report.bios = wrs_bios.GetBiosInfo();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error loading BIOS info: {ex.Message}");
        }
    }

    private static void LoadTcpConnectionsInfo(ref Report report)
    {
        try
        {
            report.tcpconnections = wrs_network.GetTcpConnectionsInfo();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error loading TCP connections info: {ex.Message}");
        }
    }

    private static void LoadProductsInfo(ref Report report)
    {
        try
        {
            report.products = wrs_products.GetProductsInfo();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error loading products info: {ex.Message}");
        }
    }

    private static void LoadSystemInfo(ref Report report)
    {
        try
        {
            report.sysinfo = wrs_sysinfo.GetSystemInfo();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error loading system info: {ex.Message}");
        }
    }

    private static void LoadVolumesInfo(ref Report report)
    {
        try
        {
            report.volumes = wrs_volumes.GetVolumesInfo();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error loading volumes info: {ex.Message}");
        }
    }

    private static void LoadWinUpdatesInfo(ref Report report)
    {
        try
        {
            report.winupdates = wrs_winupdates.GetWinUpdatesInfo();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error loading Windows updates info: {ex.Message}");
        }
    }
}