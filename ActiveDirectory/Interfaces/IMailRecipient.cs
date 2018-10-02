namespace Utilities.ActiveDirectory.Interfaces
{
    public interface IMailRecipient
    {
        string Comment { get; } // Comment
        string GarbageCollPeriod { get; } // Garbage-Coll-Period
        string LabeledURI { get; } // labeledURI
        string LegacyExchangeDN { get; } // Legacy-Exchange-DN
        string MSDSGeoCoordinatesAltitude { get; } // ms-DS-GeoCoordinates-Altitude
        string MSDSGeoCoordinatesLatitude { get; } // ms-DS-GeoCoordinates-Latitude
        string MSDSGeoCoordinatesLongitude { get; } // ms-DS-GeoCoordinates-Longitude
        string MSDSPhoneticDisplayName { get; } // ms-DS-Phonetic-Display-Name
        string MSExchAssistantName { get; } // ms-Exch-Assistant-Name
        string MSExchLabeledURI { get; } // ms-Exch-LabeledURI
        string Secretary { get; } // secretary
        string ShowInAddressBook { get; } // Show-In-Address-Book
        string TelephoneNumber { get; } // Telephone-Number
        string TextEncodedOrAddress { get; } // Text-Encoded-OR-Address
        string UserCert { get; } // User-Cert
        string UserSMIMECertificate { get; } // User-SMIME-Certificate
        string X509Cert { get; } // X509-Cert
    }
}
