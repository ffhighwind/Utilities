namespace Utilities.ActiveDirectory.Interfaces
{
    public interface IPerson
    {
        string AttributeCertificateAttribute { get; } // attributeCertificateAttribute
        string SeeAlso { get; } // See-Also
        string SerialNumber { get; } // Serial-Number
        string Surname { get; } // Surname
        string TelephoneNumber { get; } // Telephone-Number
        string UserPassword { get; } // User-Password
    }
}
