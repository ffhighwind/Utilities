using System;
using System.DirectoryServices.AccountManagement;

namespace Utilities.ActiveDirectory
{
    public class ADComputer : ADUser, IComputer, IDisposable
    {
        public ADComputer() : base() { }

        public ADComputer(ComputerPrincipal principal) : base()
        {
            Principal = principal;
        }

        //ipHost

        //Catalogs
        //Default-Local-Policy-Object
        //DNS-Host-Name
        //ipHostNumber
        //Local-Policy-Flags
        //Location
        //Machine-Role
        //Managed-By
        //ms-DS-Additional-Dns-Host-Name
        //ms-DS-Additional-Sam-Account-Name
        //ms-DS-ExecuteScriptPassword
        //ms-DS-Generation-Id
        //ms-DS-Host-Service-Account
        //ms-DS-isGC
        //ms-DS-isRODC
        //ms-DS-Is-User-Cachable-At-Rodc
        //ms-DS-KrbTgt-Link
        //ms-DS-Never-Reveal-Group
        //ms-DS-Promotion-Settings
        //ms-DS-Revealed-List
        //ms-DS-Revealed-Users
        //ms-DS-Reveal-OnDemand-Group
        //ms-DS-SiteName
        //ms-Imaging-Hash-Algorithm
        //ms-Imaging-Thumbprint-Hash
        //msSFU-30-Aliases
        //ms-TPM-OwnerInformation
        //ms-TPM-Tpm-Information-For-Computer
        //ms-TS-Endpoint-Data
        //ms-TS-Endpoint-Plugin
        //ms-TS-Endpoint-Type
        //ms-TS-Primary-Desktop-BL
        //ms-TS-Secondary-Desktop-BL
        //Netboot-GUID
        //Netboot-Initialization
        //Netboot-Machine-File-Path
        //Netboot-Mirror-Data-File
        //Netboot-SIF-File
        //nisMapName
        //Operating-System
        //Operating-System-Hotfix
        //Operating-System-Service-Pack
        //Operating-System-Version
        //Physical-Location-Object
        //Policy-Replication-Flags
        //RID-Set-References
        //Site-GUID
        //Volume-Count
    }
}
}
