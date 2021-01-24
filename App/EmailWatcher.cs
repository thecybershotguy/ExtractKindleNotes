using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using ProjectTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Threading;

namespace ExtractKindleNotes
{

    public static class AttachmentFormat
    {

        #region Public Fields

        public static string CSV_FORMAT = "text/csv";

        #endregion Public Fields


    }


    /// <summary>
    /// Note Viewer View model
    /// </summary>
    /// <seealso cref="ExtractKindleNotes.BaseClass" />
    public class EmailWatcher : BaseClass
    {

        #region Private Fields

        private const string USER_ID = "me";
        private readonly DispatcherTimer _emailCheckTimer;
        private readonly DispatcherTimer _payloadParser;

        /// <summary>
        /// The Label specific emails
        /// </summary>
        private readonly Queue<Message> LabelSpecificEmails = new Queue<Message>();

        #endregion Private Fields

        #region Public Properties

        public GoogleServiceHelper GoogleServiceHelper { get; }
        public ViewModel ViewModel { get; }

        #endregion Public Properties

        #region Public Constructors

        public EmailWatcher(GoogleServiceHelper googleServiceHelper , ViewModel viewModel) : base(nameof(EmailWatcher))
        {
            GoogleServiceHelper = googleServiceHelper ?? throw new ArgumentNullException(nameof(googleServiceHelper));
            ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _emailCheckTimer = new DispatcherTimer();
            _payloadParser = new DispatcherTimer();
            _emailCheckTimer.Interval = TimeSpan.FromSeconds(1);
            _payloadParser.Interval = TimeSpan.FromSeconds(1);
            InitializeTimers();
        }

        #endregion Public Constructors

        #region Public Methods

