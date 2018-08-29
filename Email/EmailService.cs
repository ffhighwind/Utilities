using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Exchange.WebServices.Data;

namespace Utilities.Email
{
    /// <summary>
    /// /// EWS Managed API Documentation:
    /// https://docs.microsoft.com/en-us/previous-versions/office/developer/exchange-server-2010/dd633710(v=exchg.80)
    /// </summary>
    public class EmailService
    {
        public const int PageSize = 50;

        public EmailService() { }

        public EmailService(string email)
            : this(email, null, null) { }

        public EmailService(string email, string username, string password)
        {
            Email = email;
            Username = username;
            Password = password;
        }

        public string Email { get; private set; }

        public string Username { get; private set; }

        public bool IsConnected { get; private set; }

        public string Password { protected get; set; }

        public ExchangeService Service { get; private set; }

        public bool Connect(string email, ExchangeVersion version = ExchangeVersion.Exchange2010_SP1)
        {
            Email = email;
            return Connect(version);
        }

        public bool Connect(string email, string username, string password, ExchangeVersion version = ExchangeVersion.Exchange2010_SP1)
        {
            Email = email;
            Username = username;
            Password = password;
            return Connect(version);
        }

        /// <summary>
        /// Connect to the Exchange server with the given credentials.
        /// </summary>
        /// <param name="version">The version of the Exchange Service you are connecting to.</param>
        public bool Connect(ExchangeVersion version = ExchangeVersion.Exchange2010_SP1)
        {
            try {
                // This will initialize the exchange web service object
                Service = new ExchangeService(version) {
                    Credentials = new WebCredentials(Username, Password),
                    UseDefaultCredentials = string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password)
                };
                Service.AutodiscoverUrl(Email, RedirectionUrlValidationCallback);
                IsConnected = Service.Url != null;
            }
            catch (Exception ex) {
                Console.Error.WriteLine(ex.Message);
                IsConnected = false;
            }
            return IsConnected;
        }

        public EmailMessage CreateEmail(string recipient, string subject, params string[] files)
        {
            EmailMessage email = new EmailMessage(Service) { Subject = subject };
            email.ToRecipients.Add(new EmailAddress(recipient));
            foreach (string file in files) {
                email.Attachments.AddFileAttachment(file);
            }
            return email;
        }

        public EmailMessage CreateEmail()
        {
            return new EmailMessage(Service);
        }

        public bool FolderExists(string folderName, FolderId parentFolder, FolderTraversal traversal = FolderTraversal.Shallow)
        {
            return FindItems(new SearchFilter.ContainsSubstring(FolderSchema.DisplayName, folderName), parentFolder, traversal).Any();
        }

        public bool FolderExists(string folderName, WellKnownFolderName parentFolder = WellKnownFolderName.Root,
            FolderTraversal traversal = FolderTraversal.Shallow, string mailbox = null)
        {
            FolderId folderId = mailbox == null ? new FolderId(parentFolder) : new FolderId(parentFolder, mailbox);
            return FindItems(new SearchFilter.ContainsSubstring(FolderSchema.DisplayName, folderName), folderId, traversal).Any();
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
            FolderView fview = new FolderView(PageSize, 0) {
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
                folder = new Folder(Service) {
                    DisplayName = name
                };
                folder.Save(parentFolder);
            }
            return folder;
        }

        public Folder CreateFolder(string folderName, WellKnownFolderName parentFolder = WellKnownFolderName.MsgFolderRoot, string mailbox = null)
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
            return Folder.Bind(Service, folder, properties ?? PropertySet.FirstClassProperties);
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
            FolderView fview = new FolderView(PageSize, 0) {
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
            FolderView fview = new FolderView(PageSize, 0) {
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

        public IEnumerable<FileAttachment> GetFileAttachments(FolderId folderId, ItemTraversal traversal = ItemTraversal.Shallow, PropertySet properties = null)
        {
            ItemView iview = new ItemView(PageSize, 0) {
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

        public IEnumerable<EmailMessage> GetEmails(FolderId folderId, PropertySet properties = null)
        {
            if (properties == null)
                properties = PropertySet.FirstClassProperties;
            ItemView iview = new ItemView(PageSize, 0) {
                PropertySet = properties ?? PropertySet.FirstClassProperties,
            };
            iview.OrderBy.Add(ItemSchema.DateTimeReceived, SortDirection.Descending);
            FindItemsResults<Item> results = Service.FindItems(folderId, new SearchFilter.IsEqualTo(ItemSchema.ItemClass, ItemClass.Email), iview);
            if (results.TotalCount > 0) {
                do {
                    foreach (Item email in results) {
                        //if (email is EmailMessage emailMsg)
                        yield return (EmailMessage) email;
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
