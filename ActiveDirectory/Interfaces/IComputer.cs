namespace Utilities.ActiveDirectory.Interfaces
{
    /// <summary>
    /// https:string docs { get; } // docs.microsoft.com/en-us/windows/desktop/ADSchema/c-computer#windows-server-2012-attributes
    /// </summary>
    public interface IComputer
    {
        string Catalogs { get; } // Catalogs
        string DefaultLocalPolicyObject { get; } // Default-Local-Policy-Object
        string DNSHostName { get; } // DNS-Host-Name
        string LocalPolicyFlags { get; } // Local-Policy-Flags
        string Location { get; } // Location
        string MachineRole { get; } // Machine-Role
        string ManagedBy { get; } // Managed-By
        string MSDSAdditionalDnsHostName { get; } // ms-DS-Additional-Dns-Host-Name
        string MSDSAdditionalSamAccountName { get; } // ms-DS-Additional-Sam-Account-Name
        string MSDSAuthenticatedAtDC { get; } // ms-DS-AuthenticatedAt-DC
        string MSDSExecuteScriptPassword { get; } // ms-DS-ExecuteScriptPassword
        string MSDSGenerationId { get; } // ms-DS-Generation-Id
        string MSDSHostServiceAccount { get; } // ms-DS-Host-Service-Account
        string MSDSisGC { get; } // ms-DS-isGC
        string MSDSisRODC { get; } // ms-DS-isRODC
        string MSDSIsUserCachableAtRodc { get; } // ms-DS-Is-User-Cachable-At-Rodc
        string MSDSKrbTgtLink { get; } // ms-DS-KrbTgt-Link
        string MSDSNeverRevealGroup { get; } // ms-DS-Never-Reveal-Group
        string MSDSPromotionSettings { get; } // ms-DS-Promotion-Settings
        string MSDSRevealedList { get; } // ms-DS-Revealed-List
        string MSDSRevealedUsers { get; } // ms-DS-Revealed-Users
        string MSDSRevealOnDemandGroup { get; } // ms-DS-Reveal-OnDemand-Group
        string MSDSSiteName { get; } // ms-DS-SiteName
        string MSImagingHashAlgorithm { get; } // ms-Imaging-Hash-Algorithm
        string MSImagingThumbprintHash { get; } // ms-Imaging-Thumbprint-Hash
        string MSSFU30Aliases { get; } // msSFU-30-Aliases
        string MSSFU30Name { get; } // msSFU-30-Name
        string MSSFU30NisDomain { get; } // msSFU-30-Nis-Domain
        string MSTPMOwnerInformation { get; } // ms-TPM-OwnerInformation
        string MSTPMTpmInformationForComputer { get; } // ms-TPM-Tpm-Information-For-Computer
        string MSTSEndpointData { get; } // ms-TS-Endpoint-Data
        string MSTSEndpointPlugin { get; } // ms-TS-Endpoint-Plugin
        string MSTSEndpointType { get; } // ms-TS-Endpoint-Type
        string MSTSPrimaryDesktopBL { get; } // ms-TS-Primary-Desktop-BL
        string MSTSProperty01 { get; } // MS-TS-Property01
        string MSTSProperty02 { get; } // MS-TS-Property02
        string MSTSSecondaryDesktopBL { get; } // ms-TS-Secondary-Desktop-BL
        string NetbootGUID { get; } // Netboot-GUID
        string NetbootInitialization { get; } // Netboot-Initialization
        string NetbootMachineFilePath { get; } // Netboot-Machine-File-Path
        string NetbootMirrorDataFile { get; } // Netboot-Mirror-Data-File
        string NetbootSIFFile { get; } // Netboot-SIF-File
        string NetworkAddress { get; } // Network-Address
        string NisMapName { get; } // nisMapName
        string OperatingSystem { get; } // Operating-System
        string OperatingSystemHotfix { get; } // Operating-System-Hotfix
        string OperatingSystemServicePack { get; } // Operating-System-Service-Pack
        string OperatingSystemVersion { get; } // Operating-System-Version
        string PhysicalLocationObject { get; } // Physical-Location-Object
        string PolicyReplicationFlags { get; } // Policy-Replication-Flags
        string RIDSetReferences { get; } // RID-Set-References
        string SiteGUID { get; } // Site-GUID
        string VolumeCount { get; } // Volume-Count
    }
}
