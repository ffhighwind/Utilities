using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Exchange.WebServices.Data;
using System.Net.Mail;

namespace Utilities.Email
{
	/// <summary>
	/// EWS Managed API Documentation:
	/// https://docs.microsoft.com/en-us/previous-versions/office/developer/exchange-server-2010/dd633710(v=exchg.80)
	/// https://docs.microsoft.com/en-us/exchange/client-developer/exchange-server-development
	/// </summary>
	public class EmailService
	{
		/// <summary>
		/// The maximum number of items requested at a time. The maximum this can be is 1000.
		/// </summary>
		public const int PageSize = 200;
		private string _Email;
		private string _Username;
		private string _Url;

		/// <summary>
		/// Constructs a new <see cref="EmailService"/> using the given credentials.
		/// </summary>
		/// <param name="url">The host <see cref="Uri"/> to connect to. If this is <see langword="null"/> then the service will attempt to auto-discover the host.</param>
		/// <param name="email">The email address to connect with.</param>
		/// <param name="username">The username to connect with. If this is <see langword="null"/> then <see cref="Email"/> will be used as the username.</param>
		public EmailService(string url = null, string email = null, string username = null)
		{
			_Url = url;
			_Email = email;
			_Username = username;
		}

		/// <summary>
		/// The email address to connect with.
		/// </summary>
		public string Email {
			get => _Email;
			set {
				if (IsConnected)
					throw new InvalidOperationException("Already connected.");
				_Email = value;
			}
		}

		/// <summary>
		/// The username to connect with. If this is <see langword="null"/> then <see cref="Email"/> will be used as the username.
		/// </summary>
		public string Username {
			get => _Username;
			set {
				if (IsConnected)
					throw new InvalidOperationException("Already connected.");
				_Username = value;
			}
		}

		/// <summary>
		/// The host <see cref="Uri"/> to connect to. If this is <see langword="null"/> then the service will attempt to auto-discover the host.
		/// </summary>
		public string Url {
			get => _Url;
			set {
				if (IsConnected)
					throw new InvalidOperationException("Already connected.");
				_Url = value;
			}
		}

		/// <summary>
		/// True if connection has been established. False otherwise.
		/// </summary>
		public bool IsConnected { get => Service?.Url != null; }

		/// <summary>
		/// The <see cref="ExchangeService"/> or <see langword="null"/> if no connection has been established.
		/// </summary>
		public ExchangeService Service { get; private set; }

		/// <summary>
		/// Connects to an <see cref="ExchangeService"/> using the given credentials.
		/// </summary>
		/// <param name="email">The email address to connect with.</param>
		/// <param name="password">The password to connect with.</param>
		/// <param name="version">The <see cref="ExchangeVersion"/> to connect with.</param>
		/// <returns>True if connection was established. False otherwise.</returns>
		public bool Connect(string email, string password, ExchangeVersion version = ExchangeVersion.Exchange2010_SP1)
		{
			return Connect(_Url, email, _Username, password, version);
		}

		/// <summary>
		/// Connects to an <see cref="ExchangeService"/> using the given credentials.
		/// </summary>
		/// <param name="url">The host <see cref="Uri"/> to connect to. If this is <see langword="null"/> then the service will attempt to auto-discover the host.</param>
		/// <param name="email">The email address to connect with.</param>
		/// <param name="username">The username to connect with. If this is <see langword="null"/> then <see cref="Email"/> will be used as the username.</param>
		/// <param name="password">The password to connect with.</param>
		/// <param name="version">The <see cref="ExchangeVersion"/> to connect with.</param>
		/// <returns>True if connection was established. False otherwise.</returns>
		public bool Connect(string url, string email, string username, string password = null, ExchangeVersion version = ExchangeVersion.Exchange2010_SP1)
		{
			Url = url;
			try {
				_Email = email;
				_Username = username;
				Service = new ExchangeService(version)
				{
					Credentials = new WebCredentials(Username ?? Email, password),
					UseDefaultCredentials = string.IsNullOrWhiteSpace(password)
				};
				if (_Url != null)
					Service.Url = new Uri(_Url);
				else
					Service.AutodiscoverUrl(Email, RedirectionUrlValidationCallback);
			}
			catch (Exception ex) {
				Console.Error.WriteLine(ex.Message);
				Service = null;
			}
			return IsConnected;
		}

