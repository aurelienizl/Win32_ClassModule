﻿using Microsoft.Win32;

namespace Windows_Compliancy_Report_Client.Reporting;

internal class Startup
{
    public Startup(string? name)
    {
        Name = name;
    }

    public string? Name { get; }

    public static List<Startup>? GetStartupApps()
    {
        var apps = new List<Startup>();

        try
        {
            foreach (var val in StartupFolder()) apps.Add(val);
            foreach (var val in RegistryChecks(@"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce",
                         RegistryHive.Users)) apps.Add(val);
            foreach (var val in RegistryChecks(@"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce",
                         RegistryHive.LocalMachine))
                apps.Add(val);
            foreach (var val in RegistryChecks(@"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run",
                         RegistryHive.Users)) apps.Add(val);
            foreach (var val in RegistryChecks(@"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run",
                         RegistryHive.LocalMachine))
                apps.Add(val);
            return apps;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static List<Startup> StartupFolder()
    {
        if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup))) return new List<Startup>();
        var files = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Startup)).GetFiles();
        return Convert(files.Where(info => info.Name != "desktop.ini").Select(info => info.Name).ToList());
    }

    private static List<Startup> RegistryChecks(string key, RegistryHive hive)
    {
        using var startupKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry32)
            .OpenSubKey(key);
        var valueNames = startupKey?.GetValueNames();
        if (valueNames is null) return new List<Startup>();
        return Convert(valueNames.ToList());
    }

    private static List<Startup> Convert(List<string> list)
    {
        var res = new List<Startup>();
        foreach (var val in list) res.Add(new Startup(val));
        return res;
    }
}