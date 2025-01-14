﻿using System;
using System.Collections.Generic;
using System.Management;
using WCRC_Service.Modules;

#pragma warning disable CA1416 // Valider la compatibilité de la plateforme


internal class Win32_Product
{
    public Win32_Product(string assignmentType, string caption, string description,
        string identifyingNumber, string installDate, string installDate2,
        string installLocation, string installState, string helpLink,
        string helpTelephone, string installSource, string language,
        string localPackage, string name, string packageCache,
        string packageCode, string packageName, string productID,
        string regOwner, string regCompany, string sKUNumber,
        string transforms, string uRLInfoAbout, string uRLUpdateInfo,
        string vendor, string version)
    {
        GetAssignmentType = assignmentType;
        GetCaption = caption;
        GetDescription = description;
        GetIdentifyingNumber = identifyingNumber;
        GetInstallDate = installDate;
        GetInstallDate2 = installDate2;
        GetInstallLocation = installLocation;
        GetInstallState = installState;
        GetHelpLink = helpLink;
        GetHelpTelephone = helpTelephone;
        GetInstallSource = installSource;
        GetLanguage = language;
        GetLocalPackage = localPackage;
        GetName = name;
        GetPackageCache = packageCache;
        GetPackageCode = packageCode;
        GetPackageName = packageName;
        GetProductID = productID;
        GetRegOwner = regOwner;
        GetRegCompany = regCompany;
        GetSKUNumber = sKUNumber;
        GetTransforms = transforms;
        GetURLInfoAbout = uRLInfoAbout;
        GetURLUpdateInfo = uRLUpdateInfo;
        GetVendor = vendor;
        GetVersion = version;
    }

    public string GetAssignmentType { get; }

    public string GetCaption { get; }

    public string GetDescription { get; }

    public string GetIdentifyingNumber { get; }

    public string GetInstallDate { get; }

    public string GetInstallDate2 { get; }

    public string GetInstallLocation { get; }

    public string GetInstallState { get; }

    public string GetHelpLink { get; }

    public string GetHelpTelephone { get; }

    public string GetInstallSource { get; }

    public string GetLanguage { get; }

    public string GetLocalPackage { get; }

    public string GetName { get; }

    public string GetPackageCache { get; }

    public string GetPackageCode { get; }

    public string GetPackageName { get; }

    public string GetProductID { get; }

    public string GetRegOwner { get; }

    public string GetRegCompany { get; }

    public string GetSKUNumber { get; }

    public string GetTransforms { get; }

    public string GetURLInfoAbout { get; }

    public string GetURLUpdateInfo { get; }

    public string GetVendor { get; }

    public string GetVersion { get; }

    public static string QuerySafeGetter(ManagementObject obj, string query)
    {
        try
        {
            if (!string.IsNullOrEmpty(obj[query].ToString())) return obj[query].ToString();

            var res = (string)obj[query];
            if (!string.IsNullOrEmpty(res)) return res;
            return "N/A";
        }
        catch (Exception)
        {
            return "N/A";
        }
    }

    public static List<Win32_Product> GetProducts()
    {
        var products = new List<Win32_Product>();

        try
        {
            var searcher =
                new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_Product");

            foreach (var o in searcher.Get())
            {
                var queryObj = (ManagementObject)o;
                try
                {
                    var win32Products =
                        new Win32_Product(
                            QuerySafeGetter(queryObj, "AssignmentType"),
                            QuerySafeGetter(queryObj, "Caption"),
                            QuerySafeGetter(queryObj, "Description"),
                            QuerySafeGetter(queryObj, "IdentifyingNumber"),
                            QuerySafeGetter(queryObj, "InstallDate"),
                            QuerySafeGetter(queryObj, "InstallDate2"),
                            QuerySafeGetter(queryObj, "InstallLocation"),
                            QuerySafeGetter(queryObj, "InstallState"),
                            QuerySafeGetter(queryObj, "HelpLink"),
                            QuerySafeGetter(queryObj, "HelpTelephone"),
                            QuerySafeGetter(queryObj, "InstallSource"),
                            QuerySafeGetter(queryObj, "Language"),
                            QuerySafeGetter(queryObj, "LocalPackage"),
                            QuerySafeGetter(queryObj, "Name"),
                            QuerySafeGetter(queryObj, "PackageCache"),
                            QuerySafeGetter(queryObj, "PackageCode"),
                            QuerySafeGetter(queryObj, "PackageName"),
                            QuerySafeGetter(queryObj, "ProductID"),
                            QuerySafeGetter(queryObj, "RegOwner"),
                            QuerySafeGetter(queryObj, "RegCompany"),
                            QuerySafeGetter(queryObj, "SKUNumber"),
                            QuerySafeGetter(queryObj, "Transforms"),
                            QuerySafeGetter(queryObj, "URLInfoAbout"),
                            QuerySafeGetter(queryObj, "URLUpdateInfo"),
                            QuerySafeGetter(queryObj, "Vendor"),
                            QuerySafeGetter(queryObj, "Version")
                        );

                    products.Add(win32Products);
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            Logs.LogWrite("Got products successfully");

            return products;
        }
        catch (Exception)
        {
            Logs.LogWrite("Error : products");

            return products;
        }
    }
}