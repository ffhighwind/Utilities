using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

namespace Utilities.ActiveDirectory
{
    public class ADUser : ADOrganizationalPerson, IDisposable, IUser, ISecurityPrincipal, IMailRecipient
    {
        public ADUser() { }

        public ADUser(UserPrincipal principal)
        {
            Principal = principal;
        }

        private AuthenticablePrincipal principal;

        public AuthenticablePrincipal Principal {
            get => principal;
            set {
                principal = value;
                DirectoryEntry = (DirectoryEntry) value.GetUnderlyingObject();
            }
        }

        public string GECOS => this["gecos"]?.ToString();

        public int? GidNumber => (int?) this["gidNumber"];

        public int? LoginShell => (int?) this["loginShell"];

        public string LastName => this["sn"]?.ToString();

        public string CountryNotation => this["c"]?.ToString();

        public string City => this["l"]?.ToString();

        public string State => this["st"]?.ToString();

        public string Title => this["title"]?.ToString();

        public string PostalCode => this["postalCode"]?.ToString();

        public string PhysicalDeliveryOfficeName => this["physicalDeliveryOfficeName"]?.ToString();

        public string FirstName => this["givenName"]?.ToString();

        public string MiddleName => this["initials"]?.ToString();

        public string Country => this["co"]?.ToString();

        public string Department => this["department"]?.ToString();

        public string Company => this["company"]?.ToString();

        public string ObjectGuid => this["objectGUID"]?.ToString();

        public string UserAccountControl => this["userAccountControl"]?.ToString();

        public string BadPwdCount => this["badPwdCount"]?.ToString();

        public string CodePage => this["codePage"]?.ToString();

        public string CountryCode => this["countryCode"]?.ToString();

        public string BadPasswordTime => this["badPasswordTime"]?.ToString();

        public string LastLogoff => this["lastLogoff"]?.ToString();

        public string LastLogon => this["lastLogon"]?.ToString();

        public string PasswordLastSet => this["pwdLastSet"]?.ToString();

        public string PrimaryGroupID => this["primaryGroupID"]?.ToString();

        public string ObjectSid => this["objectSid"]?.ToString();

        public string AdminCount => this["adminCount"]?.ToString();

        public string AccountExpires => this["accountExpires"]?.ToString();

        public string LogonCount => this["logonCount"]?.ToString();

        public string LoginName => this["sAMAccountName"]?.ToString();

        public string SAMAccountType => this["sAMAccountType"]?.ToString();

        public string ShowInAddressBook => this["showInAddressBook"]?.ToString();

        public string LegacyExchangeDN => this["legacyExchangeDN"]?.ToString();

        public string UserPrincipalName => this["userPrincipalName"]?.ToString();

        public string Extension => this["ipPhone"]?.ToString();

        public string ServicePrincipalName => this["servicePrincipalName"]?.ToString();

        public string LastLogonTimestamp => this["lastLogonTimestamp"]?.ToString();

        public string EmailAddress => this["mail"]?.ToString();

        public string Manager => this["manager"]?.ToString();

        public string Mobile => this["mobile"]?.ToString();

        public string Pager => this["pager"]?.ToString();

        public string Fax => this["facsimileTelephoneNumber"]?.ToString();

        public string HomePhone => this["homePhone"]?.ToString();

        public string MSExchUserAccountControl => this["msExchUserAccountControl"]?.ToString();

        public string MDBUuseDefaults => this["mDBUseDefaults"]?.ToString();

        public string MSExchMailboxSecurityDescriptor => this["msExchMailboxSecurityDescriptor"]?.ToString();

        public string HomeMDB => this["homeMDB"]?.ToString();

        public string MSExchPoliciesIncluded => this["msExchPoliciesIncluded"]?.ToString();

        public string HomeMTA => this["homeMTA"]?.ToString();

        public string MSExchRecipientTypeDetails => this["msExchRecipientTypeDetails"]?.ToString();

        public string MailNickname => this["mailNickname"]?.ToString();

        public string MSExchHomeServerName => this["msExchHomeServerName"]?.ToString();

        public string MSExchVersion => this["msExchVersion"]?.ToString();

        public string MSExchRecipientDisplayType => this["msExchRecipientDisplayType"]?.ToString();

        public string MSExchMailboxGuid => this["msExchMailboxGuid"]?.ToString();

        public string ScriptPath => this["scriptPath"]?.ToString();

        //New?
        public string MSMQSignCertificates => this["mSMQSignCertificates"]?.ToString();

        public string MSMQDigests => this["mSMQDigests"]?.ToString();

        public string MSTSExpireDate => this["msTSExpireDate"]?.ToString();

        public string MSTSLicenseVersion => this["msTSLicenseVersion"]?.ToString();

        public string MSTSManagingLS => this["msTSManagingLS"]?.ToString();

        public string MSExchMailboxTemplateLink => this["msExchMailboxTemplateLink"]?.ToString();

        public string MSExchUMDtmfMap => this["msExchUMDtmfMap"]?.ToString();

        public string MSExchTextMessagingState => this["msExchTextMessagingState"]?.ToString();

        public string MSExchUserCulture => this["msExchUserCulture"]?.ToString();

        public string MSExchUserBL => this["msExchUserBL"]?.ToString();

        public string MSExchELCMailboxFlags => this["msExchELCMailboxFlags"]?.ToString();

        public string MSExchWhenMailboxCreated => this["msExchWhenMailboxCreated"]?.ToString();

        public string MSExchRBACPolicyLink => this["msExchRBACPolicyLink"]?.ToString();

        #region ShadowAccount
        private readonly shadowExpire
        shadowFlag
        private readonly shadowInactive
        shadowLastChange
        private readonly shadowMax
        shadowMin
        private readonly shadowWarning
        #endregion

        uidNumber
        private readonly unixHomeDirectory
        unixUserPassword

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue) {
                if (disposing) {
                    Principal.Dispose();
                }
                Principal = null;
                DirectoryEntry = null;
                disposedValue = true;
            }
        }

        ~ADUser()
        {
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}