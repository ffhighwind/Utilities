namespace Utilities.Email
{
	/// <summary>
	/// https://docs.microsoft.com/en-us/office/vba/outlook/Concepts/Forms/item-types-and-message-classes
	/// https://docs.microsoft.com/en-us/previous-versions/office/developer/exchange-server-2007/aa580993(v=exchg.80)
	/// https://docs.microsoft.com/en-us/exchange/client-developer/web-service-reference/itemclass
	///
	/// These can also be obtained via new EmailMessage().ItemClass.
	/// </summary>
	public static class ItemClass
	{
		/// <summary>
		/// Journal entries
		/// </summary>
		public const string Activity = "IPM.Activity";

		public const string Appointment = "IPM.Appointment";

		public const string Contact = "IPM.Contact";

		public const string DistributionList = "IPM.DistList";

		/// <summary>
		/// txt, docx, doc, xls, xlsx, xlsm, ppt, pps, pptx, pdf, zip
		/// </summary>
		/// <see href="https://techniclee.wordpress.com/2010/06/29/importing-documents-into-outlook/"/>
		public const string Document = "IPM.Document";

		/// <summary>
		/// Exception item of a recurrence series.
		/// </summary>
		public const string OLEClass = "IPM.OLE.Class";

		/// <summary>
		/// Unknown item type
		/// </summary>
		public const string Item = "IPM.Items";

		public const string Email = "IPM.Note";

		/// <summary>
		/// Reports from the Internet Mail Connect (the Exchange Server gateway to the Internet).
		/// </summary>
		public const string Notification = "IPM.Note.IMC.Notification";

		/// <summary>
		/// Out-of-office templates.
		/// </summary>
		public const string OOOTemplate = "IPM.Note.Rules.OofTemplate.Microsoft";

		/// <summary>
		/// Posting notes in a folder.
		/// </summary>
		public const string Post = "IPM.Post";

		/// <summary>
		/// Creating notes
		/// </summary>
		public const string StickyNote = "IPM.StickyNote";

		/// <summary>
		/// Message recall reports.
		/// </summary>
		public const string RecallReport = "IPM.Recall.Report";

		/// <summary>
		/// Recalling sent messages from recipient inboxes.
		/// </summary>
		public const string Recall = "IPM.Outlook.Recall";

		/// <summary>
		/// Remote Mail message headers.
		/// </summary>
		public const string Remote = "IPM.Remote";

		/// <summary>
		/// Editing rule reply templates.
		/// </summary>
		public const string ReplyTemplate = "IPM.Note.Rules.ReplyTemplate.Microsoft";

		/// <summary>
		/// Reporting item status.
		/// </summary>
		public const string Report = "IPM.Report";

		/// <summary>
		/// Resending a failed message.
		/// </summary>
		public const string Resend = "IPM.Resend";

		public const string CanceledMeeting = "IPM.Schedule.Meeting.Canceled";

		public const string RequestMeeting = "IPM.Schedule.Meeting.Request";

		public const string DeclineMeeting = "IPM.Schedule.Meeting.Resp.Neg";

		public const string AcceptMeeting = "IPM.Schedule.Meeting.Resp.Pos";

		public const string TentativeMeeting = "IPM.Schedule.Meeting.Resp.Tent";

		/// <summary>
		/// Encrypted emails to other people.
		/// </summary>
		public const string EmailSecure = "IPM.Note.Secure";

		/// <summary>
		/// Digitally signed emails to other people.
		/// </summary>
		public const string EmailSecureSign = "IPM.Note.Secure.Sign";

		public const string Task = "IPM.Task";

		public const string AcceptTask = "IPM.TaskRequest.Accept";

		public const string DeclineTask = "IPM.TaskRequest.Decline";

		public const string RequestTask = "IPM.TaskRequest";

		public const string UpdateTask = "IPM.TaskRequest.Update";
	}
}
