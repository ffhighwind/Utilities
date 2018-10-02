using Utilities.ActiveDirectory.Interfaces;

namespace Utilities.ActiveDirectory
{
    public abstract class ADPerson : ADBase, IPerson
    {
        string IPerson.AttributeCertificateAttribute => throw new System.NotImplementedException();

        string IPerson.SeeAlso => throw new System.NotImplementedException();

        string IPerson.SerialNumber => throw new System.NotImplementedException();

        string IPerson.Surname => throw new System.NotImplementedException();

        string IPerson.TelephoneNumber => throw new System.NotImplementedException();

        string IPerson.UserPassword => throw new System.NotImplementedException();
    }
}
