namespace Utilities.ActiveDirectory
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/windows/desktop/api/lmaccess/ns-lmaccess-_user_info_1008
    /// </summary>
    public enum AccountType : ulong
    {
        Normal = 0x200, // UF_NORMAL_ACCOUNT
        TempDuplicate = 0x100, // UF_TEMP_DUPLICATE_ACCOUNT
        InterDomainTrust = 0x800, // UF_INTERDOMAIN_TRUST_ACCOUNT
        WorkstationTrust = 0x1000, // UF_WORKSTATION_TRUST_ACCOUNT
        ServerTrust = 0x2000, // UF_SERVER_TRUST_ACCOUNT
        MajorityNodeSet = 0x20000 // UF_MNS_LOGON_ACCOUNT
    }
}
