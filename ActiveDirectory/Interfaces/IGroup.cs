namespace Utilities.ActiveDirectory.Interfaces
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/windows/desktop/ADSchema/c-group#windows-server-2012-attributes
    /// </summary>
    public interface IGroup
    {
        string ControlAccessRights { get; } // Control-Access-Rights
        string DesktopProfile { get; } // Desktop-Profile
        string EmailAddresses { get; } // E-mail-Addresses
        string GroupAttributes { get; } // Group-Attributes
        string GroupMembershipSAM { get; } // Group-Membership-SAM
        string GroupType { get; } // Group-Type
        string ManagedBy { get; } // Managed-By
        string Member { get; } // Member
        string MSDSAzApplicationData { get; } // ms-DS-Az-Application-Data
        string MSDSAzBizRule { get; } // ms-DS-Az-Biz-Rule
        string MSDSAzBizRuleLanguage { get; } // ms-DS-Az-Biz-Rule-Language
        string MSDSAzGenericData { get; } // ms-DS-Az-Generic-Data
        string MSDSAzLastImportedBizRulePath { get; } // ms-DS-Az-Last-Imported-Biz-Rule-Path
        string MSDSAzLDAPQuery { get; } // ms-DS-Az-LDAP-Query
        string MSDSAzObjectGuid { get; } // ms-DS-Az-Object-Guid
        string MSDSNonMembers { get; } // ms-DS-Non-Members
        string MSDSPrimaryComputer { get; } // ms-DS-Primary-Computer
        string MSSFU30Name { get; } // msSFU-30-Name
        string MSSFU30NisDomain { get; } // msSFU-30-Nis-Domain
        string MSSFU30PosixMember { get; } // msSFU-30-Posix-Member
        string NonSecurityMember { get; } // Non-Security-Member
        string NTGroupMembers { get; } // NT-Group-Members
        string OperatorCount { get; } // Operator-Count
        string PrimaryGroupToken { get; } // Primary-Group-Token
    }
}
