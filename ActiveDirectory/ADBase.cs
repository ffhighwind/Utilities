using System.Collections;
using System.DirectoryServices;

namespace Utilities.ActiveDirectory
{
    /// <summary>
    /// Top class
    /// https://docs.microsoft.com/en-us/windows/desktop/ADSchema/c-top#windows-server-2012-attributes
    /// </summary>
    public abstract class ADBase
    {
        //NT-Security-Descriptor
        //Object-Category
        //Object-Class
        //Instance-Type

        public DirectoryEntry DirectoryEntry { get; protected set; }

        public object this[string property] {
            get {
                if (DirectoryEntry.Properties.Contains(property))
                    return DirectoryEntry.Properties[property].Value;
                return null;
            }
        }

        public ICollection PropertyNames => DirectoryEntry.Properties.PropertyNames;

        public string AdminDescription => this["adminDescription"]?.ToString();

        public string AdminDisplayName => this["adminDisplayName"]?.ToString();

        public string AllowedAttributes => this["allowedAttributes"]?.ToString();

        public string AllowedAttributesEffective => this["allowedAttributesEffective"]?.ToString();

        public string AllowedChildClasses => this["allowedChildClasses"]?.ToString();

        public string AllowedChildClassesEffective => this["allowedChildClasses"]?.ToString();

        public string BridgeheadServerListBL => this["bridgeheadServerListBL"]?.ToString();

        public string CanonicalName => this["canonicalName"]?.ToString();

        public string CommonName => this["cn"]?.ToString();

        public string CreateTimeStamp => this["createTimeStamp"]?.ToString();

        public string Description => this["description"]?.ToString();

        public string DisplayName => this["displayName"]?.ToString();

        public string DisplayNamePrintable => this["displayNamePrintable"]?.ToString();

        public string DSASignature => this["dSASignature"]?.ToString();

        public string DSCorePropagationData => this["dSCorePropagationData"]?.ToString();

        public string ExtensionName => this["extensionName"]?.ToString();

        public int? Flags => (int?) this["flags"];

        public bool? FromEntry => (bool?) this["fromEntry"]; // true = object is writable, false = readonly

        public string FRSComputerReferenceBL => this["frsComputerReferenceBL"]?.ToString();

        public string FRSMemberReferenceBL => this["fRSMemberReferenceBL"]?.ToString();

        public string FSMORoleOwner => this["fSMORoleOwner"]?.ToString();

        public InstanceType InstanceType => new InstanceType((int) this["instanceType"]);

        public bool? IsCriticalSystemObject => (bool?) this["isCriticalSystemObject"];

        public bool? IsDeleted => (bool?) this["isDeleted"];

        public string MemberOf => this["memberOf"]?.ToString();

        public string IsPrivilegeHolder => this["isPrivilegeHolder"]?.ToString();

        public bool? IsRecycled => (bool?) this["isRecycled"];

        public string LastKnownParent => this["lastKnownParent"]?.ToString();

        public string ManagedObjects => this["managedObjects"]?.ToString();

        public string MasteredBy => this["masteredBy"]?.ToString();

        public string ModifyTimeStamp => this["modifyTimeStamp"]?.ToString();

        public string MSCOMPartitionSetLink => this["msCOM-PartitionSetLink"]?.ToString();

        public string MSCOMUserLink => this["msCOM-UserLink"]?.ToString();

        public string MSDFSRComputerReferenceBL => this["msDFSR-ComputerReferenceBL"]?.ToString();

        public string MSDFSRMemberReferenceBL => this["msDFSR-MemberReferenceBL"]?.ToString();

        public int? MSDSApproxImmedSubordinates => (int?) this["msDS-Approx-Immed-Subordinates"];

        public string MSDSAuthenticatedToAccountlist => this["msDS-AuthenticatedToAccountlist"]?.ToString();

        public string MSDSClaimSharesPossibleValuesWithBL => this["msDS-ClaimSharesPossibleValuesWithBL"]?.ToString();

        public int? MSDSConsistencyChildCount => (int?) this["mS-DS-ConsistencyChildCount"];

        public string MSDSConsistencyGuid => this["mS-DS-ConsistencyGuid"]?.ToString();

        public string MSDSEnabledFeatureBL => this["msDS-EnabledFeatureBL"]?.ToString();

        public string MSDSHostServiceAccountBL => this["msDS-HostServiceAccountBL"]?.ToString();

        public string MSDSIsDomainFor => this["msDS-IsDomainFor"]?.ToString();

        public string MSDSIsFullReplicaFor => this["msDS-IsFullReplicaFor"]?.ToString();

        public string MSDSIsPartialReplicaFor => this["msDS-IsPartialReplicaFor"]?.ToString();

        public string MSDSIsPrimaryComputerFor => this["msDS-IsPrimaryComputerFor"]?.ToString();

        public string MSDSKrbTgtLinkBL => this["msDS-KrbTgtLinkBl"]?.ToString();

        public string MSDSLastKnownRDN => this["msDS-LastKnownRDN"]?.ToString();

        public string MSDSLocalEffectiveDeletionTime => this["msDS-LocalEffectiveDeletionTime"]?.ToString();

        public string MSDSLocalEffectiveRecycleTime => this["msDS-LocalEffectiveRecycleTime"]?.ToString();

        public string MSDsMasteredBy => this["msDs-masteredBy"]?.ToString();

        public string MSDSMembersForAzRoleBL => this["msDS-MembersForAzRoleBL"]?.ToString();

        public string MSDSMembersOfResourcePropertyListBL => this["msDS-MembersOfResourcePropertyListBL"]?.ToString();

