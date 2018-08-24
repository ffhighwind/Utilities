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
        public EmailService() { }

        public EmailService(string email, string username, string password)
        {
            Email = email;
            Username = username;
            Password = password;
        }

        public string Email { get; protected set; }

        public string Username { get; protected set; }

        public bool IsConnected { get; private set; }

        public string Password { protected get; set; }

        public ExchangeService Service { get; private set; }

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
            FindItemsResults<Item> findItemsResults = Service.FindItems(parentFolder, filter, fview);
            do {
                foreach (Item item in findItemsResults) {
                    yield return item;
                }
                if (findItemsResults.NextPageOffset.HasValue)
                    fview.Offset = findItemsResults.NextPageOffset.Value;
            } while (findItemsResults.MoreAvailable);
        }

        public Folder CreateFolder(string folderName, FolderId parentFolder)
        {
            Folder folder = FindFolder(folderName, parentFolder);
            if (folder == null) {
                folder = new Folder(Service) {
                    DisplayName = folderName
                };
                folder.Save(parentFolder);
            }
            return folder;
        }

        public Folder CreateFolder(string folderName, WellKnownFolderName parentFolder = WellKnownFolderName.MsgFolderRoot)
        {
            return CreateFolder(folderName, new FolderId(parentFolder));
        }

        public Folder FindFolder(string folderName, WellKnownFolderName parentFolder = WellKnownFolderName.Root,
            FolderTraversal traversal = FolderTraversal.Shallow, string mailbox = null, PropertySet properties = null)
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
            FindFoldersResults findFolderResults = Service.FindFolders(parentFolder, filter, fview);
            do {
                foreach (Folder myFolder in findFolderResults.Folders) {
                    yield return myFolder;
                }
                if (findFolderResults.NextPageOffset.HasValue)
                    fview.Offset = findFolderResults.NextPageOffset.Value;
            } while (findFolderResults.MoreAvailable);
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
            FindFoldersResults findFolderResults = Service.FindFolders(parentFolder, fview);
            do {
                foreach (Folder folder in findFolderResults.Folders) {
                    yield return folder;
                }
                if (findFolderResults.NextPageOffset.HasValue)
                    fview.Offset = findFolderResults.NextPageOffset.Value;
            } while (findFolderResults.MoreAvailable);
        }

        public IEnumerable<FileAttachment> GetFileAttachments(Folder folder, PropertySet properties = null)
        {
            ItemView iview = new ItemView(PageSize, 0) {
                PropertySet = properties ?? PropertySet.FirstClassProperties
            };
            iview.OrderBy.Add(ItemSchema.DateTimeReceived, SortDirection.Descending);
            FindItemsResults<Item> findItemsResults = Service.FindItems(folder.Id, iview);
            do {
                foreach (Item item in findItemsResults) {
                    foreach (FileAttachment fileAttachment in GetNestedFileAttachments(item)) {
                        yield return fileAttachment;
                    }
                }
                if (findItemsResults.NextPageOffset.HasValue)
                    iview.Offset = findItemsResults.NextPageOffset.Value;
            } while (findItemsResults.MoreAvailable);
        }

        public IEnumerable<EmailMessage> GetEmails(Folder folder, PropertySet properties = null)
        {
            if (properties == null)
                properties = PropertySet.FirstClassProperties;
            ItemView iview = new ItemView(PageSize, 0);
            iview.OrderBy.Add(ItemSchema.DateTimeReceived, SortDirection.Descending);
            FindItemsResults<Item> findItemsResults = Service.FindItems(folder.Id, iview);
            if (findItemsResults.TotalCount > 0) {
                do {
                    Service.LoadPropertiesForItems(findItemsResults, properties);
                    foreach (Item email in findItemsResults) {
                        if (email is EmailMessage emailMsg)
                            yield return emailMsg;
                    }
                    if (findItemsResults.NextPageOffset.HasValue)
                        iview.Offset = findItemsResults.NextPageOffset.Value;
                } while (findItemsResults.MoreAvailable);
            }
        }

        public void DeleteEmail(EmailMessage email, DeleteMode mode = DeleteMode.MoveToDeletedItems)
        {
            email.Delete(mode);
        }


        private IEnumerable<FileAttachment> GetNestedFileAttachments(Item item)
        {
            if (item.HasAttachments) {
                foreach (Attachment att in item.Attachments) {
                    if (att is ItemAttachment iatt) {
                        if (iatt.Item == null)
                            att.Load();
                        foreach (FileAttachment fileAttachment in GetNestedFileAttachments(iatt.Item)) {
                            yield return fileAttachment;
                        }
                    }
                    else if (att is FileAttachment fileAttachment)
                        yield return fileAttachment;
                    else
                        Console.WriteLine("Unknown attachment type: " + att.GetType().Name);
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

        /// <summary>
        /// https://docs.microsoft.com/en-us/exchange/client-developer/exchange-web-services/property-sets-and-response-shapes-in-ews-in-exchange
        /// </summary>
        public static class PropertySets
        {
            public static readonly PropertySet IdOnly =
                new PropertySet(BasePropertySet.IdOnly);

            public static readonly PropertySet Default =
                new PropertySet(BasePropertySet.FirstClassProperties);

            public static readonly PropertySet FolderAll =
                new PropertySet(
                    BasePropertySet.FirstClassProperties,
                    FolderSchema.ArchiveTag,
                    FolderSchema.ChildFolderCount,
                    FolderSchema.DisplayName,
                    FolderSchema.EffectiveRights,
                    FolderSchema.FolderClass,
                    FolderSchema.ManagedFolderInformation,
                    FolderSchema.ParentFolderId,
                    FolderSchema.Permissions,
                    FolderSchema.PolicyTag,
                    FolderSchema.TotalCount,
                    FolderSchema.UnreadCount,
                    FolderSchema.WellKnownFolderName);

            public static readonly PropertySet ItemAll =
                new PropertySet(
                    BasePropertySet.FirstClassProperties,
                    ItemSchema.AllowedResponseActions,
                    ItemSchema.ArchiveTag,
                    ItemSchema.Attachments,
                    ItemSchema.Body,
                    ItemSchema.Categories,
                    ItemSchema.ConversationId,
                    ItemSchema.Culture,
                    ItemSchema.DateTimeCreated,
                    ItemSchema.DateTimeReceived,
                    ItemSchema.DateTimeSent,
                    ItemSchema.DisplayCc,
                    ItemSchema.DisplayTo,
                    ItemSchema.EffectiveRights,
                    ItemSchema.EntityExtractionResult,
                    ItemSchema.Flag,
                    ItemSchema.HasAttachments,
                    ItemSchema.IconIndex,
                    ItemSchema.Importance,
                    ItemSchema.InReplyTo,
                    ItemSchema.InstanceKey,
                    ItemSchema.InternetMessageHeaders,
                    ItemSchema.IsAssociated,
                    ItemSchema.IsDraft,
                    ItemSchema.IsFromMe,
                    ItemSchema.IsReminderSet,
                    ItemSchema.IsResend,
                    ItemSchema.IsSubmitted,
                    ItemSchema.IsUnmodified,
                    ItemSchema.ItemClass,
                    ItemSchema.LastModifiedName,
                    ItemSchema.LastModifiedTime,
                    ItemSchema.MimeContent,
                    ItemSchema.NormalizedBody,
                    ItemSchema.ParentFolderId,
                    ItemSchema.PolicyTag,
                    ItemSchema.Preview,
                    ItemSchema.ReminderDueBy,
                    ItemSchema.ReminderMinutesBeforeStart,
                    ItemSchema.RetentionDate,
                    ItemSchema.Sensitivity,
                    ItemSchema.Size,
                    ItemSchema.StoreEntryId,
                    ItemSchema.Subject,
                    ItemSchema.TextBody,
                    ItemSchema.UniqueBody,
                    ItemSchema.WebClientEditFormQueryString,
                    ItemSchema.WebClientReadFormQueryString);
        }

        public const int PageSize = 50;
    }
}