		/// <summary>
		/// Connects to an <see cref="ExchangeService"/> using the given credentials.
		/// </summary>
		/// <param name="password">The password to connect with.</param>
		/// <param name="version">The <see cref="ExchangeVersion"/> to connect with.</param>
		/// <returns>True if connection was established. False otherwise.</returns>
		public bool Connect(string password = null, ExchangeVersion version = ExchangeVersion.Exchange2010_SP1)
		{
			return Connect(_Url, _Email, _Username, password, version);
		}

		/// <summary>
		/// Creates a new <see cref="EmailMessage"/>.
		/// </summary>
		/// <returns>A new <see cref="EmailMessage"/>.</returns>
		public EmailMessage CreateEmail()
		{
			return new EmailMessage(Service);
		}

		/// <summary>
		/// Sends a <see cref="MailMessage"/> using the specified host <see cref="Uri"/> and port.
		/// </summary>
		/// <param name="email">The <see cref="MailMessage"/> to send.</param>
		/// <param name="host">The host <see cref="Uri"/> to send the <see cref="MailMessage"/> from.</param>
		/// <param name="port">The port to send the <see cref="MailMessage"/> using. By default this will use the application settings.</param>
		public static void Send(MailMessage email, string host, int port = 0)
		{
			using (SmtpClient client = new SmtpClient(host, port)) {
				client.Send(email);
			}
		}

		/// <summary>
		/// Sends a <see cref="MailMessage"/> using the <see cref="ExchangeService.Url"/>.
		/// </summary>
		/// <param name="email">The <see cref="MailMessage"/> to send.</param>
		/// <param name="port">The port to send the <see cref="MailMessage"/> using. By default this will use the application settings.</param>
		public void Send(MailMessage email, int port = 0)
		{
			using (SmtpClient client = new SmtpClient(Service.Url.AbsolutePath, port)) {
				client.Send(email);
			}
		}

		/// <summary>
		/// Returns a list of <see cref="Item"/>s contained within a <see cref="WellKnownFolderName"/>.
		/// </summary>
		/// <param name="filter">The <see cref="SearchFilter"/> to perform.</param>
		/// <param name="parentFolder">The <see cref="FolderId"/> of the <see cref="Folder"/> to search.</param>
		/// <param name="traversal">The <see cref="ItemTraversal"/> method to perform. By default this is only items within the
		/// initial <see cref="Folder"/>.</param>
		/// <param name="mailbox">The mailbox to search, or <see langword="null"/> to search the default mailbox.</param>
		/// <returns>A list of <see cref="Item"/>s contained within a <see cref="WellKnownFolderName"/>.</returns>
		public IEnumerable<Item> FindItems(SearchFilter filter, WellKnownFolderName parentFolder = WellKnownFolderName.Root,
			FolderTraversal traversal = FolderTraversal.Shallow, string mailbox = null)
		{
			FolderId folderId = mailbox == null ? new FolderId(parentFolder) : new FolderId(parentFolder, mailbox);
			foreach (Item item in FindItems(filter, folderId, traversal)) {
				yield return item;
			}
		}

