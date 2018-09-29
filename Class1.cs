namespace Utilities
{
    internal class Class1
    {
        public static void Main(string[] args)
        {
            ActiveDirectory.ActiveDirectory ad = new ActiveDirectory.ActiveDirectory();

            System.Net.NetworkInformation.IPGlobalProperties props = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties();

            //string domain1 = ActiveDirectory.ActiveDirectory.ComputerDomain;
            string domain2 = ActiveDirectory.ActiveDirectory.CurrentDomain;
            string username = ActiveDirectory.ActiveDirectory.CurrentUserName;
            ad.Bind();
            ad.GetUsers();
        }
    }
}
