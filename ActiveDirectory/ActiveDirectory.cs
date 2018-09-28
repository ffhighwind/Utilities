using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;

namespace Utilities.ActiveDirectory
{
    /// <summary>
    /// https://www.codeproject.com/Articles/90142/Everything-in-Active-Directory-via-C-NET-Using
    /// </summary>
    public class ActiveDirectory
    {
        public static string DefaultPath { get; private set; }

        private static ActiveDirectory ad { get; set; }
        public static ActiveDirectory Default {
            get {
                if (ad == null) {
                    DirectoryEntry de = new DirectoryEntry("LDAP://RootDSE");
                    string defaultPath = "LDAP://" +
                       de.Properties["defaultNamingContext"][0].
                           ToString();
                    if (ad == null) {
                        ad = new ActiveDirectory();
                        ad.Create(defaultPath, null, null);
                    }
                }
                return ad;
            }
        }

        public DirectoryEntry Entry { get; private set; }
        public PrincipalContext Context { get; private set; }

        private ActiveDirectory() { }
        public ActiveDirectory(string path, string username, string password)
        {
            Create(path, username, password);
        }

        public void Create(string username, string password)
        {
            Create(null, username, password);
        }

        public void Create(string path, string username, string password)
        {
            path = path ?? DefaultPath;
            if (username != null && password != null) {

                Entry = new DirectoryEntry(path, username, password);
                Context = new PrincipalContext(ContextType.Domain, path, username, password);
            }
            else {
                Context = new PrincipalContext(ContextType.Domain, path);
                Entry = new DirectoryEntry(path);
                Entry.AuthenticationType = AuthenticationTypes.Secure;
            }
            //Entry.UsePropertyCache = true;
        }

        /// <summary>
        /// https://docs.microsoft.com/en-us/windows/desktop/adsi/search-filter-syntax
        /// https://docs.microsoft.com/en-us/windows/desktop/ADSchema/classes-all
        /// https://docs.microsoft.com/en-us/windows/desktop/ad/object-class-and-object-category
        /// </summary>
        /// <param name="filter">"(&(objectCategory=User)(objectClass=person)(name=" + name + "))"</param>
        /// <param name="propertiesToLoad"></param>
        /// <returns></returns>
        public IEnumerable<SearchResult> Search(string filter, params string[] propertiesToLoad)
        {
            return Searcher(filter, propertiesToLoad).FindAll().Cast<SearchResult>();
        }

        public DirectorySearcher Searcher(string filter, params string[] props)
        {
            DirectorySearcher searcher = new DirectorySearcher(Entry);
            searcher.PropertiesToLoad.AddRange(props);
            return searcher;
        }

        public IEnumerable<UserPrincipal> GetUsers(UserPrincipal filter = null)
        {
            PrincipalSearcher searcher = new PrincipalSearcher(new UserPrincipal(Context));
            if (filter != null) {
                searcher.QueryFilter = filter;
            }
            PrincipalSearchResult<Principal> result = searcher.FindAll();
            return result.Cast<UserPrincipal>().Where(user => user != null);
        }

        public UserPrincipal GetUser(UserPrincipal filter = null)
        {
            return GetUsers(filter).FirstOrDefault();
        }

        public IEnumerable<GroupPrincipal> GetGroups(GroupPrincipal filter = null)
        {
            PrincipalSearcher searcher = new PrincipalSearcher(new GroupPrincipal(Context));
            if (filter != null) {
                searcher.QueryFilter = filter;
            }
            PrincipalSearchResult<Principal> result = searcher.FindAll();
            return result.Cast<GroupPrincipal>().Where(group => group != null);
        }

        public GroupPrincipal GetGroup(GroupPrincipal filter = null)
        {
            return GetGroups(filter).FirstOrDefault();
        }

        public IEnumerable<ComputerPrincipal> GetComputers(ComputerPrincipal filter = null)
        {
            PrincipalSearcher searcher = new PrincipalSearcher(new ComputerPrincipal(Context));
            if (filter != null) {
                searcher.QueryFilter = filter;
            }
            PrincipalSearchResult<Principal> result = searcher.FindAll();
            return result.Cast<ComputerPrincipal>().Where(computer => computer != null);
        }

        public ComputerPrincipal GetComputer(ComputerPrincipal filter = null)
        {
            return GetComputers(filter).FirstOrDefault();
        }

        public static DirectoryEntry GetUnderlyingObject(Principal principal)
        {
            return (DirectoryEntry) principal.GetUnderlyingObject();
        }

        public static string GetProperty(Principal principal, string prop)
        {
            DirectoryEntry entry = GetUnderlyingObject(principal);
            return entry.Properties.Contains(prop) ? entry.Properties[prop].Value.ToString() : null;
        }
    }
}