		/// <summary>
		/// Returns a list of <see cref="Item"/>s contained within a <see cref="Folder"/>.
		/// </summary>
		/// <param name="filter">The <see cref="SearchFilter"/> to perform.</param>
		/// <param name="parentFolder">The <see cref="FolderId"/> of the <see cref="Folder"/> to search.</param>
		/// <param name="traversal">The <see cref="ItemTraversal"/> method to perform. By default this is only items within the
		/// initial <see cref="Folder"/>.</param>
		/// <returns>A list of <see cref="Item"/>s contained within a <see cref="Folder"/>.</returns>
		public IEnumerable<Item> FindItems(SearchFilter filter, FolderId parentFolder, FolderTraversal traversal = FolderTraversal.Shallow)
		{
			FolderView fview = new FolderView(PageSize, 0)
			{
				PropertySet = PropertySet.FirstClassProperties,
				Traversal = traversal,
			};

			while (true) {
				FindItemsResults<Item> results = Service.FindItems(parentFolder, fview);
				foreach (Item item in results) {
					yield return item;
				}
				if (results.NextPageOffset.HasValue)
					fview.Offset = results.NextPageOffset.Value;
				else
					fview.Offset += PageSize;
				if (!results.MoreAvailable)
					break;
			}
		}

		/// <summary>
		/// Creates a new <see cref="Folder"/> in a specified <see cref="FolderId"/>. If the <see cref="Folder"/> already exists then it will be returned.
		/// </summary>
		/// <param name="name">The name of the new <see cref="Folder"/>.</param>
		/// <param name="parentFolder">The <see cref="FolderId"/> to create the the <see cref="Folder"/> in.</param>
		/// <returns>A new <see cref="Folder"/> in a specified <see cref="FolderId"/>. If the <see cref="Folder"/> already exists then it will be returned.</returns>
		public Folder CreateFolder(string folderName, FolderId parentFolder)
		{
			Folder folder = FindFolder(folderName, parentFolder);
			if (folder == null) {
				folder = new Folder(Service)
				{
					DisplayName = folderName
				};
				folder.Save(parentFolder);
			}
			return folder;
		}

		/// <summary>
		/// Creates a new <see cref="Folder"/> in a <see cref="WellKnownFolderName"/>. If the <see cref="Folder"/> already exists then it will be returned.
		/// </summary>
		/// <param name="folderName">The name of the new <see cref="Folder"/>.</param>
		/// <param name="parentFolder">The <see cref="WellKnownFolderName"/> to create the the <see cref="Folder"/> in.</param>
		/// <param name="mailbox">The mailbox to create the <see cref="Folder"/> in, or <see langword="null"/> to create it in the default mailbox.</param>
		/// <returns>A new <see cref="Folder"/> in <see cref="WellKnownFolderName"/>. If the <see cref="Folder"/> already exists then it will be returned.</returns>
		public Folder CreateFolder(string folderName, WellKnownFolderName parentFolder = WellKnownFolderName.MsgFolderRoot,
			string mailbox = null)
		{
			FolderId folderId = mailbox == null ? new FolderId(parentFolder) : new FolderId(parentFolder, mailbox);
			return CreateFolder(folderName, folderId);
		}

		/// <summary>
		/// Returns the <see cref="Folder"/> matching the <see cref="FolderId"/>.
		/// </summary>
		/// <param name="folder">The <see cref="FolderId"/> to find.</param>
		/// <param name="properties">The <see cref="EmailMessageSchema"/> details to load into the emails returned.
		/// By default this is <see cref="PropertySet.FirstClassProperties"/>.</param>
		/// <returns>The <see cref="Folder"/> matching the <see cref="FolderId"/>.</returns>
		public Folder FindFolder(FolderId folder, PropertySet properties = null)
		{
			return Folder.Bind(Service, folder, properties ?? PropertySet.FirstClassProperties);
		}

		/// <summary>
		/// Returns the <see cref="WellKnownFolderName"/>.
		/// </summary>
		/// <param name="folder">The <see cref="WellKnownFolderName"/> to find.</param>
		/// <param name="mailbox">The mailbox to search, or <see langword="null"/> to search the default mailbox.</param>
		/// <param name="properties">The <see cref="EmailMessageSchema"/> details to load into the emails returned.
		/// By default this is <see cref="PropertySet.FirstClassProperties"/>.</param>
		/// <returns>The <see cref="WellKnownFolderName"/>.</returns>
		public Folder FindFolder(WellKnownFolderName folder, string mailbox = null, PropertySet properties = null)
		{
			FolderId folderId = mailbox == null ? new FolderId(folder) : new FolderId(folder, mailbox);
			return Folder.Bind(Service, folderId, properties ?? PropertySet.FirstClassProperties);
		}

