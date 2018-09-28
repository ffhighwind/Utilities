using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.ActiveDirectory
{
    public class ADUser
    {
        public ADUser(UserPrincipal principal)
        {
            this.Principal = principal;
            DirectoryEntry = (DirectoryEntry) principal.GetUnderlyingObject();
        }

        public UserPrincipal Principal { get; private set; }
        public DirectoryEntry DirectoryEntry { get; private set; }

        public object this[string property] {
            get {
                if (DirectoryEntry.Properties.Contains(property))
                    return DirectoryEntry.Properties[property].Value;
                return null;
            }
        }

        public string ObjectClass { get { return this["objectClass"]?.ToString(); } }

        public string ContainerName { get { return this["cn"]?.ToString(); } }

        public string LastName { get { return this["sn"]?.ToString(); } }

        public string CountryNotation { get { return this["c"]?.ToString(); } }

        public string City { get { return this["l"]?.ToString(); } }

        public string State { get { return this["st"]?.ToString(); } }

        public string Title { get { return this["title"]?.ToString(); } }

        public string PostalCode { get { return this["postalCode"]?.ToString(); } }

        public string PhysicalDeliveryOfficeName { get { return this["physicalDeliveryOfficeName"]?.ToString(); } }

        public string FirstName { get { return this["givenName"]?.ToString(); } }

        public string MiddleName { get { return this["initials"]?.ToString(); } }

        public string DistinguishedName { get { return this["distinguishedName"]?.ToString(); } }

        public string InstanceType { get { return this["instanceType"]?.ToString(); } }

        public string WhenCreated { get { return this["whenCreated"]?.ToString(); } }

        public string WhenChanged { get { return this["whenChanged"]?.ToString(); } }

        public string DisplayName { get { return this["displayName"]?.ToString(); } }

        public string USNCreated { get { return this["uSNCreated"]?.ToString(); } }

        public string MemberOf { get { return this["memberOf"]?.ToString(); } }

        public string USNChanged { get { return this["uSNChanged"]?.ToString(); } }

        public string Country { get { return this["co"]?.ToString(); } }

        public string Department { get { return this["department"]?.ToString(); } }

        public string Company { get { return this["company"]?.ToString(); } }

        public string ProxyAddresses { get { return this["proxyAddresses"]?.ToString(); } }

        public string StreetAddress { get { return this["streetAddress"]?.ToString(); } }

        public string DirectReports { get { return this["directReports"]?.ToString(); } }

        public string Name { get { return this["name"]?.ToString(); } }

        public string ObjectGuid { get { return this["objectGUID"]?.ToString(); } }

        public string UserAccountControl { get { return this["userAccountControl"]?.ToString(); } }

        public string BadPwdCount { get { return this["badPwdCount"]?.ToString(); } }

        public string CodePage { get { return this["codePage"]?.ToString(); } }

        public string CountryCode { get { return this["countryCode"]?.ToString(); } }

        public string BadPasswordTime { get { return this["badPasswordTime"]?.ToString(); } }

        public string LastLogoff { get { return this["lastLogoff"]?.ToString(); } }

        public string LastLogon { get { return this["lastLogon"]?.ToString(); } }

        public string PasswordLastSet { get { return this["pwdLastSet"]?.ToString(); } }

        public string PrimaryGroupID { get { return this["primaryGroupID"]?.ToString(); } }

        public string ObjectSid { get { return this["objectSid"]?.ToString(); } }

        public string AdminCount { get { return this["adminCount"]?.ToString(); } }

        public string AccountExpires { get { return this["accountExpires"]?.ToString(); } }

        public string LogonCount { get { return this["logonCount"]?.ToString(); } }

        public string LoginName { get { return this["sAMAccountName"]?.ToString(); } }

        public string SAMAccountType { get { return this["sAMAccountType"]?.ToString(); } }

        public string ShowInAddressBook { get { return this["showInAddressBook"]?.ToString(); } }

        public string LegacyExchangeDN { get { return this["legacyExchangeDN"]?.ToString(); } }

        public string UserPrincipalName { get { return this["userPrincipalName"]?.ToString(); } }

        public string Extension { get { return this["ipPhone"]?.ToString(); } }

        public string ServicePrincipalName { get { return this["servicePrincipalName"]?.ToString(); } }

        public string ObjectCategory { get { return this["objectCategory"]?.ToString(); } }

        public string DSCorePropagationData { get { return this["dSCorePropagationData"]?.ToString(); } }

        public string lastLogonTimestamp { get { return this["lastLogonTimestamp"]?.ToString(); } }

        public string EmailAddress { get { return this["mail"]?.ToString(); } }

        public string Manager { get { return this["manager"]?.ToString(); } }

        public string Mobile { get { return this["mobile"]?.ToString(); } }

        public string Pager { get { return this["pager"]?.ToString(); } }

        public string Fax { get { return this["facsimileTelephoneNumber"]?.ToString(); } }

        public string HomePhone { get { return this["homePhone"]?.ToString(); } }

        public string MSExchUserAccountControl { get { return this["msExchUserAccountControl"]?.ToString(); } }

        public string MDBUuseDefaults { get { return this["mDBUseDefaults"]?.ToString(); } }

        public string MSExchMailboxSecurityDescriptor { get { return this["msExchMailboxSecurityDescriptor"]?.ToString(); } }

        public string HomeMDB { get { return this["homeMDB"]?.ToString(); } }

        public string MSExchPoliciesIncluded { get { return this["msExchPoliciesIncluded"]?.ToString(); } }

        public string HomeMTA { get { return this["homeMTA"]?.ToString(); } }

        public string MSExchRecipientTypeDetails { get { return this["msExchRecipientTypeDetails"]?.ToString(); } }

        public string MailNickname { get { return this["mailNickname"]?.ToString(); } }

        public string MSExchHomeServerName { get { return this["msExchHomeServerName"]?.ToString(); } }

        public string MSExchVersion { get { return this["msExchVersion"]?.ToString(); } }

        public string MSExchRecipientDisplayType { get { return this["msExchRecipientDisplayType"]?.ToString(); } }

        public string MSExchMailboxGuid { get { return this["msExchMailboxGuid"]?.ToString(); } }

        public string NTSecurityDescriptor { get { return this["nTSecurityDescriptor"]?.ToString(); } }

        public string Description { get { return this["description"]?.ToString(); } }

        public string ScriptPath { get { return this["scriptPath"]?.ToString(); } }

        //New?
        public string MSMQSignCertificates { get { return this["mSMQSignCertificates"]?.ToString(); } }

        public string MSMQDigests { get { return this["mSMQDigests"]?.ToString(); } }

        public string MSTSExpireDate { get { return this["msTSExpireDate"]?.ToString(); } }

        public string MSTSLicenseVersion { get { return this["msTSLicenseVersion"]?.ToString(); } }

        public string MSTSManagingLS { get { return this["msTSManagingLS"]?.ToString(); } }

        public string MSExchMailboxTemplateLink { get { return this["msExchMailboxTemplateLink"]?.ToString(); } }

        public string MSExchUMDtmfMap { get { return this["msExchUMDtmfMap"]?.ToString(); } }

        public string MSExchTextMessagingState { get { return this["msExchTextMessagingState"]?.ToString(); } }

"msExchUserCulture"

        "msExchUserBL"
"msExchELCMailboxFlags"
"msExchWhenMailboxCreated"

"msExchRBACPolicyLink"

    }
}
