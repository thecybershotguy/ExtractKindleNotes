using GmailServiceProject;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using ProjectTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace ExtractKindleNotes
{
    /// <summary>
    /// Note Viewer View model
    /// </summary>
    /// <seealso cref="ExtractKindleNotes.BaseClass" />
    public class NotesModel : BaseClass
    {
        #region Fields And Properties

        public UsersResource _usersResource;
        private const string CSV_FORMAT = "text/csv";
        private const string USER_ID = "me";
        private readonly System.Timers.Timer _emailCheckTimer;
        private readonly Timer _payloadParser;


        /// <summary>
        /// The Label specific emails
        /// </summary>
        private readonly Queue<Message> LabelSpecificEmails = new Queue<Message>();

        private bool _googleServiceInitialized = false;
        public GoogleAPI GoogleAPI { get; private set; }



        /// <summary>
        /// Gets a value indicating whether [Google service initialized].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [Google service initialized]; otherwise, <c>false</c>.
        /// </value>
        public bool GoogleServiceInitialized
        {
            get { return _googleServiceInitialized; }
            private set
            {
                _googleServiceInitialized = value;
                NotifyPropertyChanged();
            }
        }
        #endregion

        #region Constructor
        public NotesModel() : base(nameof(NotesModel))
        {
            _emailCheckTimer = new Timer(1000);
            _payloadParser = new Timer(1000);
            InitializeTimers();
        }
        #endregion

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
                var response = await _usersResource.Labels.List(USER_ID).ExecuteAsync();
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
        /// Initializes the G-mail service asynchronous.
        /// </summary>
        public async Task InitializeGmailServiceAsync()
        {
            try
            {
                GoogleAPI = new GoogleAPI();
                await GoogleAPI.CreateUserCredentialAsync();
                _usersResource = GoogleAPI.GetGmailUserResource();
                GoogleServiceInitialized = true;
            }
            catch (Exception ex)
            {
                LogError(ex);
                GoogleServiceInitialized = false;
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
                _emailCheckTimer.Elapsed += CheckForNewEmailTimer_Elapsed;
                _emailCheckTimer.Enabled = true;
                _emailCheckTimer.Start();

                _payloadParser.Elapsed += ParsePayLoadFromEmailsQueue_ElapsedAsync;
                _payloadParser.Enabled = true;
                _payloadParser.Start();
            }
            catch (Exception e)
            {
                LogError(e);
                throw;
            }
        }

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
        private async void CheckForNewEmailTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                _emailCheckTimer.Stop();
                _emailCheckTimer.Enabled = false;


                if (GoogleServiceInitialized is false)
                    await InitializeGmailServiceAsync();
                else
                    await CheckForNewEmails("Kindle");
            }
            catch (Exception exc)
            {
                LogError(exc);

                // If something goes wrong try Initializing G-mail service again in the next timer tick
                // If This happens show UI Indication
                GoogleServiceInitialized = false;
            }
            finally
            {
                _emailCheckTimer.Enabled = true;
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

            var dataExisits = attachment.Data == null ? false : true;

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
            var messageContent = message.Payload.Parts.Where(s => string.Equals(s.MimeType, attachmentType, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            if (messageContent is null)
                throw new InvalidOperationException($"No attachment founds for {attachmentType} type which sucks");

            var attachment = await _usersResource.Messages.Attachments.Get(USER_ID, message.Id, messageContent.Body.AttachmentId).ExecuteAsync();
            return attachment;
        }

        /// <summary>
        /// Decodes useful information from the Emails in the queue
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        /// <exception cref="NotImplementedException"></exception>
        private async void ParsePayLoadFromEmailsQueue_ElapsedAsync(object sender, ElapsedEventArgs e)
        {
            try
            {
                _payloadParser.Stop();
                _payloadParser.Enabled = false;

                // Read all the Queued emails
                while (LabelSpecificEmails.Count != 0)
                {
                    var message = LabelSpecificEmails.Dequeue();
                    MessagePartBody attachment = await GetAttachment(message, CSV_FORMAT);
                    string readableData = DecodeAttachmentData(attachment);

                    var book = new Book(readableData);
                    // TODO : Invoke event to send book data
                    

                    // TODO : Make a new exception CSV FIle format change , when that happens handle it safely and save the CSV File to update the code
                    //Tools.SaveToFile(readableData, message.Id, true, Tools.TypeOfFile.Csv);
                }
            }
            catch (Exception exc)
            {
                LogError(exc);
            }
            finally
            {
                _payloadParser.Enabled = true;
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

                    var message = await _usersResource.Messages.Get(USER_ID, mail.Id).ExecuteAsync();
                    LabelSpecificEmails.Enqueue(message);
                    LogInformation($"Adding emails to queue [ {counter} / {response.Messages.Count} ]");

                    if (deleteEmail)
                    {
                        await _usersResource.Messages.Trash(USER_ID, mail.Id).ExecuteAsync();
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

                UsersResource.MessagesResource.ListRequest messagesToRead = _usersResource.Messages.List(USER_ID);
                messagesToRead.LabelIds = labelId;
                messagesToRead.IncludeSpamTrash = false;

                var response = await messagesToRead.ExecuteAsync();

                var newEmailExisits = response.Messages == null ? false : true;

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
    }
}
