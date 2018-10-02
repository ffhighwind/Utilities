using Utilities.ActiveDirectory.Interfaces;

namespace Utilities.ActiveDirectory
{
    public abstract class ADOrganizationalPerson : ADPerson, IOrganizationalPerson
    {
        public string AddressHome => this["homePostalAddress"]?.ToString();

        string IOrganizationalPerson.Address => throw new System.NotImplementedException();

        string IOrganizationalPerson.AddressHome => throw new System.NotImplementedException();

        string IOrganizationalPerson.Assistant => throw new System.NotImplementedException();

        string IOrganizationalPerson.Company => throw new System.NotImplementedException();

        string IOrganizationalPerson.CountryCode => throw new System.NotImplementedException();

        string IOrganizationalPerson.CountryName => throw new System.NotImplementedException();

        string IOrganizationalPerson.Department => throw new System.NotImplementedException();

        string IOrganizationalPerson.DestinationIndicator => throw new System.NotImplementedException();

        string IOrganizationalPerson.Division => throw new System.NotImplementedException();

        string IOrganizationalPerson.EmailAddresses => throw new System.NotImplementedException();

        string IOrganizationalPerson.EmployeeID => throw new System.NotImplementedException();

        string IOrganizationalPerson.FacsimileTelephoneNumber => throw new System.NotImplementedException();

        string IOrganizationalPerson.GenerationQualifier => throw new System.NotImplementedException();

        string IOrganizationalPerson.GivenName => throw new System.NotImplementedException();

        string IOrganizationalPerson.HouseIdentifier => throw new System.NotImplementedException();

        string IOrganizationalPerson.Initials => throw new System.NotImplementedException();

        string IOrganizationalPerson.InternationalISDNNumber => throw new System.NotImplementedException();

        string IOrganizationalPerson.LocalityName => throw new System.NotImplementedException();

        string IOrganizationalPerson.Logo => throw new System.NotImplementedException();

        string IOrganizationalPerson.Manager => throw new System.NotImplementedException();

        string IOrganizationalPerson.MHSORAddress => throw new System.NotImplementedException();

        string IOrganizationalPerson.MSDSAllowedToActOnBehalfOfOtherIdentity => throw new System.NotImplementedException();

        string IOrganizationalPerson.MSDSAllowedToDelegateTo => throw new System.NotImplementedException();

        string IOrganizationalPerson.MSDSHABSeniorityIndex => throw new System.NotImplementedException();

        string IOrganizationalPerson.MSDSPhoneticCompanyName => throw new System.NotImplementedException();

        string IOrganizationalPerson.MSDSPhoneticDepartment => throw new System.NotImplementedException();

        string IOrganizationalPerson.MSDSPhoneticDisplayName => throw new System.NotImplementedException();

        string IOrganizationalPerson.MSDSPhoneticFirstName => throw new System.NotImplementedException();

        string IOrganizationalPerson.MSDSPhoneticLastName => throw new System.NotImplementedException();

        string IOrganizationalPerson.MSExchHouseIdentifier => throw new System.NotImplementedException();

        string IOrganizationalPerson.OrganizationalUnitName => throw new System.NotImplementedException();

        string IOrganizationalPerson.OrganizationName => throw new System.NotImplementedException();

        string IOrganizationalPerson.OtherMailbox => throw new System.NotImplementedException();

        string IOrganizationalPerson.OtherName => throw new System.NotImplementedException();

        string IOrganizationalPerson.PersonalTitle => throw new System.NotImplementedException();

        string IOrganizationalPerson.PhoneFaxOther => throw new System.NotImplementedException();

        string IOrganizationalPerson.PhoneHomeOther => throw new System.NotImplementedException();

        string IOrganizationalPerson.PhoneHomePrimary => throw new System.NotImplementedException();

        string IOrganizationalPerson.PhoneIpOther => throw new System.NotImplementedException();

        string IOrganizationalPerson.PhoneIpPrimary => throw new System.NotImplementedException();

        string IOrganizationalPerson.PhoneISDNPrimary => throw new System.NotImplementedException();

        string IOrganizationalPerson.PhoneMobileOther => throw new System.NotImplementedException();

        string IOrganizationalPerson.PhoneMobilePrimary => throw new System.NotImplementedException();

        string IOrganizationalPerson.PhoneOfficeOther => throw new System.NotImplementedException();

        string IOrganizationalPerson.PhonePagerOther => throw new System.NotImplementedException();

        string IOrganizationalPerson.PhonePagerPrimary => throw new System.NotImplementedException();

        string IOrganizationalPerson.PhysicalDeliveryOfficeName => throw new System.NotImplementedException();

        string IOrganizationalPerson.Picture => throw new System.NotImplementedException();

        string IOrganizationalPerson.PostalAddress => throw new System.NotImplementedException();

        string IOrganizationalPerson.PostalCode => throw new System.NotImplementedException();

        string IOrganizationalPerson.PostOfficeBox => throw new System.NotImplementedException();

        string IOrganizationalPerson.PreferredDeliveryMethod => throw new System.NotImplementedException();

        string IOrganizationalPerson.RegisteredAddress => throw new System.NotImplementedException();

        string IOrganizationalPerson.StateOrProvinceName => throw new System.NotImplementedException();

        string IOrganizationalPerson.StreetAddress => throw new System.NotImplementedException();

        string IOrganizationalPerson.TeletexTerminalIdentifier => throw new System.NotImplementedException();

        string IOrganizationalPerson.TelexNumber => throw new System.NotImplementedException();

        string IOrganizationalPerson.TelexPrimary => throw new System.NotImplementedException();

        string IOrganizationalPerson.TextCountry => throw new System.NotImplementedException();

        string IOrganizationalPerson.Title => throw new System.NotImplementedException();

        string IOrganizationalPerson.UserComment => throw new System.NotImplementedException();

        string IOrganizationalPerson.X121Address => throw new System.NotImplementedException();
    }
}
