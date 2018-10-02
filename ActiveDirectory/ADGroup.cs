using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using Utilities.ActiveDirectory.Interfaces;

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

        #region IGroup
        string IGroup.ControlAccessRights => throw new System.NotImplementedException();

        string IGroup.DesktopProfile => throw new System.NotImplementedException();

        string IGroup.EmailAddresses => throw new System.NotImplementedException();

        string IGroup.GroupAttributes => throw new System.NotImplementedException();

        string IGroup.GroupMembershipSAM => throw new System.NotImplementedException();

        string IGroup.GroupType => throw new System.NotImplementedException();

        string IGroup.ManagedBy => throw new System.NotImplementedException();

        string IGroup.Member => throw new System.NotImplementedException();

        string IGroup.MSDSAzApplicationData => throw new System.NotImplementedException();

        string IGroup.MSDSAzBizRule => throw new System.NotImplementedException();

        string IGroup.MSDSAzBizRuleLanguage => throw new System.NotImplementedException();

        string IGroup.MSDSAzGenericData => throw new System.NotImplementedException();

        string IGroup.MSDSAzLastImportedBizRulePath => throw new System.NotImplementedException();

        string IGroup.MSDSAzLDAPQuery => throw new System.NotImplementedException();

        string IGroup.MSDSAzObjectGuid => throw new System.NotImplementedException();

        string IGroup.MSDSNonMembers => throw new System.NotImplementedException();

        string IGroup.MSDSPrimaryComputer => throw new System.NotImplementedException();

        string IGroup.MSSFU30Name => throw new System.NotImplementedException();

        string IGroup.MSSFU30NisDomain => throw new System.NotImplementedException();

        string IGroup.MSSFU30PosixMember => throw new System.NotImplementedException();

        string IGroup.NonSecurityMember => throw new System.NotImplementedException();

        string IGroup.NTGroupMembers => throw new System.NotImplementedException();

        string IGroup.OperatorCount => throw new System.NotImplementedException();

        string IGroup.PrimaryGroupToken => throw new System.NotImplementedException();
        #endregion // IGroup

        #region ISecurityPrincipal
        string ISecurityPrincipal.AccountNameHistory => throw new System.NotImplementedException();

        string ISecurityPrincipal.AltSecurityIdentities => throw new System.NotImplementedException();

        string ISecurityPrincipal.MSDSKeyVersionNumber => throw new System.NotImplementedException();

        string ISecurityPrincipal.ObjectSid => throw new System.NotImplementedException();

        string ISecurityPrincipal.Rid => throw new System.NotImplementedException();

        string ISecurityPrincipal.SAMAccountName => throw new System.NotImplementedException();

        string ISecurityPrincipal.SAMAccountType => throw new System.NotImplementedException();

        string ISecurityPrincipal.SecurityIdentifier => throw new System.NotImplementedException();

        string ISecurityPrincipal.SIDHistory => throw new System.NotImplementedException();

        string ISecurityPrincipal.SupplementalCredentials => throw new System.NotImplementedException();

        string ISecurityPrincipal.TokenGroups => throw new System.NotImplementedException();

        string ISecurityPrincipal.TokenGroupsGlobalAndUniversal => throw new System.NotImplementedException();

        string ISecurityPrincipal.TokenGroupsNoGCAcceptable => throw new System.NotImplementedException();
        #endregion // ISecurityPrincipal

        #region // IMailRecipient
        string IMailRecipient.Comment => throw new System.NotImplementedException();

        string IMailRecipient.GarbageCollPeriod => throw new System.NotImplementedException();

        string IMailRecipient.LabeledURI => throw new System.NotImplementedException();

        string IMailRecipient.LegacyExchangeDN => throw new System.NotImplementedException();

        string IMailRecipient.MSDSGeoCoordinatesAltitude => throw new System.NotImplementedException();

        string IMailRecipient.MSDSGeoCoordinatesLatitude => throw new System.NotImplementedException();

        string IMailRecipient.MSDSGeoCoordinatesLongitude => throw new System.NotImplementedException();

        string IMailRecipient.MSDSPhoneticDisplayName => throw new System.NotImplementedException();

        string IMailRecipient.MSExchAssistantName => throw new System.NotImplementedException();

        string IMailRecipient.MSExchLabeledURI => throw new System.NotImplementedException();

        string IMailRecipient.Secretary => throw new System.NotImplementedException();

        string IMailRecipient.ShowInAddressBook => throw new System.NotImplementedException();

        string IMailRecipient.TelephoneNumber => throw new System.NotImplementedException();

        string IMailRecipient.TextEncodedOrAddress => throw new System.NotImplementedException();

        string IMailRecipient.UserCert => throw new System.NotImplementedException();

        string IMailRecipient.UserSMIMECertificate => throw new System.NotImplementedException();

        string IMailRecipient.X509Cert => throw new System.NotImplementedException();
        #endregion // IMailRecipient
    }
}
