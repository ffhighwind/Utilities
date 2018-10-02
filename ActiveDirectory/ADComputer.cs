using System;
using System.DirectoryServices.AccountManagement;
using Utilities.ActiveDirectory.Interfaces;

namespace Utilities.ActiveDirectory
{
    public class ADComputer : ADUser, IComputer, IDisposable
    {
        public ADComputer() : base() { }

        public ADComputer(ComputerPrincipal principal) : base()
        {
            Principal = principal;
        }

        string IComputer.Catalogs => throw new NotImplementedException();

        string IComputer.DefaultLocalPolicyObject => throw new NotImplementedException();

        string IComputer.DNSHostName => throw new NotImplementedException();

        string IComputer.LocalPolicyFlags => throw new NotImplementedException();

        string IComputer.Location => throw new NotImplementedException();

        string IComputer.MachineRole => throw new NotImplementedException();

        string IComputer.ManagedBy => throw new NotImplementedException();

        string IComputer.MSDSAdditionalDnsHostName => throw new NotImplementedException();

        string IComputer.MSDSAdditionalSamAccountName => throw new NotImplementedException();

        string IComputer.MSDSAuthenticatedAtDC => throw new NotImplementedException();

        string IComputer.MSDSExecuteScriptPassword => throw new NotImplementedException();

        string IComputer.MSDSGenerationId => throw new NotImplementedException();

        string IComputer.MSDSHostServiceAccount => throw new NotImplementedException();

        string IComputer.MSDSisGC => throw new NotImplementedException();

        string IComputer.MSDSisRODC => throw new NotImplementedException();

        string IComputer.MSDSIsUserCachableAtRodc => throw new NotImplementedException();

        string IComputer.MSDSKrbTgtLink => throw new NotImplementedException();

        string IComputer.MSDSNeverRevealGroup => throw new NotImplementedException();

        string IComputer.MSDSPromotionSettings => throw new NotImplementedException();

        string IComputer.MSDSRevealedList => throw new NotImplementedException();

        string IComputer.MSDSRevealedUsers => throw new NotImplementedException();

        string IComputer.MSDSRevealOnDemandGroup => throw new NotImplementedException();

        string IComputer.MSDSSiteName => throw new NotImplementedException();

        string IComputer.MSImagingHashAlgorithm => throw new NotImplementedException();

        string IComputer.MSImagingThumbprintHash => throw new NotImplementedException();

        string IComputer.MSSFU30Aliases => throw new NotImplementedException();

        string IComputer.MSSFU30Name => throw new NotImplementedException();

        string IComputer.MSSFU30NisDomain => throw new NotImplementedException();

        string IComputer.MSTPMOwnerInformation => throw new NotImplementedException();

        string IComputer.MSTPMTpmInformationForComputer => throw new NotImplementedException();

        string IComputer.MSTSEndpointData => throw new NotImplementedException();

        string IComputer.MSTSEndpointPlugin => throw new NotImplementedException();

        string IComputer.MSTSEndpointType => throw new NotImplementedException();

        string IComputer.MSTSPrimaryDesktopBL => throw new NotImplementedException();

        string IComputer.MSTSProperty01 => throw new NotImplementedException();

        string IComputer.MSTSProperty02 => throw new NotImplementedException();

        string IComputer.MSTSSecondaryDesktopBL => throw new NotImplementedException();

        string IComputer.NetbootGUID => throw new NotImplementedException();

        string IComputer.NetbootInitialization => throw new NotImplementedException();

        string IComputer.NetbootMachineFilePath => throw new NotImplementedException();

        string IComputer.NetbootMirrorDataFile => throw new NotImplementedException();

        string IComputer.NetbootSIFFile => throw new NotImplementedException();

        string IComputer.NetworkAddress => throw new NotImplementedException();

        string IComputer.NisMapName => throw new NotImplementedException();

        string IComputer.OperatingSystem => throw new NotImplementedException();

        string IComputer.OperatingSystemHotfix => throw new NotImplementedException();

        string IComputer.OperatingSystemServicePack => throw new NotImplementedException();

        string IComputer.OperatingSystemVersion => throw new NotImplementedException();

        string IComputer.PhysicalLocationObject => throw new NotImplementedException();

        string IComputer.PolicyReplicationFlags => throw new NotImplementedException();

        string IComputer.RIDSetReferences => throw new NotImplementedException();

        string IComputer.SiteGUID => throw new NotImplementedException();

        string IComputer.VolumeCount => throw new NotImplementedException();
    }
}
