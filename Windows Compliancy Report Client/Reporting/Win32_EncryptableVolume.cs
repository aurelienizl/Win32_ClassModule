﻿#pragma warning disable CA1416 // Valider la compatibilité de la plateforme

using System.Management;

namespace Windows_Compliancy_Report_Client;

internal class Win32_EncryptableVolume
{
    public Win32_EncryptableVolume(string deviceID, string persistentVolumeID, string driveLetter,
        uint protectionStatus)
    {
        GetDeviceID = deviceID;
        GetPersistentVolumeID = persistentVolumeID;
        GetDriveLetter = driveLetter;
        GetProtectionStatus = protectionStatus;
    }

    public string GetDeviceID { get; }

    public string GetPersistentVolumeID { get; }

    public string GetDriveLetter { get; }

    public uint GetProtectionStatus { get; }

    public static List<Win32_EncryptableVolume>? GetEncryptableVolume()
    {
        try
        {
            var list = new List<Win32_EncryptableVolume>();

            var searcher =
                new ManagementObjectSearcher("root\\CIMV2\\Security\\MicrosoftVolumeEncryption",
                    "SELECT * FROM Win32_EncryptableVolume");

            foreach (ManagementObject queryObj in searcher.Get())
                list.Add(
                    new Win32_EncryptableVolume(
                        !string.IsNullOrEmpty((string)queryObj["DeviceID"]) ? (string)queryObj["DeviceID"] : "N/A",
                        !string.IsNullOrEmpty((string)queryObj["PersistentVolumeID"])
                            ? (string)queryObj["PersistentVolumeID"]
                            : "N/A",
                        !string.IsNullOrEmpty((string)queryObj["DriveLetter"])
                            ? (string)queryObj["DriveLetter"]
                            : "N/A",
                        (uint)queryObj["ProtectionStatus"]
                    ));

            return list;
        }
        catch (Exception e)
        {
            Program.window?.Writeline("Bitlocker exception : \n" + e.Message, false);
            return null;
        }
    }
}