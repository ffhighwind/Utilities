﻿using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
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
	/// <see cref="https://docs.microsoft.com/en-us/previous-versions/bb384372(v=vs.90)">Customizing Principal classes.</see>
	public class ActiveDirectory : IDisposable
	{
		/// <summary>
		/// https://docs.microsoft.com/en-us/previous-versions/bb384372(v=vs.90)
		/// </summary>
		public ActiveDirectory() { }

		/// <summary>
		/// The <see cref="DirectoryEntry"/> representing the bound context.
		/// </summary>
		public DirectoryEntry Entry { get; private set; } = null;

		/// <summary>
		/// The <see cref="PrincipalContext"/> representing the bound context.
		/// </summary>
		public PrincipalContext Context { get; private set; } = null;

		/// <summary>
		/// Binds to the <see cref="ContextType.Domain"/> from the current user's domain. If no credentials are provided then the user's current credentials will be used.
		/// </summary>
		/// <param name="username">The username to bind with, or null to use the current user's credentials.</param>
		/// <param name="password">The user's password, or null to use the current user's credentials.</param>
		/// <returns>True on success. False otherwise.</returns>
		public bool Bind(string username = null, string password = null)
		{
			return Bind(username, password, ContextType.Domain, Environment.UserDomainName);
		}

		/// <summary>
		/// Binds to the given <see cref="ContextType"/> and path using the default credentials.
		/// </summary>
		/// <param name="contextType">The <see cref="ContextType"/> to bind to.</param>
		/// <param name="path">The path of the context.</param>
		/// <returns>True on success. False otherwise.</returns>
		public bool Bind(ContextType contextType, string path)
		{
			return Bind(null, null, contextType, path);
		}

		/// <summary>
		/// Binds to the given <see cref="ContextType"/> and path using the specified credentials.
		/// </summary>
		/// <param name="username">The username to bind with, or null to use the current user's credentials.</param>
		/// <param name="password">The user's password, or null to use the current user's credentials.</param>
		/// <param name="contextType">The <see cref="ContextType"/> to bind to.</param>
		/// <param name="path">The path of the context.</param>
		/// <returns>True on success. False otherwise.</returns>
		public bool Bind(string username, string password, ContextType contextType, string path)
		{
			try {
				if (username != null && password != null) {
					Entry = new DirectoryEntry(path, username, password);
					Context = new PrincipalContext(contextType, path, username, password);
				}
				else {
					Entry = new DirectoryEntry(path)
					{
						AuthenticationType = AuthenticationTypes.Secure
					};
					Context = new PrincipalContext(contextType, path);
				}
				return true;
			}
			catch (Exception ex) {
				Console.Error.WriteLine("ActiveDirectory.Bind(): " + ex.Message);
				if (Entry != null) {
					Entry.Dispose();
					Entry = null;
				}
			}
			return false;
		}

		/// <summary>
		/// The current user's <see cref="UserPrincipal"/>.
		/// </summary>
		public static UserPrincipal CurrentUser {
			get {
				PrincipalContext context = new PrincipalContext(ContextType.Domain, Environment.UserDomainName);
				return UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, Environment.UserName);
			}
		}

		/// <summary>
		/// The current user's Domain name.
		/// </summary>
		public static string CurrentDomain =>
				////return System.DirectoryServices.ActiveDirectory.Domain.GetCurrentDomain().Name; // can throw an exception
				Environment.UserDomainName;

		/// <summary>
		/// The current computer's Domain name.
		/// </summary>
		/// <exception cref="ActiveDirectoryObjectNotFoundException"/>
		public static string ComputerDomain =>
				////System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
				System.DirectoryServices.ActiveDirectory.Domain.GetComputerDomain().Name; // can throw an exception

		/// <summary>
		/// The current user's account name.
		/// </summary>
		public static string CurrentUserName => Environment.UserName;

		/// <summary>
		/// The current user's <see cref="PrincipalContext"/>.
		/// </summary>
		public static PrincipalContext CurrentContext => new PrincipalContext(ContextType.Domain, CurrentDomain);

		/// <summary>
		/// Searches using the current <see cref="DirectoryEntry"/>.
		/// </summary>
		/// <param name="filter">"(&(objectCategory=User)(objectClass=person)(name=" + name + "))"</param>
		/// <param name="propertiesToLoad">The properties to load.</param>
		/// <returns>The results of the search.</returns>
		/// <see cref="https://docs.microsoft.com/en-us/windows/desktop/adsi/search-filter-syntax"/>
		/// <see cref="https://docs.microsoft.com/en-us/windows/desktop/ADSchema/classes-all"/>
		/// <see cref="https://docs.microsoft.com/en-us/windows/desktop/ad/object-class-and-object-category"/>
		public IEnumerable<SearchResult> Search(string filter, params string[] propertiesToLoad)
		{
			DirectorySearcher searcher = new DirectorySearcher(Entry);
			if (propertiesToLoad.Length > 0)
				searcher.PropertiesToLoad.AddRange(propertiesToLoad);
			if (filter != null)
				searcher.Filter = filter;
			return searcher.FindAll().Cast<SearchResult>();
		}

		/// <summary>
		/// Returns all <see cref="UserPrincipal"/> objects that match the filter. 
		/// By default all <see cref="UserPrincipal"/> objects in the current user's domain are returned.
		/// </summary>
		/// <param name="filter">The filter for the results.</param>
		/// <param name="pageSize">The page size of the searched results.</param>
		/// <returns>All <see cref="UserPrincipal"/> objects that match the filter. By default all users in the current user's domain are returned.</returns>
		/// <see cref="https://docs.microsoft.com/en-us/previous-versions/bb384378(v=vs.90)">Filter example</see>
		public IEnumerable<UserPrincipal> GetUsers(UserPrincipal filter = null, int pageSize = 1000)
		{
			PrincipalSearcher searcher = GetUserSearcher(filter, pageSize);
			PrincipalSearchResult<Principal> result = searcher.FindAll();
			return result.Cast<UserPrincipal>().Where(user => user != null);
		}

		/// <summary>
		/// Returns the first <see cref="UserPrincipal"/> that matches the filter. 
		/// By default all <see cref="UserPrincipal"/> objects in the current user's domain are searched.
		/// </summary>
		/// <param name="filter">The filter for the result.</param>
		/// <returns>The first <see cref="UserPrincipal"/> that matches the filter. By default all users in the current user's domain are searched.</returns>
		/// <see cref="https://docs.microsoft.com/en-us/previous-versions/bb384378(v=vs.90)">example</see>
		public UserPrincipal GetUser(UserPrincipal filter = null)
		{
			using (PrincipalSearcher searcher = new PrincipalSearcher(filter ?? new UserPrincipal(Context))) {
				return searcher.FindOne() as UserPrincipal;
			}
		}

		/// <summary>
		/// Returns all <see cref="GroupPrincipal"/> objects that match the filter. 
		/// By default all <see cref="GroupPrincipal"/> objects in the current user's domain are returned.
		/// </summary>
		/// <param name="filter">The filter for the results.</param>
		/// <param name="pageSize">The page size of the searched results.</param>
		/// <returns>All <see cref="GroupPrincipal"/> objects that match the filter. By default all groups in the current user's domain are returned.</returns>
		/// <see cref="https://docs.microsoft.com/en-us/previous-versions/bb384378(v=vs.90)">Filter example</see>
		public IEnumerable<GroupPrincipal> GetGroups(GroupPrincipal filter = null, int pageSize = 1000)
		{
			PrincipalSearcher searcher = GetGroupSearcher(filter, pageSize);
			PrincipalSearchResult<Principal> result = searcher.FindAll();
			return result.Cast<GroupPrincipal>().Where(group => group != null);
		}

		/// <summary>
		/// Returns the first <see cref="GroupPrincipal"/> that matches the filter. 
		/// By default all <see cref="GroupPrincipal"/> objects in the current user's domain are searched.
		/// </summary>
		/// <param name="filter">The filter for the result.</param>
		/// <returns>The first <see cref="GroupPrincipal"/> that matches the filter. By default all groups in the current user's domain are searched.</returns>
		/// <see cref="https://docs.microsoft.com/en-us/previous-versions/bb384378(v=vs.90)">example</see>
		public GroupPrincipal GetGroup(GroupPrincipal filter = null)
		{
			using (PrincipalSearcher searcher = new PrincipalSearcher(filter ?? new GroupPrincipal(Context))) {
				return searcher.FindOne() as GroupPrincipal;
			}
		}

		/// <summary>
		/// Returns all <see cref="ComputerPrincipal"/> objects that match the filter. 
		/// By default all <see cref="ComputerPrincipal"/> objects in the current user's domain are returned.
		/// </summary>
		/// <param name="filter">The filter for the results.</param>
		/// <param name="pageSize">The page size of the searched results.</param>
		/// <returns>All <see cref="ComputerPrincipal"/> objects that match the filter. By default all computers in the current user's domain are returned.</returns>
		/// <see cref="https://docs.microsoft.com/en-us/previous-versions/bb384378(v=vs.90)">Filter example</see>
		public IEnumerable<ComputerPrincipal> GetComputers(ComputerPrincipal filter = null, int pageSize = 1000)
		{
			PrincipalSearcher searcher = GetComputerSearcher(filter, pageSize);
			PrincipalSearchResult<Principal> result = searcher.FindAll();
			return result.Cast<ComputerPrincipal>().Where(computer => computer != null);
		}

		/// <summary>
		/// Returns the first <see cref="ComputerPrincipal"/> that matches the filter. 
		/// By default all <see cref="ComputerPrincipal"/> objects in the current user's domain are searched.
		/// </summary>
		/// <param name="filter">The filter for the result.</param>
		/// <returns>The first <see cref="ComputerPrincipal"/> that matches the filter. By default all computers in the current user's domain are searched.</returns>
		/// <see cref="https://docs.microsoft.com/en-us/previous-versions/bb384378(v=vs.90)">example</see>
		public ComputerPrincipal GetComputer(ComputerPrincipal filter = null)
		{
			using (PrincipalSearcher searcher = new PrincipalSearcher(filter ?? new ComputerPrincipal(Context))) {
				return searcher.FindOne() as ComputerPrincipal;
			}
		}

		/// <summary>
		/// Returns a <see cref="PrincipalSearcher"/> that that searches for <see cref="UserPrincipal"/> objects with the given filter.
		/// </summary>
		/// <param name="filter">The filter for the result.</param>
		/// <param name="pageSize">The page size of the searched results.</param>
		/// <returns>A <see cref="PrincipalSearcher"/> that that searches for <see cref="UserPrincipal"/> objects with the given filter.</returns>
		/// <see cref="https://docs.microsoft.com/en-us/previous-versions/bb384378(v=vs.90)">example</see>
		public PrincipalSearcher GetUserSearcher(UserPrincipal filter = null, int pageSize = 1000)
		{
			PrincipalSearcher searcher = new PrincipalSearcher(filter ?? new UserPrincipal(Context));
			DirectorySearcher dirSearcher = searcher.GetUnderlyingSearcher() as DirectorySearcher;
			dirSearcher.PageSize = pageSize;
			return searcher;
		}

		/// <summary>
		/// Returns a <see cref="PrincipalSearcher"/> that that searches for <see cref="GroupPrincipal"/> objects with the given filter.
		/// </summary>
		/// <param name="filter">The filter for the result.</param>
		/// <param name="pageSize">The page size of the searched results.</param>
		/// <returns>A <see cref="PrincipalSearcher"/> that that searches for <see cref="GroupPrincipal"/> objects with the given filter.</returns>
		/// <see cref="https://docs.microsoft.com/en-us/previous-versions/bb384378(v=vs.90)">example</see>
		public PrincipalSearcher GetGroupSearcher(GroupPrincipal filter = null, int pageSize = 1000)
		{
			PrincipalSearcher searcher = new PrincipalSearcher(filter ?? new GroupPrincipal(Context));
			DirectorySearcher dirSearcher = searcher.GetUnderlyingSearcher() as DirectorySearcher;
			dirSearcher.PageSize = pageSize;
			return searcher;
		}

		/// <summary>
		/// Returns a <see cref="PrincipalSearcher"/> that that searches for <see cref="ComputerPrincipal"/> objects with the given filter.
		/// </summary>
		/// <param name="filter">The filter for the result.</param>
		/// <param name="pageSize">The page size of the searched results.</param>
		/// <returns>A <see cref="PrincipalSearcher"/> that that searches for <see cref="ComputerPrincipal"/> objects with the given filter.</returns>
		/// <see cref="https://docs.microsoft.com/en-us/previous-versions/bb384378(v=vs.90)">example</see>
		public PrincipalSearcher GetComputerSearcher(ComputerPrincipal filter = null, int pageSize = 1000)
		{
			PrincipalSearcher searcher = new PrincipalSearcher(filter ?? new ComputerPrincipal(Context));
			DirectorySearcher dirSearcher = searcher.GetUnderlyingSearcher() as DirectorySearcher;
			dirSearcher.PageSize = pageSize;
			return searcher;
		}

		public static DirectorySearcher GetUnderlyingSearcher(PrincipalSearcher searcher)
		{
			return searcher.GetUnderlyingSearcher() as DirectorySearcher;
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

		public static DirectoryEntry Root => new DirectoryEntry("LDAP://RootDSE"); //var tmp = "LDAP://" +//  de.Properties["defaultNamingContext"];

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue) {
				if (Entry != null) {
					Entry.Dispose();
					Entry = null;
				}
				if (Context != null) {
					Context.Dispose();
					Context = null;
				}
				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}
		#endregion
	}
}