        public string MSDSNCReplCursors => this["msDS-NCReplCursors"]?.ToString();

        public string MSDSNCReplInboundNeighbors => this["msDS-NCReplInboundNeighbors"]?.ToString();

        public string MSDSNCReplOutboundNeighbors => this["msDS-NCReplOutboundNeighbors"]?.ToString();

        public string MSDSNCROReplicaLocationsBL => this["msDS-NC-RO-Replica-Locations-BL"]?.ToString();

        public int? MSDSNcType => (int?) this["msDS-NcType"];

        public string MSDSNonMembersBL => this["msDS-NonMembersBL"]?.ToString();

        public string MSDSObjectReferenceBL => this["msDS-ObjectReferenceBL"]?.ToString();

        public string MSDSOperationsForAzRoleBL => this["msDS-OperationsForAzRoleBL"]?.ToString();

        public string MSDSOperationsForAzTaskBL => this["msDS-OperationsForAzTaskBL"]?.ToString();

        public string MSDSPrincipalName => this["msDS-PrincipalName"]?.ToString();

        public string MSDSPSOApplied => this["msDS-PSOApplied"]?.ToString();

        public string MSDSReplAttributeMetaData => this["msDS-ReplAttributeMetaData"]?.ToString();

        public string MSDSReplValueMetaData => this["msDS-ReplValueMetaData"]?.ToString();

        public string MSDSRevealedDSAs => this["msDS-RevealedDSAs"]?.ToString();

        public string MSDSRevealedListBL => this["msDS-RevealedListBL"]?.ToString();

        public string MSDSTasksForAzRoleBL => this["msDS-TasksForAzRoleBL"]?.ToString();

        public string MSDSTasksForAzTaskBL => this["msDS-TasksForAzTaskBL"]?.ToString();

        public string MSDSTDOEgressBL => this["msDS-TDOEgressBL"]?.ToString();

        public string MSDSTDOIngressBL => this["msDS-TDOIngressBL"]?.ToString();

        public string MSDSValueTypeReferenceBL => this["msDS-ValueTypeReferenceBL"]?.ToString();

        public string MSExchOwnerBL => this["ownerBL"]?.ToString();

        public string MSSFU30PosixMemberOf => this["msSFU30PosixMemberOf"]?.ToString();

        public string NetbootSCPBL => this["netbootSCPBL"]?.ToString();

        public string NonSecurityMemberBL => this["nonSecurityMemberBL"]?.ToString();

        public string NTSecurityDescriptor => this["nTSecurityDescriptor"].ToString();

        public string DistinguishedName => this["distinguishedName"]?.ToString();

        public string ObjectCategory => this["objectCategory"].ToString();

        public string ObjectClass => this["objectClass"].ToString();

        public string ObjectGUID => this["objectGUID"]?.ToString();

        public int? ObjectVersion => (int?) this["objectVersion"];

        public string OtherWellKnownObjects => this["otherWellKnownObjects"]?.ToString();

        public string PartialAttributeDeletionList => this["partialAttributeDeletionList"]?.ToString();

        public string PartialAttributeSet => this["partialAttributeSet"]?.ToString();

        public string PossibleInferiors => this["possibleInferiors"]?.ToString();

        public string ProxiedObjectName => this["proxiedObjectName"]?.ToString();

        public string ProxyAddresses => this["proxyAddresses"]?.ToString();

        public string QueryPolicyBL => this["queryPolicyBL"]?.ToString();

        public string Name => this["name"]?.ToString(); // Relative Distinguished Name

        public string ReplPropertyMetaData => this["replPropertyMetaData"]?.ToString();

        public string ReplUpToDateVector => this["replUpToDateVector"]?.ToString();

        public string DirectReports => this["directReports"]?.ToString();

        public string RepsFrom => this["repsFrom"]?.ToString();

        public string RepsTo => this["repsTo"]?.ToString();

        public int? Revision => (int?) this["revision"];

        public EffectiveRights SDRightsEffective {
            get {
                int? value = (int?) this["sDRightsEffective"];
                return value == null ? null : new EffectiveRights((int) value);
            }
        }

        public string ServerReferenceBL => this["serverReferenceBL"]?.ToString();

        public bool? ShowInAdvancedViewOnly => (bool?) this["showInAdvancedViewOnly"];

        public string SiteObjectBL => this["siteObjectBL"]?.ToString();

        public string StructuralObjectClass => this["structuralObjectClass"]?.ToString();

        public string SubRefs => this["subRefs"]?.ToString();

        public string SubSchemaSubEntry => this["subSchemaSubEntry"]?.ToString();

        public SystemFlags SystemFlags {
            get {
                int? value = (int?) this["systemFlags"];
                return value == null ? null : new SystemFlags((int) value);
            }
        }

        public string USNChanged => this["uSNChanged"]?.ToString(); // TimeSpan

        public string USNCreated => this["uSNCreated"]?.ToString(); // TimeSpan

        public string USNDSALastObjRemoved => this["uSNDSALastObjRemoved"]?.ToString(); // TimeSpan

        public int? USNIntersite => (int?) this["USNIntersite"];

        public string USNLastObjRem => this["uSNLastObjRem"]?.ToString();

        public string USNSource => this["uSNSource"]?.ToString(); // TimeSpan

        public string WbemPath => this["wbemPath"]?.ToString();

        public string WellKnownObjects => this["wellKnownObjects"]?.ToString();

        public string WhenChanged => this["whenChanged"]?.ToString(); // DateTime

        public string WhenCreated => this["whenCreated"]?.ToString(); // DateTime

        public string WWWHomePage => this["wWWHomePage"]?.ToString();

        public string Url => this["url"]?.ToString();
    }
}
