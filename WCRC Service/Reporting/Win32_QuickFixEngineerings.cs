﻿using System;
using System.Collections.Generic;
using System.Management;
using WCRC_Core.Reporting;

internal class Win32_QuickFixEngineerings
{
    public Win32_QuickFixEngineerings(string caption, string description, string installDate,
        string name, string status, string cSName, string fixComments, string hotFixID,
        string installedBy, string installedOn, string servicePackInEffect)
    {
        GetCaption = caption;
        GetDescription = description;
        GetInstallDate = installDate;
        GetName = name;
        GetStatus = status;
        GetCSName = cSName;
        GetFixComments = fixComments;
        GetHotFixID = hotFixID;
        GetInstalledBy = installedBy;
        GetInstalledOn = installedOn;
        GetServicePackInEffect = servicePackInEffect;
    }

    public string GetCaption { get; }

    public string GetDescription { get; }

    public string GetInstallDate { get; }

    public string GetName { get; }

    public string GetStatus { get; }

    public string GetCSName { get; }

    public string GetFixComments { get; }

    public string GetHotFixID { get; }

    public string GetInstalledBy { get; }

    public string GetInstalledOn { get; }

    public string GetServicePackInEffect { get; }

    public static string QuerySafeGetter(ManagementObject obj, string query)
    {
        try
        {
            string res = (string)obj[query];
            if (!String.IsNullOrEmpty(res))
            {
                return res;
            }
            return "N/A";
        }
        catch (Exception)
        {
            return "N/A";
        }
    }

    public static List<Win32_QuickFixEngineerings> GetQuickFixEngineering()
    {
        try
        {
            var list = new List<Win32_QuickFixEngineerings>();

            var searcher =
                new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_QuickFixEngineering");

            foreach (ManagementObject queryObj in searcher.Get())
            {
                try
                {
                    list.Add(
                        new Win32_QuickFixEngineerings(
                            QuerySafeGetter(queryObj, "Caption"),
                            QuerySafeGetter(queryObj, "Description"),
                            QuerySafeGetter(queryObj, "InstallDate"),
                            QuerySafeGetter(queryObj, "Name"),
                            QuerySafeGetter(queryObj, "Status"),
                            QuerySafeGetter(queryObj, "CSName"),
                            QuerySafeGetter(queryObj, "FixComments"),
                            QuerySafeGetter(queryObj, "HotFixID"),
                            QuerySafeGetter(queryObj, "InstalledBy"),
                            QuerySafeGetter(queryObj, "InstalledOn"),
                            QuerySafeGetter(queryObj, "ServicePackInEffect")                        
                        )
                    );
                }
                catch (Exception ex)
                {
                    WCRC.log.LogWrite("Internal error on qfe...");
                    WCRC.log.LogWrite(ex.Message);
                    WCRC.Win32_Error_.QuickFixEngineerings_error += 1;

                }
            }

            return list;
        }
        catch (Exception ex)
        {
            WCRC.log.LogWrite("Critical error on qfe...");
            WCRC.log.LogWrite(ex.Message);
            WCRC.Win32_Error_.Critical_QuickFixEngineerings_error += 1;

            return null;
        }
    }
}
