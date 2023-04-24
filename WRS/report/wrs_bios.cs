using System.Management;
class wrs_bios
{
    public struct BiosInfo
    {
        public string? Manufacturer { get; set; }
        public string? Version { get; set; }
        public string? SMBIOSBIOSVersion { get; set; }
        public string? ReleaseDate { get; set; }
    }

    public static void DisplayBiosInfo()
    {
        List<BiosInfo> biosList = GetBiosInfo();
        Console.WriteLine("----BIOS status----");

        int count = 1;
        foreach (BiosInfo bios in biosList)
        {
            Console.WriteLine("----BIOS {0}----", count);
            Console.WriteLine("Manufacturer: {0}", bios.Manufacturer);
            Console.WriteLine("Version: {0}", bios.Version);
            Console.WriteLine("SMBIOSBIOSVersion: {0}", bios.SMBIOSBIOSVersion);
            Console.WriteLine("ReleaseDate: {0}", bios.ReleaseDate);
            Console.WriteLine();
            count++;
        }
    }

    public static List<BiosInfo> GetBiosInfo()
    {
        List<BiosInfo> biosList = new List<BiosInfo>();

        try
        {
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    BiosInfo bios = new BiosInfo
                    {
                        Manufacturer = QueryManagement.QuerySafeGetter(obj, "Manufacturer"),
                        Version = QueryManagement.QuerySafeGetter(obj, "Version"),
                        SMBIOSBIOSVersion = QueryManagement.QuerySafeGetter(obj, "SMBIOSBIOSVersion"),
                        ReleaseDate = QueryManagement.QuerySafeGetter(obj, "ReleaseDate")
                    };
                    biosList.Add(bios);
                }
            }
        }
        catch (Exception)
        {
            Console.Error.WriteLine("Failed to query BIOS info.");
        }

        return biosList;
    }
}
