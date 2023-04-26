using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

class wrs_sysinfo
{
    public struct SystemInfo
    {
        public string? OsVersion { get; set; }
        public string? BiosManufacturer { get; set; }
        public string? MainboardName { get; set; }
        public string? CpuName { get; set; }
        public ulong TotalPhysicalMemoryInMb { get; set; }
        public string? GpuName { get; set; }
        public string? MacAddress { get; set; }
        public string? IpAddress { get; set; }
    }

    public static SystemInfo GetSystemInfo()
    {
        SystemInfo systemInfo = new SystemInfo();

        Thread osVersionThread = new Thread(() => systemInfo.OsVersion = GetOsVersion());
        Thread biosManufacturerThread = new Thread(() => systemInfo.BiosManufacturer = GetBiosManufacturer());
        Thread mainboardNameThread = new Thread(() => systemInfo.MainboardName = GetMainboardName());
        Thread cpuNameThread = new Thread(() => systemInfo.CpuName = GetCpuName());
        Thread totalPhysicalMemoryInMbThread = new Thread(() => systemInfo.TotalPhysicalMemoryInMb = GetTotalPhysicalMemoryInMb());
        Thread gpuNameThread = new Thread(() => systemInfo.GpuName = GetGpuName());
        Thread macAddressThread = new Thread(() => systemInfo.MacAddress = GetMacAddress());
        Thread ipAddressThread = new Thread(() => systemInfo.IpAddress = GetLanIpAddress());

        osVersionThread.Start();
        biosManufacturerThread.Start();
        mainboardNameThread.Start();
        cpuNameThread.Start();
        totalPhysicalMemoryInMbThread.Start();
        gpuNameThread.Start();
        macAddressThread.Start();
        ipAddressThread.Start();

        osVersionThread.Join();
        biosManufacturerThread.Join();
        mainboardNameThread.Join();
        cpuNameThread.Join();
        totalPhysicalMemoryInMbThread.Join();
        gpuNameThread.Join();
        macAddressThread.Join();
        ipAddressThread.Join();

        return systemInfo;
    }

    public static string GetOsVersion()
    {
        try
        {
            return Environment.OSVersion.ToString();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to query OS version: {ex.Message}");
            wrc_generator.logs!.LogError($"Failed to query OS version: {ex.Message}");
            return "Unknown";
        }
    }

    public static string GetBiosManufacturer()
    {
        try
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    return QueryManagement.QuerySafeGetter(obj, "Manufacturer");
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to query BIOS manufacturer: {ex.Message}");
            wrc_generator.logs!.LogError($"Failed to query BIOS manufacturer: {ex.Message}");
        }
        return "Unknown";
    }

    public static string GetMainboardName()
    {
        try
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    return QueryManagement.QuerySafeGetter(obj, "Product");
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to query mainboard name: {ex.Message}");
            wrc_generator.logs!.LogError($"Failed to query mainboard name: {ex.Message}");
        }
        return "Unknown";
    }

    public static string GetCpuName()
    {
        try
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    return QueryManagement.QuerySafeGetter(obj, "Name");
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to query CPU name: {ex.Message}");
            wrc_generator.logs!.LogError($"Failed to query CPU name: {ex.Message}");
        }
        return "Unknown";
    }

    public static ulong GetTotalPhysicalMemoryInMb()
    {
        try
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    ulong totalMemory = Convert.ToUInt64(QueryManagement.QuerySafeGetter(obj, "TotalPhysicalMemory"));
                    return totalMemory / (1024 * 1024);
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to query total physical memory: {ex.Message}");
            wrc_generator.logs!.LogError($"Failed to query total physical memory: {ex.Message}");
        }
        return 0;
    }

    public static string GetGpuName()
    {
        try
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    return QueryManagement.QuerySafeGetter(obj, "Name");
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to query GPU name: {ex.Message}");
            wrc_generator.logs!.LogError($"Failed to query GPU name: {ex.Message}");
        }
        return "Unknown";
    }

    public static string GetLanIpAddress()
    {
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to query LAN IP address: {ex.Message}");
            wrc_generator.logs!.LogError($"Failed to query LAN IP address: {ex.Message}");
        }
        return "Unknown";
    }

    public static string GetMacAddress()
    {
        try
        {
            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    return BitConverter.ToString(networkInterface.GetPhysicalAddress().GetAddressBytes());
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to query MAC address: {ex.Message}");
            wrc_generator.logs!.LogError($"Failed to query MAC address: {ex.Message}");
        }
        return "Unknown";
    }

    public static string GetCurrentDate()
    {
        return DateTime.Now.ToString("dd/M/yyyy");
    }

    public static string GetSystemHash()
    {
        return Cryptography.ComputeSha256Hash(GetCpuName() + GetMainboardName() + GetBiosManufacturer());
    }

    public static bool CheckIfVirtualized()
    {
        try
        {
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    string manufacturer = QueryManagement.QuerySafeGetter(obj, "Manufacturer")?.ToLowerInvariant() ?? string.Empty;
                    string model = QueryManagement.QuerySafeGetter(obj, "Model")?.ToLowerInvariant() ?? string.Empty;

                    string[] virtualizationKeywords = new string[]
                    {
                    "vmware", "virtual", "xen", "parallels", "kvm", "qemu",
                    "hyperv", "vbox", "oracle vm virtualbox", "microsoft virtual", "red hat kvm"
                    };

                    foreach (string keyword in virtualizationKeywords)
                    {
                        if (manufacturer.Contains(keyword) || model.Contains(keyword))
                        {
                            return true;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to query virtualization status: {ex.Message}");
            wrc_generator.logs!.LogError($"Failed to query virtualization status: {ex.Message}");
        }

        return false;
    }

    public static bool IsTpmEnabledAndActivated()
    {
        try
        {
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_TPM"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    bool isEnabled = Convert.ToBoolean(obj["IsEnabled"]);
                    bool isActivated = Convert.ToBoolean(obj["IsActivated"]);

                    if (isEnabled && isActivated)
                    {
                        return true;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to query TPM status: {ex.Message}");
            wrc_generator.logs!.LogError($"Failed to query TPM status: {ex.Message}");
        }

        return false;
    }

    public static void DisplaySystemInfo()
    {
        Console.WriteLine("----System Information----");
        Console.WriteLine("OS Version: {0}", GetOsVersion());
        Console.WriteLine("BIOS Manufacturer: {0}", GetBiosManufacturer());
        Console.WriteLine("Mainboard Name: {0}", GetMainboardName());
        Console.WriteLine("CPU Name: {0}", GetCpuName());
        Console.WriteLine("Total Physical Memory (MB): {0}", GetTotalPhysicalMemoryInMb());
        Console.WriteLine("GPU Name: {0}", GetGpuName());
        Console.WriteLine("LAN IP Address: {0}", GetLanIpAddress());
        Console.WriteLine("MAC Address: {0}", GetMacAddress());
        Console.WriteLine("Current Date: {0}", GetCurrentDate());
        Console.WriteLine("System Hash: {0}", GetSystemHash());
        Console.WriteLine("Is Virtualized: {0}", CheckIfVirtualized());
        Console.WriteLine("Is TPM Enabled and Activated: {0}", IsTpmEnabledAndActivated());
        Console.WriteLine();
    }

}