		/// <summary>
		/// Returns a <see cref="Folder"/> contained within a <see cref="WellKnownFolderName"/> or <see langword="null"/> if it was not found.
		/// </summary>
		/// <param name="folderName">The name of the <see cref="Folder"/>.</param>
		/// <param name="parentFolder">The <see cref="WellKnownFolderName"/> of the <see cref="Folder"/> to search.</param>
		/// <param name="traversal">The <see cref="ItemTraversal"/> method to perform. By default this is only items within the
		/// initial <see cref="Folder"/>.</param>
		/// <param name="properties">The <see cref="EmailMessageSchema"/> details to load into the emails returned.
		/// By default this is <see cref="PropertySet.FirstClassProperties"/>.</param>
		/// <returns>A <see cref="Folder"/> contained within a <see cref="WellKnownFolderName"/> or <see langword="null"/> if it was not found.</returns>
		public Folder FindFolder(string folderName, WellKnownFolderName parentFolder = WellKnownFolderName.Root, string mailbox = null,
			FolderTraversal traversal = FolderTraversal.Shallow, PropertySet properties = null)
		{
			FolderId folderId = mailbox == null ? new FolderId(parentFolder) : new FolderId(parentFolder, mailbox);
			return FindFolders(folderName, folderId, traversal, properties).FirstOrDefault();
		}

		/// <summary>
		/// Returns a <see cref="Folder"/> contained within another <see cref="Folder"/> or <see langword="null"/> if it was not found.
		/// </summary>
		/// <param name="folderName">The name of the <see cref="Folder"/>.</param>
		/// <param name="parentFolder">The <see cref="FolderId"/> of the <see cref="Folder"/> to search.</param>
		/// <param name="traversal">The <see cref="ItemTraversal"/> method to perform. By default this is only items within the
		/// initial <see cref="Folder"/>.</param>
		/// <param name="properties">The <see cref="EmailMessageSchema"/> details to load into the emails returned.
		/// By default this is <see cref="PropertySet.FirstClassProperties"/>.</param>
		/// <returns>A <see cref="Folder"/> contained within another <see cref="Folder"/> or <see langword="null"/> if it was not found.</returns>
		public Folder FindFolder(string folderName, FolderId parentFolder, FolderTraversal traversal = FolderTraversal.Shallow,
			PropertySet properties = null)
		{
			return FindFolders(folderName, parentFolder, traversal, properties).FirstOrDefault();
		}

		/// <summary>
		/// Returns a list of <see cref="Folder"/> items contained within a <see cref="WellKnownFolderName"/>.
		/// </summary>
		/// <param name="filter">The <see cref="SearchFilter"/> to perform.</param>
		/// <param name="parentFolder">The <see cref="WellKnownFolderName"/> of the <see cref="Folder"/> to search.</param>
		/// <param name="traversal">The <see cref="ItemTraversal"/> method to perform. By default this is only items within the
		/// initial <see cref="Folder"/>.</param>
		/// <param name="properties">The <see cref="EmailMessageSchema"/> details to load into the emails returned.
		/// By default this is <see cref="PropertySet.FirstClassProperties"/>.</param>
		/// <returns>A list of <see cref="Folder"/> items contained within a <see cref="WellKnownFolderName"/>.</returns>
		public IEnumerable<Folder> FindFolders(SearchFilter filter, WellKnownFolderName parentFolder,
			FolderTraversal traversal = FolderTraversal.Shallow, string mailbox = null, PropertySet properties = null)
		{
			FolderId folderId = mailbox == null ? new FolderId(parentFolder) : new FolderId(parentFolder, mailbox);
			foreach (Folder folder in FindFolders(filter, folderId, traversal, properties)) {
				yield return folder;
			}
		}

