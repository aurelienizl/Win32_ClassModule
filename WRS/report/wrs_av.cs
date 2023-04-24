using System.Management;

class wrs_av
{
    public struct AntivirusInfo
    {
        public string? DisplayName { get; set; }
        public string? ProductState { get; set; }
    }

    public static void DisplayAntivirusInfo()
    {
        List<AntivirusInfo> antivirusInfos = GetAntivirusInfo();

        foreach (AntivirusInfo info in antivirusInfos)
        {
            Console.WriteLine("----Antivirus status----");
            Console.WriteLine("DisplayName: {0}", info.DisplayName);
            Console.WriteLine("ProductState: {0}", info.ProductState);
            Console.WriteLine();
        }
    }

    public static List<AntivirusInfo> GetAntivirusInfo()
    {
        List<AntivirusInfo> antivirusInfos = new List<AntivirusInfo>();

        try
        {
            using (ManagementObjectSearcher antivirusSearcher = new ManagementObjectSearcher(@"\\.\root\SecurityCenter2", "SELECT * FROM AntiVirusProduct"))
            {
                List<string> antivirusDisplayNames = new List<string>();
                foreach (ManagementObject queryObj in antivirusSearcher.Get())
                {
                    AntivirusInfo avInfo = new AntivirusInfo
                    {
                        DisplayName = QueryManagement.QuerySafeGetter(queryObj, "displayName"),
                        ProductState = QueryManagement.QuerySafeGetter(queryObj, "productState")
                    };

                    antivirusInfos.Add(avInfo);
                    antivirusDisplayNames.Add(avInfo.DisplayName);
                }
            }

        }
        catch (Exception)
        {
            Console.Error.WriteLine("Failed to query antivirus info.");
        }

        return antivirusInfos;
    }
}