        /// <summary>
        /// Gets the label identifier asynchronous.
        /// </summary>
        /// <param name="labelName">Name of the label.</param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException">Label {labelName} not found in G-mail, Labels found: {response.Labels.Select(s => s.Name).Fuse()}</exception>
        public async Task<string> GetLabelIdAsync(string labelName = "Kindle")
        {
            try
            {
                var response = await GoogleServiceHelper.UsersResource.Labels.List(USER_ID).ExecuteAsync();
                var label = response.Labels.Where(s => string.Equals(s.Name, labelName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                if (label is null)
                    throw new InvalidDataException($"Label {labelName} not found in G-mail, Labels found: {response.Labels.Select(s => s.Name).Fuse()}");

                LogInformation($"Label id found {label.Id} for label name: {labelName}");
                return label.Id;
            }
            catch (Exception e)
            {
                LogError(e);
                throw;
            }
        }

        /// <summary>
        /// Initializes the timers.
        /// </summary>
        public void InitializeTimers()
        {
            try
            {
                _emailCheckTimer.Tick += CheckForNewEmailTimer_Elapsed;
                _emailCheckTimer.Start();

                _payloadParser.Tick += ParsePayLoadFromEmailsQueue_ElapsedAsync;
                 _payloadParser.Start();
            }
            catch (Exception e)
            {
                LogError(e);
                throw;
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Checks for new emails.
        /// </summary>
        /// <param name="labelName">Name of the label.</param>
        private async Task CheckForNewEmails(string labelName)
        {
            try
            {
                if (string.IsNullOrEmpty(labelName))
                    throw new ArgumentException("Name of the Label provided is null or empty", nameof(labelName));

                // Maybe this can be done once but its good to have this checked up as it gives you feedback if the label was deleted or trashed
                string labelId = await GetLabelIdAsync(labelName);

                await ReadMessagesFromLabelIdAsync(labelId);
            }
            catch (Exception e)
            {
                LogError(e);
                throw;
            }
        }

        /// <summary>
        /// Checks for new Email timer.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        private async void CheckForNewEmailTimer_Elapsed(object sender, EventArgs e )
        {
            try
            {
                _emailCheckTimer.Stop();

                 await CheckForNewEmails("Kindle");
            }
            catch (Exception exc)
            {
                LogError(exc);

                // If something goes wrong try Initializing G-mail service again in the next timer tick
                // If This happens show UI Indication
                await GoogleServiceHelper.InitializeGmailServiceAsync();
            }
            finally
            {
                _emailCheckTimer.Start();
            }
        }

        /// <summary>
        /// Decodes the attachment data from Base64 to string.
        /// </summary>
        /// <param name="attachment">The attachment.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">attachment</exception>
        /// <exception cref="InvalidDataException">There was no data for the Attachment Id: {attachment.AttachmentId}</exception>
        private string DecodeAttachmentData(MessagePartBody attachment)
        {
            if (attachment is null)
                throw new ArgumentNullException(nameof(attachment));

            var dataExisits = attachment.Data != null;

            if (dataExisits is true)
                return Tools.Base64ToString(attachment.Data);

            throw new InvalidDataException($"There was no data for the Attachment Id: {attachment.AttachmentId}");
        }

        /// <summary>
        /// Gets the attachment of a particular type from message body.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="attachmentType">Type of the attachment.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">No attachment founds for {attachmentType} type which sucks</exception>
        private async Task<MessagePartBody> GetAttachment(Message message, string attachmentType)
        {
            try
            {
                var messageContent = message.Payload.Parts.Where(s => string.Equals(s.MimeType, attachmentType, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                if (messageContent is null)
                    throw new InvalidOperationException($"No attachment founds for {attachmentType} type which sucks");

                var attachment = await GoogleServiceHelper.UsersResource.Messages.Attachments.Get(USER_ID, message.Id, messageContent.Body.AttachmentId).ExecuteAsync();
                return attachment;
            }
            catch (Exception exc)
            {
                LogError(exc);
                throw;
            }
        }
        /// <summary>
        /// Decodes useful information from the Emails in the queue
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        /// <exception cref="NotImplementedException"></exception>
        private async void ParsePayLoadFromEmailsQueue_ElapsedAsync(object sender, EventArgs e)
        {
            try
            {
                _payloadParser.Stop();

                // Read all the Queued emails
                while (LabelSpecificEmails.Count != 0)
                {
                    var message = LabelSpecificEmails.Dequeue();
                    MessagePartBody attachment = await GetAttachment(message, AttachmentFormat.CSV_FORMAT);
                    string readableData = DecodeAttachmentData(attachment);

                    var book = new Book(readableData);

                    Book bookRead = ViewModel.BooksRead.Where(s => s == book).FirstOrDefault();

                    if (bookRead == null)
                    {
                        book.UpdateNotes(readableData, false);
                        ViewModel.BooksRead.Add(book);
                        ViewModel.CreateAndUpdateJsonDataBase();
                    }
                    else 
                    {
                        bookRead.UpdateNotes(readableData, true);
                    }
                }
            }
            catch (Exception exc)
            {
                LogError(exc);
            }
            finally
            {
                _payloadParser.Start();
            }
        }
        /// <summary>
        /// Populates the email queue and delete email asynchronous.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <exception cref="ArgumentNullException">Response from email {nameof(response)} is null</exception>
        private async Task PopulateEmailQueueAndDeleteEmailAsync(ListMessagesResponse response, bool deleteEmail = true)
        {
            try
            {
                if (response is null)
                    throw new ArgumentNullException($"Response from email {nameof(response)} is null ");

                int counter = 0;
                foreach (Message mail in response.Messages)
                {
                    counter++;

                    var message = await GoogleServiceHelper.UsersResource.Messages.Get(USER_ID, mail.Id).ExecuteAsync();
                    LabelSpecificEmails.Enqueue(message);
                    LogInformation($"Adding emails to queue [ {counter} / {response.Messages.Count} ]");

                    if (deleteEmail)
                    {
                        await GoogleServiceHelper.UsersResource.Messages.Trash(USER_ID, mail.Id).ExecuteAsync();
                        LogInformation($"Deleting email [ {counter} / {response.Messages.Count} ]");
                    }
                }
            }
            catch (Exception e)
            {
                LogError(e);
                throw;
            }
        }

        /// <summary>
        /// Reads the messages from label identifier asynchronous.
        /// </summary>
        /// <param name="labelId">The label identifier.</param>
        /// <exception cref="ArgumentException">Label Id is null or empty - labelId</exception>
        private async Task ReadMessagesFromLabelIdAsync(string labelId)
        {
            try
            {
                if (string.IsNullOrEmpty(labelId))
                    throw new ArgumentException($"Label Id is null or empty", nameof(labelId));

                UsersResource.MessagesResource.ListRequest messagesToRead = GoogleServiceHelper.UsersResource.Messages.List(USER_ID);
                messagesToRead.LabelIds = labelId;
                messagesToRead.IncludeSpamTrash = false;

                var response = await messagesToRead.ExecuteAsync();

                var newEmailExisits = response.Messages != null;

                if (newEmailExisits)
                    await PopulateEmailQueueAndDeleteEmailAsync(response, false);
                else
                    LogDebug($"No new emails from kindle received in the label");
            }
            catch (Exception e)
            {
                LogError(e);
                throw;
            }
        }

        #endregion Private Methods
    }
}