		/// <summary>
		/// Returns a list of <see cref="Folder"/> items contained within another <see cref="Folder"/>.
		/// </summary>
		/// <param name="filter">The <see cref="SearchFilter"/> to perform.</param>
		/// <param name="parentFolder">The <see cref="FolderId"/> of the <see cref="Folder"/> to search.</param>
		/// <param name="traversal">The <see cref="ItemTraversal"/> method to perform. By default this is only items within the
		/// initial <see cref="Folder"/>.</param>
		/// <param name="properties">The <see cref="EmailMessageSchema"/> details to load into the emails returned.
		/// By default this is <see cref="PropertySet.FirstClassProperties"/>.</param>
		/// <returns>A list of <see cref="Folder"/> items contained within another <see cref="Folder"/>.</returns>
		public IEnumerable<Folder> FindFolders(SearchFilter filter, FolderId parentFolder,
			FolderTraversal traversal = FolderTraversal.Shallow, PropertySet properties = null)
		{
			FolderView fview = new FolderView(PageSize, 0)
			{
				PropertySet = properties ?? PropertySet.FirstClassProperties,
				Traversal = traversal
			};
			while (true) {
				FindFoldersResults results = Service.FindFolders(parentFolder, filter, fview);
				foreach (Folder folder in results.Folders) {
					yield return folder;
				}
				if (results.NextPageOffset.HasValue)
					fview.Offset = results.NextPageOffset.Value;
				else
					fview.Offset += PageSize;
				if (!results.MoreAvailable)
					break;
			}
		}

		/// <summary>
		/// Returns a list of <see cref="Folder"/> items contained within a <see cref="WellKnownFolderName"/>.
		/// </summary>
		/// <param name="folderName">The name of the <see cref="Folder"/>.</param>
		/// <param name="parentFolder">The <see cref="WellKnownFolderName"/> of the <see cref="Folder"/> to search.</param>
		/// <param name="traversal">The <see cref="ItemTraversal"/> method to perform. By default this is only items within the
		/// initial <see cref="Folder"/>.</param>
		/// <param name="properties">The <see cref="EmailMessageSchema"/> details to load into the emails returned.
		/// By default this is <see cref="PropertySet.FirstClassProperties"/>.</param>
		/// <returns>A list of <see cref="Folder"/> items contained within a <see cref="WellKnownFolderName"/>.</returns>
		public IEnumerable<Folder> FindFolders(string folderName, WellKnownFolderName parentFolder,
			FolderTraversal traversal = FolderTraversal.Shallow, string mailbox = null, PropertySet properties = null)
		{
			FolderId folderId = mailbox == null ? new FolderId(parentFolder) : new FolderId(parentFolder, mailbox);
			foreach (Folder folder in FindFolders(new SearchFilter.ContainsSubstring(FolderSchema.DisplayName, folderName), folderId, traversal, properties)) {
				yield return folder;
			}
		}

		/// <summary>
		/// Returns a list of <see cref="Folder"/> items contained within another <see cref="Folder"/>.
		/// </summary>
		/// <param name="folderName">The name of the <see cref="Folder"/>.</param>
		/// <param name="parentFolder">The <see cref="FolderId"/> of the <see cref="Folder"/> to search.</param>
		/// <param name="traversal">The <see cref="ItemTraversal"/> method to perform. By default this is only items within the
		/// initial <see cref="Folder"/>.</param>
		/// <param name="properties">The <see cref="EmailMessageSchema"/> details to load into the emails returned.
		/// By default this is <see cref="PropertySet.FirstClassProperties"/>.</param>
		/// <returns>A list of <see cref="Folder"/> items contained within another <see cref="Folder"/>.</returns>
		public IEnumerable<Folder> FindFolders(string folderName, FolderId parentFolder,
			FolderTraversal traversal = FolderTraversal.Shallow, PropertySet properties = null)
		{
			foreach (Folder folder in FindFolders(new SearchFilter.ContainsSubstring(FolderSchema.DisplayName, folderName), parentFolder, traversal, properties)) {
				yield return folder;
			}
		}

