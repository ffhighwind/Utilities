using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Exchange.WebServices.Data;

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
		public const int PageSize = 100;
		private string _Email;
		private string _Mailbox;
		private string _Username;
		private string _Url;

		public EmailService() { }

		public string Email {
			get => _Email;
			set {
				if (IsConnected)
					throw new InvalidOperationException("Already connected.");
				_Email = value;
			}
		}

		public string Username {
			get => _Username;
			set {
				if (IsConnected)
					throw new InvalidOperationException("Already connected.");
				_Username = value;
			}
		}

		public string Url {
			get => _Url;
			set {
				if (IsConnected)
					throw new InvalidOperationException("Already connected.");
				_Url = value;
			}
		}

		public bool IsConnected { get => Service?.Url != null; }

		public ExchangeService Service { get; private set; }

		public bool Connect(string password = null, ExchangeVersion version = ExchangeVersion.Exchange2010_SP1)
		{
			try {
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

		public EmailMessage CreateEmail()
		{
			return new EmailMessage(Service);
		}

		public static void Send(System.Net.Mail.MailMessage email, string host, int port = 0)
		{
			using (System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient(host, port)) {
				client.Send(email);
			}
		}

		public void Send(System.Net.Mail.MailMessage email, int port = 0)
		{
			using (System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient(Service.Url.AbsolutePath, port)) {
				client.Send(email);
			}
		}

		public IEnumerable<Item> FindItems(SearchFilter filter, WellKnownFolderName parentFolder = WellKnownFolderName.Root,
			FolderTraversal traversal = FolderTraversal.Shallow, string mailbox = null)
		{
			FolderId folderId = mailbox == null ? new FolderId(parentFolder) : new FolderId(parentFolder, mailbox);
			foreach (Item item in FindItems(filter, folderId, traversal)) {
				yield return item;
			}
		}

		public IEnumerable<Item> FindItems(SearchFilter filter, FolderId parentFolder, FolderTraversal traversal = FolderTraversal.Shallow)
		{
			FolderView fview = new FolderView(PageSize, 0)
			{
				PropertySet = PropertySet.FirstClassProperties,
				Traversal = traversal,
			};
			FindItemsResults<Item> results = Service.FindItems(parentFolder, fview);
			do {
				foreach (Item item in results) {
					yield return item;
				}
				if (results.NextPageOffset.HasValue)
					fview.Offset = results.NextPageOffset.Value;
			} while (results.MoreAvailable);
		}

		public Folder CreateFolder(string name, FolderId parentFolder)
		{
			Folder folder = FindFolder(name, parentFolder);
			if (folder == null) {
				folder = new Folder(Service)
				{
					DisplayName = name
				};
				folder.Save(parentFolder);
			}
			return folder;
		}

		public Folder CreateFolder(string folderName, WellKnownFolderName parentFolder = WellKnownFolderName.MsgFolderRoot,
			string mailbox = null)
		{
			FolderId folderId = mailbox == null ? new FolderId(parentFolder) : new FolderId(parentFolder, mailbox);
			return CreateFolder(folderName, folderId);
		}

		public Folder FindFolder(FolderId folder, PropertySet properties = null)
		{
			return Folder.Bind(Service, folder, properties ?? PropertySet.FirstClassProperties);
		}

		public Folder FindFolder(WellKnownFolderName folder, string mailbox = null, PropertySet properties = null)
		{
			FolderId folderId = mailbox == null ? new FolderId(folder) : new FolderId(folder, mailbox);
			return Folder.Bind(Service, folderId, properties ?? PropertySet.FirstClassProperties);
		}

		public Folder FindFolder(string folderName, WellKnownFolderName parentFolder = WellKnownFolderName.Root, string mailbox = null,
			FolderTraversal traversal = FolderTraversal.Shallow, PropertySet properties = null)
		{
			FolderId folderId = mailbox == null ? new FolderId(parentFolder) : new FolderId(parentFolder, mailbox);
			return FindFolders(folderName, folderId, traversal, properties).FirstOrDefault();
		}

		public Folder FindFolder(string folderName, FolderId parentFolder, FolderTraversal traversal = FolderTraversal.Shallow,
			PropertySet properties = null)
		{
			return FindFolders(folderName, parentFolder, traversal, properties).FirstOrDefault();
		}

		public IEnumerable<Folder> FindFolders(SearchFilter filter, WellKnownFolderName parentFolder,
			FolderTraversal traversal = FolderTraversal.Shallow, string mailbox = null, PropertySet properties = null)
		{
			FolderId folderId = mailbox == null ? new FolderId(parentFolder) : new FolderId(parentFolder, mailbox);
			foreach (Folder folder in FindFolders(filter, folderId, traversal, properties)) {
				yield return folder;
			}
		}

		public IEnumerable<Folder> FindFolders(SearchFilter filter, FolderId parentFolder,
			FolderTraversal traversal = FolderTraversal.Shallow, PropertySet properties = null)
		{
			FolderView fview = new FolderView(PageSize, 0)
			{
				PropertySet = properties ?? PropertySet.FirstClassProperties,
				Traversal = traversal
			};
			FindFoldersResults results = Service.FindFolders(parentFolder, filter, fview);
			do {
				foreach (Folder folder in results.Folders) {
					yield return folder;
				}
				if (results.NextPageOffset.HasValue)
					fview.Offset = results.NextPageOffset.Value;
			} while (results.MoreAvailable);
		}

		public IEnumerable<Folder> FindFolders(string folderName, WellKnownFolderName parentFolder,
			FolderTraversal traversal = FolderTraversal.Shallow, string mailbox = null, PropertySet properties = null)
		{
			FolderId folderId = mailbox == null ? new FolderId(parentFolder) : new FolderId(parentFolder, mailbox);
			foreach (Folder folder in FindFolders(new SearchFilter.ContainsSubstring(FolderSchema.DisplayName, folderName), folderId, traversal, properties)) {
				yield return folder;
			}
		}

		public IEnumerable<Folder> FindFolders(string folderName, FolderId parentFolder,
			FolderTraversal traversal = FolderTraversal.Shallow, PropertySet properties = null)
		{
			foreach (Folder folder in FindFolders(new SearchFilter.ContainsSubstring(FolderSchema.DisplayName, folderName), parentFolder, traversal, properties)) {
				yield return folder;
			}
		}

		public IEnumerable<Folder> GetFolders(WellKnownFolderName parentFolder,
			FolderTraversal traversal = FolderTraversal.Shallow, string mailbox = null, PropertySet properties = null)
		{
			FolderId folderId = mailbox == null ? new FolderId(parentFolder) : new FolderId(parentFolder, mailbox);
			return GetFolders(folderId, traversal, properties);
		}

		public IEnumerable<Folder> GetFolders(FolderId parentFolder, FolderTraversal traversal = FolderTraversal.Shallow,
			PropertySet properties = null)
		{
			FolderView fview = new FolderView(PageSize, 0)
			{
				PropertySet = properties ?? PropertySet.FirstClassProperties,
				Traversal = traversal
			};
			FindFoldersResults results = Service.FindFolders(parentFolder, fview);
			do {
				foreach (Folder folder in results.Folders) {
					yield return folder;
				}
				if (results.NextPageOffset.HasValue)
					fview.Offset = results.NextPageOffset.Value;
			} while (results.MoreAvailable);
		}

		/// <summary>
		/// Returns a list of <see cref="FileAttachment"/> items contained within a <see cref="Folder"/>.
		/// </summary>
		/// <param name="folderId">The <see cref="FolderId"/> of the <see cref="Folder"/> to search.</param>
		/// <param name="traversal">The search method to perform. By default this is only items within the
		/// initial <see cref="Folder"/>.</param>
		/// <param name="properties">The <see cref="EmailMessageSchema"/> details to load into the emails returned.
		/// By default this is <see cref="PropertySet.FirstClassProperties"/>.
		/// Use <see cref="PropertySets.EmailAll"/> or Load() on the <see cref="EmailMessage"/> to get more details.</param>
		/// <returns></returns>
		public IEnumerable<FileAttachment> GetFileAttachments(FolderId folderId, ItemTraversal traversal = ItemTraversal.Shallow,
			PropertySet properties = null)
		{
			ItemView iview = new ItemView(PageSize, 0)
			{
				PropertySet = properties ?? PropertySet.FirstClassProperties,
				Traversal = traversal
			};
			iview.OrderBy.Add(ItemSchema.DateTimeReceived, SortDirection.Descending);
			FindItemsResults<Item> results = Service.FindItems(folderId, iview);
			do {
				foreach (Item item in results) {
					foreach (FileAttachment file in GetNestedFileAttachments(item)) {
						yield return file;
					}
				}
				if (results.NextPageOffset.HasValue)
					iview.Offset = results.NextPageOffset.Value;
			} while (results.MoreAvailable);
		}

		/// <summary>
		/// Returns a list of <see cref="EmailMessage"/> items contained within a <see cref="Folder"/>.
		/// </summary>
		/// <param name="folderId">The <see cref="FolderId"/> of the <see cref="Folder"/> to search.</param>
		/// <param name="traversal"></param>
		/// <param name="properties">The <see cref="EmailMessageSchema"/> details to load into the emails returned.
		/// By default this is <see cref="PropertySet.FirstClassProperties"/>.
		/// Use <see cref="PropertySets.EmailAll"/> or Load() on the <see cref="EmailMessage"/> to get more details.</param>
		/// <returns>A list of <see cref="EmailMessage"/> items contained within a <see cref="Folder"/>.</returns>
		public IEnumerable<EmailMessage> GetEmails(FolderId folderId, ItemTraversal traversal = ItemTraversal.Shallow,
			PropertySet properties = null)
		{
			ItemView iview = new ItemView(PageSize, 0)
			{
				PropertySet = properties ?? PropertySet.FirstClassProperties,
				Traversal = traversal
			};
			iview.OrderBy.Add(ItemSchema.DateTimeReceived, SortDirection.Descending);
			FindItemsResults<Item> results = Service.FindItems(folderId, new SearchFilter.IsEqualTo(ItemSchema.ItemClass, "IPM.Note"), iview);
			if (results.TotalCount > 0) {
				do {
					foreach (Item email in results) {
						if (email is EmailMessage emailMsg) {
							yield return emailMsg;
						}
					}
					if (results.NextPageOffset.HasValue)
						iview.Offset = results.NextPageOffset.Value;
				} while (results.MoreAvailable);
			}
		}

		public IEnumerable<FileAttachment> GetFileAttachments(EmailMessage email)
		{
			return GetNestedFileAttachments(email);
		}

		/// <summary>
		/// Gets <see cref="FileAttachment"/> items in nested emails.
		/// </summary>
		/// <param name="item">The <see cref="Item"/> to search for <see cref="FileAttachment"/> items.</param>
		/// <returns>A list of <see cref="FileAttachment"/> items contained within the <see cref="Item"/>.</returns>
		private IEnumerable<FileAttachment> GetNestedFileAttachments(Item item)
		{
			if (item.HasAttachments) {
				foreach (Attachment att in item.Attachments) {
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
		/// https://msdn.microsoft.com/en-us/library/office/dn567668(v=exchg.150).aspx
		/// </summary>
		/// <param name="redirectionUrl">The URL to validate.</param>
		/// <returns>True if the the final redirected URL is encrypted. False otherwise.</returns>
		private bool RedirectionUrlValidationCallback(string redirectionUrl)
		{
			return new Uri(redirectionUrl).Scheme == "https";
			////return redirectionUrl.StartsWith("https", StringComparison.OrdinalIgnoreCase);
		}
	}
}
