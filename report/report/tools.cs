using System.Security.Cryptography;
using System.Text;
using System.Management;
internal static class Cryptography
{
    public static string ComputeSha256Hash(string rawData)
    {
        // Create a SHA256   
        using (var sha256Hash = SHA256.Create())
        {
            // ComputeHash - returns byte array  
            var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

            // Convert byte array to a string   
            var builder = new StringBuilder();
            for (var i = 0; i < bytes.Length; i++) builder.Append(bytes[i].ToString("x2"));
            return builder.ToString();
        }
    }
}

internal static class QueryManagement
{
    public static string QuerySafeGetter(ManagementObject obj, string query)
    {
        try
        {
            object result = obj[query];
            return result?.ToString() ?? "N/A";
        }
        catch (Exception)
        {
            Console.Error.WriteLine($"Failed to query {query}.");
            return "N/A";
        }
    }

}