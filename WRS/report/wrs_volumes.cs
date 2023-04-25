using System.Management;

class wrs_volumes
{
    public struct VolumeInfo
    {
        public string? Name;
        public string? FileSystem;
        public ulong? Size;
        public ulong? FreeSpace;
        public string? EncryptionStatus;
    }

    public static void DisplayVolumesInfo()
    {
        List<VolumeInfo> volumes = GetVolumesInfo();

        foreach (VolumeInfo volume in volumes)
        {
            Console.WriteLine("----Volume status----");
            Console.WriteLine($"Name: {volume.Name}");
            Console.WriteLine($"FileSystem: {volume.FileSystem}");
            Console.WriteLine($"Size: {volume.Size}");
            Console.WriteLine($"FreeSpace: {volume.FreeSpace}");
            Console.WriteLine($"EncryptionStatus: {volume.EncryptionStatus}");
            Console.WriteLine();
        }
    }

    public static List<VolumeInfo> GetVolumesInfo()
    {
        List<VolumeInfo> volumes = new List<VolumeInfo>();
        Dictionary<string, int> volumeIndexes = new Dictionary<string, int>();

        try
        {
            using (ManagementObjectSearcher volumeSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_LogicalDisk"))
            {
                foreach (ManagementObject volumeObj in volumeSearcher.Get())
                {
                    VolumeInfo volume = new VolumeInfo
                    {
                        Name = volumeObj["Name"]?.ToString(),
                        FileSystem = volumeObj["FileSystem"]?.ToString(),
                        Size = ulong.TryParse(volumeObj["Size"]?.ToString(), out ulong size) ? size : (ulong?)null,
                        FreeSpace = ulong.TryParse(volumeObj["FreeSpace"]?.ToString(), out ulong freeSpace) ? freeSpace : (ulong?)null,
                        EncryptionStatus = "N/A"
                    };

                    volumes.Add(volume);
                    volumeIndexes[volume.Name?.TrimEnd('\\') ?? ""] = volumes.Count - 1;
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error getting volume info: {ex.Message}");
            wrc_generator.logs!.LogError($"Error getting volume info: {ex.Message}");
        }

        try
        {
            var searcher = new ManagementObjectSearcher("root\\CIMV2\\Security\\MicrosoftVolumeEncryption", "SELECT * FROM Win32_EncryptableVolume");

            foreach (var o in searcher.Get())
            {
                var queryObj = (ManagementObject)o;
                string? driveLetter = queryObj["DriveLetter"]?.ToString();

                if (!string.IsNullOrEmpty(driveLetter))
                {
                    string trimmedDriveLetter = driveLetter.TrimEnd('\\');

                    if (volumeIndexes.TryGetValue(trimmedDriveLetter, out int index))
                    {
                        uint? protectionStatus = uint.TryParse(queryObj["ProtectionStatus"]?.ToString(), out uint status) ? status : (uint?)null;

                        volumes[index] = new VolumeInfo
                        {
                            Name = volumes[index].Name,
                            FileSystem = volumes[index].FileSystem,
                            Size = volumes[index].Size,
                            FreeSpace = volumes[index].FreeSpace,
                            EncryptionStatus = GetEncryptionStatus(protectionStatus)
                        };
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error getting volume encryption info: {ex.Message}");
            wrc_generator.logs!.LogError($"Error getting volume encryption info: {ex.Message}");
        }

        return volumes;
    }

    private static string GetEncryptionStatus(uint? status)
    {
        return status switch
        {
            0 => "Encryption Disabled",
            1 => "Encryption Enabled",
            2 => "Encryption Enabled, Locked",
            3 => "Encryption Enabled, Unlocked",
            4 => "Encryption Enabled, Auto-unlock Enabled",
            _ => "Unknown"
        };
    }
}