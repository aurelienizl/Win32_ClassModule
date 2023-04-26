using Microsoft.Win32;
using System.Management;
using System.Collections.Concurrent;
class wrs_products
{

    private class ProductInfoComparer : IEqualityComparer<ProductInfo>
    {
        public bool Equals(ProductInfo x, ProductInfo y)
        {
            return string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(x.Version, y.Version, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(ProductInfo obj)
        {
            int hashProductName = obj.Name != null ? obj.Name.ToUpperInvariant().GetHashCode() : 0;
            int hashProductVersion = obj.Version != null ? obj.Version.ToUpperInvariant().GetHashCode() : 0;

            return hashProductName ^ hashProductVersion;
        }
    }


    public struct ProductInfo
    {
        public string? Name { get; set; }
        public string? Version { get; set; }
        public string? InstallDate { get; set; }
        public string? Vendor { get; set; }
    }

    public static void DisplayProductsInfo()
    {
        var products = GetProductsInfo();

        foreach (ProductInfo product in products)
        {
            Console.WriteLine("----Installed Application----");
            Console.WriteLine("Name: {0}", product.Name);
            Console.WriteLine("Version: {0}", product.Version);
            Console.WriteLine("InstallDate: {0}", product.InstallDate);
            Console.WriteLine("Vendor: {0}", product.Vendor);
            Console.WriteLine();
        }
    }

    public static List<ProductInfo> GetProductsInfo()
    {
        var products = new ConcurrentBag<ProductInfo>();

        try
        {
            // Get products from WMI
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Product"))
            {
                var managementObjects = searcher.Get();

                Parallel.ForEach(managementObjects.Cast<ManagementObject>(), obj =>
                {
                    var product = new ProductInfo
                    {
                        Name = QueryManagement.QuerySafeGetter(obj, "Name"),
                        Version = QueryManagement.QuerySafeGetter(obj, "Version"),
                        InstallDate = QueryManagement.QuerySafeGetter(obj, "InstallDate"),
                        Vendor = QueryManagement.QuerySafeGetter(obj, "Vendor")
                    };
                    products.Add(product);
                });
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error getting installed applications info: {ex.Message}");
            wrc_generator.logs!.LogError($"Error getting installed applications info: {ex.Message}");
        }

        try
        {
            // Get products from the registry
            string[] registryKeys = new string[]
            {
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
            @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            };

            Parallel.ForEach(registryKeys, keyPath =>
            {
                using (RegistryKey? key = Registry.LocalMachine.OpenSubKey(keyPath))
                {
                    if (key != null)
                    {
                        foreach (string subKeyName in key.GetSubKeyNames())
                        {
                            using (RegistryKey? subKey = key.OpenSubKey(subKeyName))
                            {
                                if (subKey != null)
                                {
                                    string? name = subKey.GetValue("DisplayName") as string;
                                    string? version = subKey.GetValue("DisplayVersion") as string;
                                    string? installDate = subKey.GetValue("InstallDate") as string;
                                    string? vendor = subKey.GetValue("Publisher") as string;

                                    if (!string.IsNullOrEmpty(name))
                                    {
                                        var product = new ProductInfo
                                        {
                                            Name = name,
                                            Version = version,
                                            InstallDate = installDate,
                                            Vendor = vendor
                                        };
                                        products.Add(product);
                                    }
                                }
                            }
                        }
                    }
                }
            });
        }

        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error getting installed applications info: {ex.Message}");
            wrc_generator.logs!.LogError($"Error getting installed applications info: {ex.Message}");
        }
        return products.Distinct(new ProductInfoComparer()).ToList();
    }

}
