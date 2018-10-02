using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using Utilities.ActiveDirectory.Interfaces;

namespace Utilities.ActiveDirectory
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/windows/desktop/ADSchema/c-user#windows-server-2012-attributes
    /// </summary>
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

        #region IUser
        TimeSpan? IUser.AccountExpires => (TimeSpan?) this["accountExpires"];
        string IUser.ACSPolicyName => this["aCSPolicyName"]?.ToString();
        string IUser.AdminCount => this["adminCount"]?.ToString();
        string IUser.Audio => this["audio"]?.ToString();
        string IUser.BadPasswordTime => this["badPasswordTime"]?.ToString();
        int? IUser.BadPwdCount => (int?) this["badPwdCount"];
        string IUser.BusinessCategory => this["businessCategory"]?.ToString();
        string IUser.CarLicense => this["carLicense"]?.ToString();
        int? IUser.CodePage => (int?) this["codePage"];
        string IUser.ControlAccessRights => this["controlAccessRights"]?.ToString();
        string IUser.DBCSPwd => this["dBCSPwd"]?.ToString();
        string IUser.DefaultClassStore => this["defaultClassStore"]?.ToString();
        string IUser.DepartmentNumber => this["departmentNumber"]?.ToString();
        string IUser.DesktopProfile => this["desktopProfile"]?.ToString();
        string IUser.DynamicLDAPServer => this["dynamicLDAPServer"]?.ToString();
        string IUser.EmailAddresses => this["mail"]?.ToString();
        string IUser.EmployeeNumber => this["employeeNumber"]?.ToString();
        string IUser.EmployeeType => this["employeeType"]?.ToString();
        string IUser.GivenName => this["givenName"]?.ToString();
        string IUser.GroupMembershipSAM => this["groupMembershipSAM"]?.ToString();
        string IUser.GroupPriority => this["groupPriority"]?.ToString();
        string IUser.GroupsToIgnore => this["groupsToIgnore"]?.ToString();
        string IUser.HomeDirectory => this["homeDirectory"]?.ToString();
        string IUser.HomeDrive => this["homeDrive"]?.ToString();
        string IUser.Initials => this["initials"]?.ToString();
        string IUser.JpegPhoto => this["jpegPhoto"]?.ToString();
        string IUser.LabeledURI => this["labeledURI"]?.ToString();
        string IUser.LastLogoff => this["lastLogoff"]?.ToString();
        string IUser.LastLogon => this["lastLogon"]?.ToString();
        string IUser.LastLogonTimestamp => this["lastLogonTimestamp"]?.ToString();
        string IUser.LmPwdHistory => this["lmPwdHistory"]?.ToString();
        int? IUser.LocaleID => (int?) this["localeID"];
        TimeSpan? IUser.LockoutTime => (TimeSpan?) this["lockoutTime"];
        string IUser.LogonCount => this["logonCount"]?.ToString();
        string IUser.LogonHours => this["logonHours"]?.ToString();
        string IUser.LogonWorkstation => this["logonWorkstation"]?.ToString();
        string IUser.Manager => this["manager"]?.ToString();
        ulong? IUser.MaxStorage => (ulong?) this["maxStorage"];
        string IUser.MSCOMUserPartitionSetLink => this["msCOM-UserPartitionSetLink"]?.ToString();
        string IUser.MSDRMIdentityCertificate => this["msDRM-IdentityCertificate"]?.ToString();
        string IUser.MSDSAuthenticatedAtDC => this["msDS-AuthenticatedAtDC"]?.ToString();
        string IUser.MSDSCachedMembership => this["msDS-Cached-Membership"]?.ToString();
        TimeSpan? IUser.MSDSCachedMembershipTimeStamp => (TimeSpan?) this["msDS-Cached-Membership-Time-Stamp"];
        string IUser.MSDSCreatorSID => this["mS-DS-CreatorSID"]?.ToString();
        int? IUser.MSDSFailedInteractiveLogonCount => (int?) this["msDS-FailedInteractiveLogonCount"];
        int? IUser.MSDSFailedInteractiveLogonCountAtLastSuccessfulLogon => (int?) this["msDS-FailedInteractiveLogonCountAtLastSuccessfulLogon"];
        TimeSpan? IUser.MSDSLastFailedInteractiveLogonTime => (TimeSpan?) this["msDS-LastFailedInteractiveLogonTime"];
        TimeSpan? IUser.MSDSLastSuccessfulInteractiveLogonTime => (TimeSpan?) this["msDS-LastSuccessfulInteractiveLogonTime"];
        string IUser.MSDSPrimaryComputer => this["msDS-PrimaryComputer"]?.ToString();
        string IUser.MSDSResultantPSO => this["msDS-ResultantPSO"]?.ToString();
        int? IUser.MSDSSecondaryKrbTgtNumber => (int?) this["msDS-SecondaryKrbTgtNumber"];
        string IUser.MSDSSiteAffinity => this["msDS-Site-Affinity"]?.ToString();
        string IUser.MSDSSourceObjectDN => this["msDS-SourceObjectDN"]?.ToString();
        int? IUser.MSDSSupportedEncryptionTypes => (int?) this["msDS-SupportedEncryptionTypes"];
        UserAccess IUser.MSDSUserAccountControlComputed {
            get {
                if (this["msDS-User-Account-Control-Computed"] is int ival) {
                    return new UserAccess((ulong) ival);
                }
                return null;
            }
        }
        string IUser.MSDSUserPasswordExpiryTimeComputed => this[""]?.ToString();
        string IUser.MSIISFTPDir => this[""]?.ToString();
        string IUser.MSIISFTPRoot => this[""]?.ToString();
        string IUser.MSPKIAccountCredentials => this[""]?.ToString();
        string IUser.MSPKICredentialRoamingTokens => this[""]?.ToString();
        string IUser.MSPKIDPAPIMasterKeys => this[""]?.ToString();
        string IUser.MSPKIRoamingTimeStamp => this[""]?.ToString();
        string IUser.MSRADIUSFramedInterfaceId => this[""]?.ToString();
        string IUser.MSRADIUSFramedIpv6Prefix => this[""]?.ToString();
        string IUser.MSRADIUSFramedIpv6Route => this[""]?.ToString();
        string IUser.MSRADIUSSavedFramedInterfaceId => this[""]?.ToString();
        string IUser.MSRADIUSSavedFramedIpv6Prefix => this[""]?.ToString();
        string IUser.MSRADIUSSavedFramedIpv6Route => this[""]?.ToString();
        string IUser.MSTSAllowLogon => this[""]?.ToString();
        string IUser.MSTSBrokenConnectionAction => this[""]?.ToString();
        string IUser.MSTSConnectClientDrives => this[""]?.ToString();
        string IUser.MSTSConnectPrinterDrives => this[""]?.ToString();
        string IUser.MSTSDefaultToMainPrinter => this[""]?.ToString();
        string IUser.MSTSExpireDate => this["msTSExpireDate"]?.ToString();
        string IUser.MSTSExpireDate2 => this[""]?.ToString();
        string IUser.MSTSExpireDate3 => this[""]?.ToString();
        string IUser.MSTSExpireDate4 => this[""]?.ToString();
        string IUser.MSTSHomeDirectory => this[""]?.ToString();
        string IUser.MSTSHomeDrive => this[""]?.ToString();
        string IUser.MSTSInitialProgram => this[""]?.ToString();
        string IUser.MSTSLicenseVersion => this["msTSLicenseVersion"]?.ToString();
        string IUser.MSTSLicenseVersion2 => this[""]?.ToString();
        string IUser.MSTSLicenseVersion3 => this[""]?.ToString();
        string IUser.MSTSLicenseVersion4 => this[""]?.ToString();
        string IUser.MSTSManagingLS => this["msTSManagingLS"]?.ToString();
        string IUser.MSTSManagingLS2 => this[""]?.ToString();
        string IUser.MSTSManagingLS3 => this[""]?.ToString();
        string IUser.MSTSManagingLS4 => this[""]?.ToString();
        string IUser.MSTSMaxConnectionTime => this[""]?.ToString();
        string IUser.MSTSMaxDisconnectionTime => this[""]?.ToString();
        string IUser.MSTSMaxIdleTime => this[""]?.ToString();
        string IUser.MSTSPrimaryDesktop => this[""]?.ToString();
        string IUser.MSTSProfilePath => this[""]?.ToString();
        string IUser.MSTSProperty01 => this[""]?.ToString();
        string IUser.MSTSProperty02 => this[""]?.ToString();
        string IUser.MSTSReconnectionAction => this[""]?.ToString();
        string IUser.MSTSRemoteControl => this[""]?.ToString();
        string IUser.MSTSSecondaryDesktops => this[""]?.ToString();
        string IUser.MSTSWorkDirectory => this[""]?.ToString();
        string IUser.MSTSLSProperty01 => this[""]?.ToString();
        string IUser.MSTSLSProperty02 => this[""]?.ToString();
        string IUser.MSMQDigests => this["mSMQDigests"]?.ToString();
        string IUser.MSMQDigestsMig => this[""]?.ToString();
        string IUser.MSMQSignCertificates => this["mSMQSignCertificates"]?.ToString();
        string IUser.MSMQSignCertificatesMig => this[""]?.ToString();
        string IUser.MSNPAllowDialin => this[""]?.ToString();
        string IUser.MSNPCallingStationID => this[""]?.ToString();
        string IUser.MSNPSavedCallingStationID => this[""]?.ToString();
        string IUser.MSRADIUSCallbackNumber => this[""]?.ToString();
        string IUser.MSRADIUSFramedIPAddress => this[""]?.ToString();
        string IUser.MSRADIUSFramedRoute => this[""]?.ToString();
        string IUser.MSRADIUSServiceType => this[""]?.ToString();
        string IUser.MSRASSavedCallbackNumber => this[""]?.ToString();
        string IUser.MSRASSavedFramedIPAddress => this[""]?.ToString();
        string IUser.MSRASSavedFramedRoute => this[""]?.ToString();
        string IUser.MSSFU30Name => this[""]?.ToString();
        string IUser.MSSFU30NisDomain => this[""]?.ToString();
        string IUser.NetworkAddress => this[""]?.ToString();
        string IUser.NtPwdHistory => this[""]?.ToString();
        string IUser.OperatorCount => this[""]?.ToString();
        string IUser.OrganizationName => this[""]?.ToString();
        string IUser.OtherLoginWorkstations => this[""]?.ToString();
        string IUser.PhoneHomePrimary => this[""]?.ToString();
        string IUser.PhoneMobilePrimary => this[""]?.ToString();
        string IUser.PhonePagerPrimary => this[""]?.ToString();
        string IUser.Photo => this[""]?.ToString();
        string IUser.PreferredOU => this[""]?.ToString();
        string IUser.PreferredLanguage => this[""]?.ToString();
        string IUser.PrimaryGroupID => this["primaryGroupID"]?.ToString();
        string IUser.ProfilePath => this[""]?.ToString();
        string IUser.PwdLastSet => this[""]?.ToString();
        string IUser.RoomNumber => this[""]?.ToString();
        string IUser.ScriptPath => this["scriptPath"]?.ToString();
        string IUser.Secretary => this[""]?.ToString();
        string IUser.ServicePrincipalName => this["servicePrincipalName"]?.ToString();
        string IUser.TerminalServer => this[""]?.ToString();
        string IUser.Uid => this["uid"]?.ToString();
        string IUser.UnicodePwd => this[""]?.ToString();
        string IUser.UserAccountControl => this["userAccountControl"]?.ToString();
        string IUser.UserParameters => this[""]?.ToString();
        string IUser.UserPrincipalName => this["userPrincipalName"]?.ToString();
        string IUser.UserSharedFolder => this[""]?.ToString();
        string IUser.UserSharedFolderOther => this[""]?.ToString();
        string IUser.UserSMIMECertificate => this[""]?.ToString();
        string IUser.UserWorkstations => this[""]?.ToString();
        string IUser.UserPKCS12 => this[""]?.ToString();
        string IUser.X500uniqueIdentifier => this[""]?.ToString();
        string IUser.X509Cert => this[""]?.ToString();
        #endregion // IUser

        #region PosixAccount
        public string GECOS => this["gecos"]?.ToString();

        public int? GidNumber => (int?) this["gidNumber"];

        public int? LoginShell => (int?) this["loginShell"];

        public int? UidNumber => (int?) this["uidNumber"];

        public string UnixHomeDirectory => this["unixHomeDirectory"]?.ToString();

        public string UnixUserPassword => this["unixUserPassword"]?.ToString();

        public string UserPassword => this["userPassword"]?.ToString();
        #endregion // PosixAccount

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

        public string CountryCode => this["countryCode"]?.ToString();

        public string PasswordLastSet => this["pwdLastSet"]?.ToString();

        public string ObjectSid => this["objectSid"]?.ToString();

        public string LoginName => this["sAMAccountName"]?.ToString();

        public string SAMAccountType => this["sAMAccountType"]?.ToString();

        public string ShowInAddressBook => this["showInAddressBook"]?.ToString();

        public string LegacyExchangeDN => this["legacyExchangeDN"]?.ToString();

        public string Extension => this["ipPhone"]?.ToString();

        public string EmailAddress => this["mail"]?.ToString();

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

        public string MSExchMailboxTemplateLink => this["msExchMailboxTemplateLink"]?.ToString();

        public string MSExchUMDtmfMap => this["msExchUMDtmfMap"]?.ToString();

        public string MSExchTextMessagingState => this["msExchTextMessagingState"]?.ToString();

        public string MSExchUserCulture => this["msExchUserCulture"]?.ToString();

        public string MSExchUserBL => this["msExchUserBL"]?.ToString();

        public string MSExchELCMailboxFlags => this["msExchELCMailboxFlags"]?.ToString();

        public string MSExchWhenMailboxCreated => this["msExchWhenMailboxCreated"]?.ToString();

        public string MSExchRBACPolicyLink => this["msExchRBACPolicyLink"]?.ToString();

        string ISecurityPrincipal.AccountNameHistory => throw new NotImplementedException();

        string ISecurityPrincipal.AltSecurityIdentities => throw new NotImplementedException();

        string ISecurityPrincipal.MSDSKeyVersionNumber => throw new NotImplementedException();

        string ISecurityPrincipal.ObjectSid => throw new NotImplementedException();

        string ISecurityPrincipal.Rid => throw new NotImplementedException();

        string ISecurityPrincipal.SAMAccountName => throw new NotImplementedException();

        string ISecurityPrincipal.SAMAccountType => throw new NotImplementedException();

        string ISecurityPrincipal.SecurityIdentifier => throw new NotImplementedException();

        string ISecurityPrincipal.SIDHistory => throw new NotImplementedException();

        string ISecurityPrincipal.SupplementalCredentials => throw new NotImplementedException();

        string ISecurityPrincipal.TokenGroups => throw new NotImplementedException();

        string ISecurityPrincipal.TokenGroupsGlobalAndUniversal => throw new NotImplementedException();

        string ISecurityPrincipal.TokenGroupsNoGCAcceptable => throw new NotImplementedException();

        string IMailRecipient.Comment => throw new NotImplementedException();

        string IMailRecipient.GarbageCollPeriod => throw new NotImplementedException();

        string IMailRecipient.LabeledURI => throw new NotImplementedException();

        string IMailRecipient.LegacyExchangeDN => throw new NotImplementedException();

        string IMailRecipient.MSDSGeoCoordinatesAltitude => throw new NotImplementedException();

        string IMailRecipient.MSDSGeoCoordinatesLatitude => throw new NotImplementedException();

        string IMailRecipient.MSDSGeoCoordinatesLongitude => throw new NotImplementedException();

        string IMailRecipient.MSDSPhoneticDisplayName => throw new NotImplementedException();

        string IMailRecipient.MSExchAssistantName => throw new NotImplementedException();

        string IMailRecipient.MSExchLabeledURI => throw new NotImplementedException();

        string IMailRecipient.Secretary => throw new NotImplementedException();

        string IMailRecipient.ShowInAddressBook => throw new NotImplementedException();

        string IMailRecipient.TelephoneNumber => throw new NotImplementedException();

        string IMailRecipient.TextEncodedOrAddress => throw new NotImplementedException();

        string IMailRecipient.UserCert => throw new NotImplementedException();

        string IMailRecipient.UserSMIMECertificate => throw new NotImplementedException();

        string IMailRecipient.X509Cert => throw new NotImplementedException();

        #region ShadowAccount
        //shadowExpire
        //shadowFlag
        //shadowInactive
        //shadowLastChange
        //shadowMax
        //shadowMin
        //shadowWarning
        #endregion

        //uidNumber
        //unixHomeDirectory
        //unixUserPassword

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