		/// <summary>
		/// Returns a list of <see cref="Folder"/> items contained within a <see cref="WellKnownFolderName"/>.
		/// </summary>
		/// <param name="parentFolder">The <see cref="WellKnownFolderName"/> of the <see cref="Folder"/> to search.</param>
		/// <param name="traversal">The <see cref="ItemTraversal"/> method to perform. By default this is only items within the
		/// initial <see cref="Folder"/>.</param>
		/// <param name="properties">The <see cref="EmailMessageSchema"/> details to load into the emails returned.
		/// By default this is <see cref="PropertySet.FirstClassProperties"/>.</param>
		/// <returns>A list of <see cref="Folder"/> items contained within a <see cref="WellKnownFolderName"/>.</returns>
		public IEnumerable<Folder> GetFolders(WellKnownFolderName parentFolder,
			FolderTraversal traversal = FolderTraversal.Shallow, string mailbox = null, PropertySet properties = null)
		{
			FolderId folderId = mailbox == null ? new FolderId(parentFolder) : new FolderId(parentFolder, mailbox);
			return GetFolders(folderId, traversal, properties);
		}

		/// <summary>
		/// Returns a list of <see cref="Folder"/> items contained within another <see cref="Folder"/>.
		/// </summary>
		/// <param name="parentFolder">The <see cref="FolderId"/> of the <see cref="Folder"/> to search.</param>
		/// <param name="traversal">The <see cref="ItemTraversal"/> method to perform. By default this is only items within the
		/// initial <see cref="Folder"/>.</param>
		/// <param name="properties">The <see cref="EmailMessageSchema"/> details to load into the emails returned.
		/// By default this is <see cref="PropertySet.FirstClassProperties"/>.</param>
		/// <returns>A list of <see cref="Folder"/> items contained within another <see cref="Folder"/>.</returns>
		public IEnumerable<Folder> GetFolders(FolderId parentFolder, FolderTraversal traversal = FolderTraversal.Shallow,
			PropertySet properties = null)
		{
			FolderView fview = new FolderView(PageSize, 0)
			{
				PropertySet = properties ?? PropertySet.FirstClassProperties,
				Traversal = traversal
			};
			while (true) {
				FindFoldersResults results = Service.FindFolders(parentFolder, fview);
				foreach (Folder folder in results.Folders) {
					yield return folder;
				}
				if (results.NextPageOffset.HasValue)
					fview.Offset = results.NextPageOffset.Value;
				else
					fview.Offset += PageSize;
				if (!results.MoreAvailable)
					break;
			}
		}

		/// <summary>
		/// Returns a list of <see cref="FileAttachment"/> items contained within another <see cref="Folder"/>.
		/// </summary>
		/// <param name="folderId">The <see cref="FolderId"/> of the <see cref="Folder"/> to search.</param>
		/// <param name="traversal">The <see cref="ItemTraversal"/> method to perform. By default this is only items within the
		/// initial <see cref="Folder"/>.</param>
		/// <param name="properties">The <see cref="EmailMessageSchema"/> details to load into the emails returned.
		/// By default this is <see cref="PropertySet.FirstClassProperties"/>.
		/// Use <see cref="PropertySets.EmailAll"/> or Load() on the <see cref="EmailMessage"/> to get more details.</param>
		/// <returns>A list of <see cref="FileAttachment"/> items contained within another <see cref="Folder"/>.</returns>
		public IEnumerable<FileAttachment> GetFileAttachments(FolderId folderId, ItemTraversal traversal = ItemTraversal.Shallow,
			PropertySet properties = null)
		{
			ItemView iview = new ItemView(PageSize, 0)
			{
				PropertySet = properties ?? PropertySet.FirstClassProperties,
				Traversal = traversal
			};
			iview.OrderBy.Add(ItemSchema.DateTimeReceived, SortDirection.Descending);
			while (true) {
				FindItemsResults<Item> results = Service.FindItems(folderId, iview);
				foreach (Item item in results) {
					foreach (FileAttachment file in GetNestedFileAttachments(item)) {
						yield return file;
					}
				}
				if (results.NextPageOffset.HasValue)
					iview.Offset = results.NextPageOffset.Value;
				else
					iview.Offset += PageSize;
				if (!results.MoreAvailable)
					break;
			}
		}

