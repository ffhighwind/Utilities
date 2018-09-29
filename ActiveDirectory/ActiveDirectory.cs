using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;

namespace Utilities.ActiveDirectory
{

    /// <summary>
    /// LDAP=Lightweight Directory Access Protocol
    /// DN=Domain Name
    /// CN=Common/Container Name
    /// OU=Organization Unit
    /// DC=Domain Component
    /// </summary>
    public class ActiveDirectory
    {
        public ActiveDirectory() { }

        public DirectoryEntry Entry { get; private set; }

        public PrincipalContext Context { get; private set; }

        public bool Bind()
        {
            return Bind(ContextType.Domain, Environment.UserDomainName, null, null);
        }

        public bool Bind(string username, string password)
        {
            return Bind(ContextType.Domain, Environment.UserDomainName, username, password);
        }

        public bool Bind(ContextType contextType, string path, string username, string password)
        {
            try {
                if (username != null && password != null) {
                    Entry = new DirectoryEntry(path, username, password);
                    Context = new PrincipalContext(contextType, path, username, password);
                }
                else {
                    Context = new PrincipalContext(contextType, path);
                    Entry = new DirectoryEntry(path) {
                        AuthenticationType = AuthenticationTypes.Secure
                    };
                }
                return true;
            }
            catch (Exception ex) {
                Console.Error.WriteLine("ActiveDirectory.Bind(): " + ex.Message);
            }
            return false;
        }

        public static UserPrincipal CurrentUser {
            get {
                PrincipalContext context = new PrincipalContext(ContextType.Domain, Environment.UserDomainName);
                return UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, Environment.UserName);
            }
        }

        public static string CurrentDomain =>
                ////return System.DirectoryServices.ActiveDirectory.Domain.GetCurrentDomain().Name; // can throw an exception
                Environment.UserDomainName;

        public static string ComputerDomain =>
                ////System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
                System.DirectoryServices.ActiveDirectory.Domain.GetComputerDomain().Name; // can throw an exception

        public static string CurrentUserName => Environment.UserName;

        public static PrincipalContext CurrentContext => new PrincipalContext(ContextType.Domain, CurrentDomain);

        /// <summary>
        /// https://docs.microsoft.com/en-us/windows/desktop/adsi/search-filter-syntax
        /// https://docs.microsoft.com/en-us/windows/desktop/ADSchema/classes-all
        /// https://docs.microsoft.com/en-us/windows/desktop/ad/object-class-and-object-category
        /// </summary>
        /// <param name="filter">"(&(objectCategory=User)(objectClass=person)(name=" + name + "))"</param>
        /// <param name="propertiesToLoad">The properties to load.</param>
        /// <returns>The results of the search.</returns>
        public IEnumerable<SearchResult> Search(string filter, params string[] propertiesToLoad)
        {
            DirectorySearcher searcher = new DirectorySearcher(Entry);
            if (propertiesToLoad.Length > 0)
                searcher.PropertiesToLoad.AddRange(propertiesToLoad);
            if (filter != null)
                searcher.Filter = filter;
            return searcher.FindAll().Cast<SearchResult>();
        }

        public IEnumerable<UserPrincipal> GetUsers(UserPrincipal filter = null)
        {
            PrincipalSearcher searcher = filter == null
                ? new PrincipalSearcher(new UserPrincipal(Context))
                : new PrincipalSearcher(filter);
            PrincipalSearchResult<Principal> result = searcher.FindAll();
            return result.Cast<UserPrincipal>().Where(user => user != null);
        }

        public UserPrincipal GetUser(UserPrincipal filter = null)
        {
            PrincipalSearcher searcher = filter == null
                ? new PrincipalSearcher(new UserPrincipal(Context))
                : new PrincipalSearcher(filter);
            return searcher.FindOne() as UserPrincipal;
        }

        public IEnumerable<GroupPrincipal> GetGroups(GroupPrincipal filter = null)
        {
            PrincipalSearcher searcher = filter == null
                ? new PrincipalSearcher(new GroupPrincipal(Context))
                : new PrincipalSearcher(filter);
            PrincipalSearchResult<Principal> result = searcher.FindAll();
            return result.Cast<GroupPrincipal>().Where(group => group != null);
        }

        public GroupPrincipal GetGroup(GroupPrincipal filter = null)
        {
            PrincipalSearcher searcher = filter == null
                ? new PrincipalSearcher(new GroupPrincipal(Context))
                : new PrincipalSearcher(filter);
            return searcher.FindOne() as GroupPrincipal;
        }

        public IEnumerable<ComputerPrincipal> GetComputers(ComputerPrincipal filter = null)
        {
            PrincipalSearcher searcher = filter == null
                ? new PrincipalSearcher(new ComputerPrincipal(Context))
                : new PrincipalSearcher(filter);
            PrincipalSearchResult<Principal> result = searcher.FindAll();
            return result.Cast<ComputerPrincipal>().Where(computer => computer != null);
        }

        public ComputerPrincipal GetComputer(ComputerPrincipal filter = null)
        {
            PrincipalSearcher searcher = filter == null
                ? new PrincipalSearcher(new ComputerPrincipal(Context))
                : new PrincipalSearcher(filter);
            return searcher.FindOne() as ComputerPrincipal;
        }

        public static DirectoryEntry GetUnderlyingObject(Principal principal)
        {
            return principal.GetUnderlyingObject() as DirectoryEntry;
        }

        public static object GetProperty(Principal principal, string prop)
        {
            DirectoryEntry entry = GetUnderlyingObject(principal);
            return entry.Properties.Contains(prop) ? entry.Properties[prop].Value : null;
        }

        /*
        public static ActiveDirectory Default {
            get {
                if (ad == null) {
                    DirectoryEntry de = new DirectoryEntry("LDAP://RootDSE");
                    string defaultPath = "LDAP://" +
                       de.Properties["defaultNamingContext"][0].
                           ToString();
                    if (ad == null) {
                        ad = new ActiveDirectory();
                        ad.Bind(defaultPath, null, null);
                    }
                }
                return ad;
            }
        }
        */
    }
}