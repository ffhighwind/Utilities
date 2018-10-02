using Utilities.ActiveDirectory.Interfaces;

namespace Utilities.ActiveDirectory
{
    public abstract class ADOrganizationalPerson : ADPerson, IOrganizationalPerson
    {
        public string AddressHome => this["homePostalAddress"]?.ToString();
    }
}
