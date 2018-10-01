namespace Utilities.ActiveDirectory
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/windows/desktop/ADSchema/a-samaccounttype
    /// </summary>
    public enum AccountType
    {
        Domain = 0x0,
        Group = 0x10000000,
        NonSecurityGroup = 0x10000001,
        Alias = 0x20000000,
        NonSecurityAlias = 0x20000001,
        User = 0x30000000,
        NormalUserAccount = 0x30000000,
        Machine = 0x30000001,
        TrustAccount = 0x30000002,
        AppBasicGroup = 0x40000000,
        AppQueryGroup = 0x40000001,
        Max = 0x7fffffff,
    }
}