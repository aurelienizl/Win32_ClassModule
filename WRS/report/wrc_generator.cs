using System.Text.Json;

class wrc_generator
{
    public static WrsLogs? logs;
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
        logs = new WrsLogs();
        Report report = new Report();

        // Log start of report generation
        logs.LogInfo("Report generation started");

        // Start loading data asynchronously
        Task[] tasks = new Task[]
        {
        Task.Run(() => LoadAccountsInfo(ref report)),
        Task.Run(() => LoadAntivirusInfo(ref report)),
        Task.Run(() => LoadBiosInfo(ref report)),
        Task.Run(() => LoadTcpConnectionsInfo(ref report)),
        Task.Run(() => LoadProductsInfo(ref report)),
        Task.Run(() => LoadSystemInfo(ref report)),
        Task.Run(() => LoadVolumesInfo(ref report)),
        Task.Run(() => LoadWinUpdatesInfo(ref report)),
        };

        // Log the start of each task
        foreach (Task task in tasks)
        {
            logs.LogInfo($"Task {task.Id} started");
        }

        // Wait for all tasks to complete
        Task.WaitAll(tasks);

        // Log the end of each task
        foreach (Task task in tasks)
        {
            logs.LogInfo($"Task {task.Id} finished");
        }

        // Log end of report generation
        logs.LogInfo("Report generation finished");

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
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to generate report: {ex.Message}");
                logs.LogError($"Failed to generate report: {ex.Message}");
            }
            logs.LogInfo("Report path is: " + reportPath);
        }
        else
        {
            Console.Error.WriteLine("Failed to generate report: Could not get application data folder path.");
            logs.LogCritical("Failed to generate report: Could not get application data folder path.");
        }
        logs.LogInfo("Report generation finished");
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
            logs!.LogError($"Error loading accounts info: {ex.Message}");
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
            logs!.LogError($"Error loading antivirus info: {ex.Message}");
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
            logs!.LogError($"Error loading BIOS info: {ex.Message}");
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
            logs!.LogError($"Error loading TCP connections info: {ex.Message}");
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
            logs!.LogError($"Error loading products info: {ex.Message}");
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
            logs!.LogError($"Error loading system info: {ex.Message}");
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
            logs!.LogError($"Error loading volumes info: {ex.Message}");
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
            logs!.LogError($"Error loading Windows updates info: {ex.Message}");
        }
    }
}