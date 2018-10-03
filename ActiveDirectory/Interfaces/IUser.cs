﻿using System;

namespace Utilities.ActiveDirectory.Interfaces
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/windows/desktop/ADSchema/c-user#windows-server-2012-attributes
    /// </summary>
    public interface IUser
    {
        TimeSpan? AccountExpires { get; }
        string ACSPolicyName { get; }
        string AddressHome { get; }
        string AdminCount { get; }
        string Audio { get; }
        string BadPasswordTime { get; }
        int? BadPwdCount { get; }
        string BusinessCategory { get; }
        string CarLicense { get; }
        int? CodePage { get; }
        string ControlAccessRights { get; }
        string DBCSPwd { get; }
        string DefaultClassStore { get; }
        string DepartmentNumber { get; }
        string DesktopProfile { get; }
        string DynamicLDAPServer { get; }
        string EmailAddresses { get; }
        string EmployeeNumber { get; }
        string EmployeeType { get; }
        string GivenName { get; }
        string GroupMembershipSAM { get; }
        string GroupPriority { get; }
        string GroupsToIgnore { get; }
        string HomeDirectory { get; }
        string HomeDrive { get; }
        string Initials { get; }
        string JpegPhoto { get; }
        string LabeledURI { get; }
        string LastLogoff { get; }
        string LastLogon { get; }
        string LastLogonTimestamp { get; }
        string LmPwdHistory { get; }
        int? LocaleID { get; }
        TimeSpan? LockoutTime { get; }
        string LogonCount { get; }
        string LogonHours { get; }
        string LogonWorkstation { get; }
        string Manager { get; }
        ulong? MaxStorage { get; }
        string MSCOMUserPartitionSetLink { get; }
        string MSDRMIdentityCertificate { get; }
        string MSDSAuthenticatedAtDC { get; }
        string MSDSCachedMembership { get; }
        TimeSpan? MSDSCachedMembershipTimeStamp { get; }
        string MSDSCreatorSID { get; }
        int? MSDSFailedInteractiveLogonCount { get; }
        int? MSDSFailedInteractiveLogonCountAtLastSuccessfulLogon { get; }
        TimeSpan? MSDSLastFailedInteractiveLogonTime { get; }
        TimeSpan? MSDSLastSuccessfulInteractiveLogonTime { get; }
        string MSDSPrimaryComputer { get; }
        string MSDSResultantPSO { get; }
        int? MSDSSecondaryKrbTgtNumber { get; }
        string MSDSSiteAffinity { get; }
        string MSDSSourceObjectDN { get; }
        int? MSDSSupportedEncryptionTypes { get; }
        UserAccess MSDSUserAccountControlComputed { get; }
        string MSDSUserPasswordExpiryTimeComputed { get; }
        string MSIISFTPDir { get; }
        string MSIISFTPRoot { get; }
        string MSPKIAccountCredentials { get; }
        string MSPKICredentialRoamingTokens { get; }
        string MSPKIDPAPIMasterKeys { get; }
        string MSPKIRoamingTimeStamp { get; }
        string MSRADIUSFramedInterfaceId { get; }
        string MSRADIUSFramedIpv6Prefix { get; }
        string MSRADIUSFramedIpv6Route { get; }
        string MSRADIUSSavedFramedInterfaceId { get; }
        string MSRADIUSSavedFramedIpv6Prefix { get; }
        string MSRADIUSSavedFramedIpv6Route { get; }
        string MSTSAllowLogon { get; }
        string MSTSBrokenConnectionAction { get; }
        string MSTSConnectClientDrives { get; }
        string MSTSConnectPrinterDrives { get; }
        string MSTSDefaultToMainPrinter { get; }
        string MSTSExpireDate { get; }
        string MSTSExpireDate2 { get; }
        string MSTSExpireDate3 { get; }
        string MSTSExpireDate4 { get; }
        string MSTSHomeDirectory { get; }
        string MSTSHomeDrive { get; }
        string MSTSInitialProgram { get; }
        string MSTSLicenseVersion { get; }
        string MSTSLicenseVersion2 { get; }
        string MSTSLicenseVersion3 { get; }
        string MSTSLicenseVersion4 { get; }
        string MSTSManagingLS { get; }
        string MSTSManagingLS2 { get; }
        string MSTSManagingLS3 { get; }
        string MSTSManagingLS4 { get; }
        string MSTSMaxConnectionTime { get; }
        string MSTSMaxDisconnectionTime { get; }
        string MSTSMaxIdleTime { get; }
        string MSTSPrimaryDesktop { get; }
        string MSTSProfilePath { get; }
        string MSTSProperty01 { get; }
        string MSTSProperty02 { get; }
        string MSTSReconnectionAction { get; }
        string MSTSRemoteControl { get; }
        string MSTSSecondaryDesktops { get; }
        string MSTSWorkDirectory { get; }
        string MSTSLSProperty01 { get; }
        string MSTSLSProperty02 { get; }
        string MSMQDigests { get; }
        string MSMQDigestsMig { get; }
        string MSMQSignCertificates { get; }
        string MSMQSignCertificatesMig { get; }
        string MSNPAllowDialin { get; }
        string MSNPCallingStationID { get; }
        string MSNPSavedCallingStationID { get; }
        string MSRADIUSCallbackNumber { get; }
        string MSRADIUSFramedIPAddress { get; }
        string MSRADIUSFramedRoute { get; }
        string MSRADIUSServiceType { get; }
        string MSRASSavedCallbackNumber { get; }
        string MSRASSavedFramedIPAddress { get; }
        string MSRASSavedFramedRoute { get; }
        string MSSFU30Name { get; }
        string MSSFU30NisDomain { get; }
        string NetworkAddress { get; }
        string NtPwdHistory { get; }
        string OperatorCount { get; }
        string OrganizationName { get; }
        string OtherLoginWorkstations { get; }
        string PhoneHomePrimary { get; }
        string PhoneMobilePrimary { get; }
        string PhonePagerPrimary { get; }
        string Photo { get; }
        string PreferredOU { get; }
        string PreferredLanguage { get; }
        string PrimaryGroupID { get; }
        string ProfilePath { get; }
        string PwdLastSet { get; }
        string RoomNumber { get; }
        string ScriptPath { get; }
        string Secretary { get; }
        string ServicePrincipalName { get; }
        string TerminalServer { get; }
        string Uid { get; }
        string UnicodePwd { get; }
        string UserAccountControl { get; }
        string UserParameters { get; }
        string UserPrincipalName { get; }
        string UserSharedFolder { get; }
        string UserSharedFolderOther { get; }
        string UserSMIMECertificate { get; }
        string UserWorkstations { get; }
        string UserPKCS12 { get; }
        string X500uniqueIdentifier { get; }
        string X509Cert { get; }
    }
}