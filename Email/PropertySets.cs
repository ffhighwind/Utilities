using Microsoft.Exchange.WebServices.Data;

namespace Utilities.Email
{

	/// <summary>
	/// PropertySet
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

		public static readonly PropertySet EmailAll =
			new PropertySet(BasePropertySet.FirstClassProperties,
				EmailMessageSchema.ToRecipients,
				EmailMessageSchema.ReceivedRepresenting,
				EmailMessageSchema.ReceivedBy,
				EmailMessageSchema.Sender,
				EmailMessageSchema.ReplyTo,
				EmailMessageSchema.References,
				EmailMessageSchema.InternetMessageId,
				EmailMessageSchema.IsResponseRequested,
				EmailMessageSchema.IsReadReceiptRequested,
				EmailMessageSchema.IsRead,
				EmailMessageSchema.IsDeliveryReceiptRequested,
				EmailMessageSchema.From,
				EmailMessageSchema.ConversationTopic,
				EmailMessageSchema.ConversationIndex,
				EmailMessageSchema.CcRecipients,
				EmailMessageSchema.BccRecipients,
				EmailMessageSchema.ApprovalRequestData,
				EmailMessageSchema.VotingInformation);

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

		public static readonly PropertySet TaskAll =
			new PropertySet(
				TaskSchema.ActualWork,
				TaskSchema.AssignedTime,
				TaskSchema.BillingInformation,
				TaskSchema.ChangeCount,
				TaskSchema.Companies,
				TaskSchema.CompleteDate,
				TaskSchema.Contacts,
				TaskSchema.DelegationState,
				TaskSchema.Delegator,
				TaskSchema.DueDate,
				TaskSchema.IsComplete,
				TaskSchema.IsRecurring,
				TaskSchema.IsTeamTask,
				TaskSchema.Mileage,
				TaskSchema.Mode,
				TaskSchema.Owner,
				TaskSchema.PercentComplete,
				TaskSchema.Recurrence,
				TaskSchema.StartDate,
				TaskSchema.Status,
				TaskSchema.StatusDescription,
				TaskSchema.TotalWork);

		public static readonly PropertySet AppointmentAll =
			new PropertySet(BasePropertySet.FirstClassProperties,
				AppointmentSchema.StartTimeZone,
				AppointmentSchema.Duration,
				AppointmentSchema.TimeZone,
				AppointmentSchema.AppointmentReplyTime,
				AppointmentSchema.AppointmentSequenceNumber,
				AppointmentSchema.AppointmentState,
				AppointmentSchema.Recurrence,
				AppointmentSchema.FirstOccurrence,
				AppointmentSchema.LastOccurrence,
				AppointmentSchema.ModifiedOccurrences,
				AppointmentSchema.DeletedOccurrences,
				AppointmentSchema.ConferenceType,
				AppointmentSchema.AllowNewTimeProposal,
				AppointmentSchema.IsOnlineMeeting,
				AppointmentSchema.MeetingWorkspaceUrl,
				AppointmentSchema.NetShowUrl,
				AppointmentSchema.ICalUid,
				AppointmentSchema.ICalRecurrenceId,
				AppointmentSchema.ICalDateTimeStamp,
				AppointmentSchema.EnhancedLocation,
				AppointmentSchema.AdjacentMeetings,
				AppointmentSchema.JoinOnlineMeetingUrl,
				AppointmentSchema.ConflictingMeetings,
				AppointmentSchema.ConflictingMeetingCount,
				AppointmentSchema.EndTimeZone,
				AppointmentSchema.Start,
				AppointmentSchema.End,
				AppointmentSchema.OriginalStart,
				AppointmentSchema.IsAllDayEvent,
				AppointmentSchema.LegacyFreeBusyStatus,
				AppointmentSchema.Location,
				AppointmentSchema.When,
				AppointmentSchema.IsMeeting,
				AppointmentSchema.IsCancelled,
				AppointmentSchema.IsRecurring,
				AppointmentSchema.MeetingRequestWasSent,
				AppointmentSchema.IsResponseRequested,
				AppointmentSchema.AppointmentType,
				AppointmentSchema.MyResponseType,
				AppointmentSchema.Organizer,
				AppointmentSchema.RequiredAttendees,
				AppointmentSchema.OptionalAttendees,
				AppointmentSchema.Resources,
				AppointmentSchema.AdjacentMeetingCount,
				AppointmentSchema.OnlineMeetingSettings);

		static PropertySets()
		{
			AppointmentAll.AddRange(ItemAll);
			EmailAll.AddRange(ItemAll);
			TaskAll.AddRange(ItemAll);
		}
	}
}
