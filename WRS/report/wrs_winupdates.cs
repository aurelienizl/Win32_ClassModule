using System.Management;
using System.Threading.Tasks;

class wrs_winupdates
{
    public struct WinUpdateInfo
    {
        public string? HotFixID { get; set; }
        public string? Description { get; set; }
        public string? InstalledBy { get; set; }
        public string? InstalledOn { get; set; }
    }

    public static void DisplayWinUpdatesInfo()
    {
        var updates = GetWinUpdatesInfo();

        foreach (WinUpdateInfo update in updates)
        {
            Console.WriteLine("----Installed Security Patch----");
            Console.WriteLine("HotFixID: {0}", update.HotFixID);
            Console.WriteLine("Description: {0}", update.Description);
            Console.WriteLine("InstalledBy: {0}", update.InstalledBy);
            Console.WriteLine("InstalledOn: {0}", update.InstalledOn);
            Console.WriteLine();
        }
    }

    public static List<WinUpdateInfo> GetWinUpdatesInfo()
    {
        var updates = new List<WinUpdateInfo>();

        try
        {
            using (var searcher = new ManagementObjectSearcher("SELECT HotFixID, Description, InstalledBy, InstalledOn FROM Win32_QuickFixEngineering WHERE Description = 'Security Update'"))
            {
                var managementObjects = searcher.Get();
                var syncObj = new object();

                Parallel.ForEach(managementObjects.Cast<ManagementObject>(), obj =>
                {
                    var update = new WinUpdateInfo
                    {
                        HotFixID = QueryManagement.QuerySafeGetter(obj, "HotFixID"),
                        Description = QueryManagement.QuerySafeGetter(obj, "Description"),
                        InstalledBy = QueryManagement.QuerySafeGetter(obj, "InstalledBy"),
                        InstalledOn = QueryManagement.QuerySafeGetter(obj, "InstalledOn")
                    };

                    lock (syncObj)
                    {
                        updates.Add(update);
                    }
                });
            }
        }
        catch (Exception)
        {
            Console.Error.WriteLine("Failed to query WMI for installed security patches.");
        }

        return updates;
    }
}
