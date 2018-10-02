namespace Utilities.ActiveDirectory.Interfaces
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/windows/desktop/ADSchema/c-organizationalperson#windows-server-2012-attributes
    /// </summary>
    public interface IOrganizationalPerson : IPerson
    {
        string Address { get; } // Address
        string AddressHome { get; } // Address-Home
        string Assistant { get; } // Assistant
        string Company { get; } // Company
        string CountryCode { get; } // Country-Code
        string CountryName { get; } // Country-Name
        string Department { get; } // Department
        string DestinationIndicator { get; } // Destination-Indicator
        string Division { get; } // Division
        string EmailAddresses { get; } // E-mail-Addresses
        string EmployeeID { get; } // Employee-ID
        string FacsimileTelephoneNumber { get; } // Facsimile-Telephone-Number
        string GenerationQualifier { get; } // Generation-Qualifier
        string GivenName { get; } // Given-Name
        string HouseIdentifier { get; } // houseIdentifier
        string Initials { get; } // Initials
        string InternationalISDNNumber { get; } // International-ISDN-Number
        string LocalityName { get; } // Locality-Name
        string Logo { get; } // Logo
        string Manager { get; } // Manager
        string MHSORAddress { get; } // MHS-OR-Address
        string MSDSAllowedToActOnBehalfOfOtherIdentity { get; } // ms-DS-Allowed-To-Act-On-Behalf-Of-Other-Identity
        string MSDSAllowedToDelegateTo { get; } // ms-DS-Allowed-To-Delegate-To
        string MSDSHABSeniorityIndex { get; } // ms-DS-HAB-Seniority-Index
        string MSDSPhoneticCompanyName { get; } // ms-DS-Phonetic-Company-Name
        string MSDSPhoneticDepartment { get; } // ms-DS-Phonetic-Department
        string MSDSPhoneticDisplayName { get; } // ms-DS-Phonetic-Display-Name
        string MSDSPhoneticFirstName { get; } // ms-DS-Phonetic-First-Name
        string MSDSPhoneticLastName { get; } // ms-DS-Phonetic-Last-Name
        string MSExchHouseIdentifier { get; } // ms-Exch-House-Identifier
        string OrganizationalUnitName { get; } // Organizational-Unit-Name
        string OrganizationName { get; } // Organization-Name
        string OtherMailbox { get; } // Other-Mailbox
        string OtherName { get; } // Other-Name
        string PersonalTitle { get; } // Personal-Title
        string PhoneFaxOther { get; } // Phone-Fax-Other
        string PhoneHomeOther { get; } // Phone-Home-Other
        string PhoneHomePrimary { get; } // Phone-Home-Primary
        string PhoneIpOther { get; } // Phone-Ip-Other
        string PhoneIpPrimary { get; } // Phone-Ip-Primary
        string PhoneISDNPrimary { get; } // Phone-ISDN-Primary
        string PhoneMobileOther { get; } // Phone-Mobile-Other
        string PhoneMobilePrimary { get; } // Phone-Mobile-Primary
        string PhoneOfficeOther { get; } // Phone-Office-Other
        string PhonePagerOther { get; } // Phone-Pager-Other
        string PhonePagerPrimary { get; } // Phone-Pager-Primary
        string PhysicalDeliveryOfficeName { get; } // Physical-Delivery-Office-Name
        string Picture { get; } // Picture
        string PostalAddress { get; } // Postal-Address
        string PostalCode { get; } // Postal-Code
        string PostOfficeBox { get; } // Post-Office-Box
        string PreferredDeliveryMethod { get; } // Preferred-Delivery-Method
        string RegisteredAddress { get; } // Registered-Address
        string StateOrProvinceName { get; } // State-Or-Province-Name
        string StreetAddress { get; } // Street-Address
        string TeletexTerminalIdentifier { get; } // Teletex-Terminal-Identifier
        string TelexNumber { get; } // Telex-Number
        string TelexPrimary { get; } // Telex-Primary
        string TextCountry { get; } // Text-Country
        string Title { get; } // Title
        string UserComment { get; } // User-Comment
        string X121Address { get; } // X121-Address
    }
}