		/// <summary>
		/// Returns a list of <see cref="EmailMessage"/> items contained within another <see cref="Folder"/>.
		/// </summary>
		/// <param name="folderId">The <see cref="FolderId"/> of the <see cref="Folder"/> to search.</param>
		/// <param name="traversal"></param>
		/// <param name="properties">The <see cref="EmailMessageSchema"/> details to load into the emails returned.
		/// By default this is <see cref="PropertySet.FirstClassProperties"/>.
		/// Use <see cref="PropertySets.EmailAll"/> or Load() on the <see cref="EmailMessage"/> to get more details.</param>
		/// <returns>A list of <see cref="EmailMessage"/> items contained within another <see cref="Folder"/>.</returns>
		public IEnumerable<EmailMessage> GetEmails(FolderId folderId, ItemTraversal traversal = ItemTraversal.Shallow,
			PropertySet properties = null)
		{
			ItemView iview = new ItemView(PageSize, 0)
			{
				PropertySet = properties ?? PropertySet.FirstClassProperties,
				Traversal = traversal
			};
			iview.OrderBy.Add(ItemSchema.DateTimeReceived, SortDirection.Descending);
			var searchFilter = new SearchFilter.IsEqualTo(ItemSchema.ItemClass, "IPM.Note");
			while (true) {
				FindItemsResults<Item> results = Service.FindItems(folderId, searchFilter, iview);
				foreach (Item email in results) {
					if (email is EmailMessage emailMsg) {
						yield return emailMsg;
					}
				}
				if (results.NextPageOffset.HasValue)
					iview.Offset = results.NextPageOffset.Value;
				else
					iview.Offset += PageSize;
				if (!results.MoreAvailable)
					break;
			}
		}

		/// <summary>
		/// Returns a list of <see cref="FileAttachment"/> items contained within an <see cref="EmailMessage"/>.
		/// </summary>
		/// <param name="email">The <see cref="EmailMessage"/> to obtain <see cref="FileAttachment"/> items from</param>.
		/// <returns>A list of <see cref="FileAttachment"/> items contained within an <see cref="EmailMessage"/>.</returns>
		public IEnumerable<FileAttachment> GetFileAttachments(EmailMessage email)
		{
			return GetNestedFileAttachments(email);
		}

		/// <summary>
		/// Returns a list of nested <see cref="FileAttachment"/> items within an <see cref="Item"/>.
		/// </summary>
		/// <param name="item">The <see cref="Item"/> to search for <see cref="FileAttachment"/> items.</param>
		/// <returns>A list of nested <see cref="FileAttachment"/> items within an <see cref="Item"/>.</returns>
		private IEnumerable<FileAttachment> GetNestedFileAttachments(Item item)
		{
			if (item.HasAttachments) {
				item.Load();
				foreach (Microsoft.Exchange.WebServices.Data.Attachment att in item.Attachments) {
					if (att is ItemAttachment iatt) {
						if (iatt.Item == null)
							att.Load();
						foreach (FileAttachment file in GetNestedFileAttachments(iatt.Item)) {
							yield return file;
						}
					}
					else if (att is FileAttachment fileAttachment)
						yield return fileAttachment;
					else
						throw new InvalidCastException("Unknown attachment type: " + att.GetType().FullName);
				}
			}
		}

		/// <summary>
		/// Standardized redirection validation of the URL from the Exchange examples.
		/// </summary>
		/// <param name="redirectionUrl">The URL to validate.</param>
		/// <returns>True if the the final redirected URL is encrypted. False otherwise.</returns>
		/// <see cref="https://msdn.microsoft.com/en-us/library/office/dn567668(v=exchg.150).aspx"/>
		private bool RedirectionUrlValidationCallback(string redirectionUrl)
		{
			return new Uri(redirectionUrl).Scheme == "https";
			////return redirectionUrl.StartsWith("https", StringComparison.OrdinalIgnoreCase);
		}
	}
}
