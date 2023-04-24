using System.DirectoryServices.AccountManagement;
using System.Security.Principal;
using System.Collections.Concurrent;

class wrs_accounts
{
    public struct UserAccount
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string SamAccountName { get; set; }
        public string Email { get; set; }
        public DateTime? LastLogon { get; set; }
        public bool Enabled { get; set; }
        public bool IsAdministrator { get; set; }
    }

    public static void DisplayUsers()
    {
        List<UserAccount> users = GetAccountsInfo();

        foreach (UserAccount user in users)
        {
            Console.WriteLine("----User Account----");
            Console.WriteLine($"Name: {user.Name}");
            Console.WriteLine($"Display Name: {user.DisplayName}");
            Console.WriteLine($"SamAccountName: {user.SamAccountName}");
            Console.WriteLine($"Email: {user.Email}");
            Console.WriteLine($"Last Logon: {user.LastLogon}");
            Console.WriteLine($"Enabled: {user.Enabled}");
            Console.WriteLine($"Is Administrator: {user.IsAdministrator}");
            Console.WriteLine();
        }
    }

    public static List<UserAccount> GetAccountsInfo()
{
    ConcurrentBag<UserAccount> users = new ConcurrentBag<UserAccount>();

    try
    {
        using (var ctx = new PrincipalContext(ContextType.Machine))
        {
            using (var userPrincipal = new UserPrincipal(ctx))
            {
                using (var searcher = new PrincipalSearcher(userPrincipal))
                {
                    var results = searcher.FindAll();

                    Parallel.ForEach(results, p =>
                    {
                        var user = (UserPrincipal)p;

                        users.Add(new UserAccount
                        {
                            Name = user.Name,
                            DisplayName = user.DisplayName,
                            SamAccountName = user.SamAccountName,
                            Email = user.EmailAddress,
                            LastLogon = user.LastLogon,
                            Enabled = user.Enabled ?? false,
                            IsAdministrator = IsUserAdministrator(user)
                        });
                    });
                }
            }
        }
    }
    catch (Exception)
    {
        Console.Error.WriteLine("Failed to get user accounts info.");
    }

    return users.ToList();
}

    private static bool IsUserAdministrator(UserPrincipal user)
    {
        try
        {
            // Well-known SID for the local Administrators group
            SecurityIdentifier adminGroupSid = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
            using (var ctx = new PrincipalContext(ContextType.Machine))
            {
                using (var groupPrincipal = GroupPrincipal.FindByIdentity(ctx, IdentityType.Sid, adminGroupSid.ToString()))
                {
                    if (groupPrincipal != null)
                    {
                        return groupPrincipal.Members.Contains(user);
                    }
                }
            }
        }
        catch (Exception)
        {
            Console.Error.WriteLine("Failed to check if user is administrator.");
        }
        return false;
    }
}