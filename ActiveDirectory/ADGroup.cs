using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

namespace Utilities.ActiveDirectory
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/windows/desktop/ADSchema/c-group#windows-server-2012-attributes
    /// </summary>
    public class ADGroup : ADBase, IGroup, ISecurityPrincipal, IMailRecipient
    {
        public ADGroup(GroupPrincipal principal)
        {
            Principal = principal;
            DirectoryEntry = (DirectoryEntry) principal.GetUnderlyingObject();
        }

        private GroupPrincipal principal;

        public GroupPrincipal Principal {
            get => principal;
            set {
                principal = value;
                DirectoryEntry = (DirectoryEntry) value.GetUnderlyingObject();
            }
        }

        //"accountNameHistory",
        //"adminCount",
        //"adminDescription",
        //"adminDisplayName",
        //"allowedAttributes",
        //"allowedAttributesEffective",
        //"allowedChildClasses"
        //"altSecurityIdentities"
        //"bridgeheadServerListBL"
        //"canonicalName"
        //"info" // Comment
        //"cn" // Common-Name
        //"controlAccessRights"
        //"createTimeStamp"
        //"description"
        //"desktopProfile"
        //"displayName"
        //"displayNamePrintable"
        //"dSASignature"
        //"dSCorePropagationData"
        //"mail"
        //"extensionName"
        //"flags"
        //"fromEntry"
        //"frsComputerReferenceBL"
        //"fRSMemberReferenceBL"
        //"fSMORoleOwner"
        //"garbageCollPeriod"
        //"gidNumber
        //"groupAttributes"
        //"groupMembershipSAM"
        //"groupType"
        //"instanceType"
        //"isCriticalSystemObject"
        //"isDeleted"
        //"memberOf"
        //"isPrivilegeHolder"
        //"isRecycled"
        //"labeledURI"
        //"lastKnownParent"
        //"legacyExchangeDN"
        //"managedBy"
        //"managedObjects"
        //"managedBy"
        //"member"
        //"memberUid"
        //"modifyTimeStamp"

        //"msCOM-PartitionSetLink"
        //"msCOM-UserLink"
        //"msDFSR-ComputerReferenceBL"
        //"msDFSR-MemberReferenceBL"
        //"msDS-Approx-Immed-Subordinates"
        //"msDS-AuthenticatedToAccountlist"
        //"msDS-AzApplicationData"
        //"msDS-AzBizRule"
        //"msDS-AzBizRuleLanguage"
        //"msDS-AzGenericData"
        //"msDS-AzLastImportedBizRulePath"

        //"msDS-AzLDAPQuery"
        //"msDS-AzObjectGuid"
        //"msDS-ClaimSharesPossibleValuesWithBL"
        //"mS-DS-ConsistencyChildCount"
        //"mS-DS-ConsistencyGuid"
        //"msDS-EnabledFeatureBL"
        //"msDS-GeoCoordinatesAltitude"
        //"msDS-GeoCoordinatesLatitude"
        //"msDS-GeoCoordinatesLongitude"
        //"msDS-HostServiceAccountBL"
        //"msDS-IsDomainFor"
        //"msDS-IsPartialReplicaFor"
        //"msDS-IsPartialReplicaFor"
        //"msDS-IsPrimaryComputerFor"
        //"msDS-KeyVersionNumber"
        //"msDS-KrbTgtLinkBl"
        //"msDS-LastKnownRDN"
        //"msDS-LocalEffectiveDeletionTime"
        //"msDS-LocalEffectiveRecycleTime"
        //"msDs-masteredBy"
        //"msDS-MembersForAzRoleBL"
        //"msDS-MembersOfResourcePropertyListBL"
        //"msDS-NCReplCursors"
        //"msDS-NCReplInboundNeighbors"
        //"msDS-NCReplOutboundNeighbors"
        //"msDS-NC-RO-Replica-Locations-BL"
        //"msDS-NcType"
        //"msDS-NonMembers"
        //"msDS-NonMembersBL"
        //"msDS-ObjectReferenceBL"
        //"msDS-OIDToGroupLinkBl"
        //"msDS-OperationsForAzRoleBL"
        //"msDS-OperationsForAzTaskBL"
        //"msDS-PhoneticDisplayName"
        //"msDS-PrimaryComputer"
        //"msDS-PrincipalName"
        //"msDS-PSOApplied"
        //"msDS-ReplAttributeMetaData"
        //"msDS-ReplValueMetaData"
        //"msDS-RevealedDSAs"
        //"msDS-RevealedListBL"
        //"msDS-TasksForAzRoleBL"
        //"msDS-TasksForAzTaskBL"
        //"msDS-TDOEgressBL"
        //"msDS-TDOIngressBL"
        //"msDS-ValueTypeReferenceBL"
        //"msExchAssistantName"
        //"msExchLabeledURI"
        //"ownerBL" // 	ms-Exch-Owner-BL
        //"msSFU30Name"
        //"msSFU30NisDomain"
        //"msSFU30PosixMember"
        //"msSFU30PosixMemberOf"
        //"netbootSCPBL"
        //"nonSecurityMember"
        //"nonSecurityMemberBL"
        //"nTGroupMembers"
        //"nTSecurityDescriptor"
        //"distinguishedName"
        //"objectCategory"
        //"objectClass"
        //"objectGUID"
        //"objectSid"
        //"objectVersion"
        //"operatorCount"
        //"otherWellKnownObjects"
        //"partialAttributeDeletionList"
        //"partialAttributeSet"
        //"possibleInferiors"
        //"primaryGroupToken"
        //"proxiedObjectName"
        //"proxyAddresses"
        //"queryPolicyBL"
        //"name" // Relative Distinguished Name
        //"replPropertyMetaData"
        //"replUpToDateVector"
        //"directReports"
        //"repsFrom"
        //"repsTo"
        //"revision" //  revision level
        //"rid" // relative Identifier
        //"sAMAccountName"
        //sAMAccountType
        //sDRightsEffective
        //secretary
        //securityIdentifier
        //serverReferenceBL
        //showInAddressBook
        //showInAdvancedViewOnly
        //sIDHistory
        //siteObjectBL
        //structuralObjectClass
        //subRefs
        //subSchemaSubEntry
        //supplementalCredentials
        //systemFlags
        //telephoneNumber
        //textEncodedORAddress
        //tokenGroups
        //tokenGroupsGlobalAndUniversal
        //tokenGroupsNoGCAcceptable
        //unixUserPassword
        //userCert // Certificate
        //userPassword
        //userSMIMECertificate
        //uSNChanged
        //uSNCreated
        //uSNDSALastObjRemoved
        //USNIntersite
        //USN-Last-Obj-Rem
        //USN-Source
        //Wbem-Path
        //Well-Known-Objects
        //When-Changed
        //When-Created
        //WWW-Home-Page
        //WWW-Page-Other
        //X509-Cert
    }
}
