using System.DirectoryServices;

namespace Utilities.ActiveDirectory
{

    public static class ADProperties
    {

        public static void Set(DirectoryEntry entry, string name, string value)
        {
            if (entry.Properties.Contains(name)) {
                entry.Properties[name][0] = value;
                entry.CommitChanges();
            }
            else {
                entry.Properties[name].Add(value);
                entry.CommitChanges();
            }
        }

        public static string Get(DirectoryEntry entry, string name)
        {
            if (entry.Properties.Contains(name)) {
                return entry.Properties[name][0].ToString();
            }
            return null;
        }
    }
}
