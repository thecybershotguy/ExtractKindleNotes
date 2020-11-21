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
    public class NoteViewerViewModel : BaseClass
    {
        #region Fields And Properties

        public UsersResource _usersResource;
        private const string USER_ID = "me";
        private readonly System.Timers.Timer _emailCheckTimer;


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

        /// <summary>
        /// Gets the label ID asynchronous.
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
        #endregion

        #region Constructor
        public NoteViewerViewModel() : base(nameof(NoteViewerViewModel))
        {
            LogInformation($"---------------------------Starting application---------------------------");
            _emailCheckTimer = new Timer(1000);
            _emailCheckTimer.Elapsed += CheckForNewEmailTimer_Elapsed;
            _emailCheckTimer.Enabled = true;
            _emailCheckTimer.Start();
        }
        #endregion

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
        /// Populates the email queue and delete email asynchronous.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <exception cref="ArgumentNullException">Response from email {nameof(response)} is null</exception>
        private async Task PopulateEmailQueueAndDeleteEmailAsync(ListMessagesResponse response)
        {
            try
            {
                if (response is null)
                    throw new ArgumentNullException($"Response from email {nameof(response)} is null ");

                int counter = 0;
                foreach (Message mail in response.Messages)
                {
                    counter++;
                    LabelSpecificEmails.Enqueue(mail);
                    LogInformation($"Adding emails to queue [ {counter} / {response.Messages.Count} ]");
                    await _usersResource.Messages.Trash(USER_ID, mail.Id).ExecuteAsync();
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

                if (response.Messages.Any())
                    await PopulateEmailQueueAndDeleteEmailAsync(response);
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
