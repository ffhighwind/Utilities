namespace Utilities.ActiveDirectory.Interfaces
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/windows/desktop/ADSchema/c-securityprincipal#windows-server-2012-attributes
    /// </summary>
    public interface ISecurityPrincipal
    {
        string AccountNameHistory { get; } // Account-Name-History
        string AltSecurityIdentities { get; } // Alt-Security-Identities
        string MSDSKeyVersionNumber { get; } // ms-DS-KeyVersionNumber
        string ObjectSid { get; } // Object-Sid
        string Rid { get; } // Rid
        string SAMAccountName { get; } // SAM-Account-Name
        string SAMAccountType { get; } // SAM-Account-Type
        string SecurityIdentifier { get; } // Security-Identifier
        string SIDHistory { get; } // SID-History
        string SupplementalCredentials { get; } // Supplemental-Credentials
        string TokenGroups { get; } // Token-Groups
        string TokenGroupsGlobalAndUniversal { get; } // Token-Groups-Global-And-Universal
        string TokenGroupsNoGCAcceptable { get; } // Token-Groups-No-GC-Acceptable
    }
}
