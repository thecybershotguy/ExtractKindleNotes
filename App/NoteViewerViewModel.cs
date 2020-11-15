using GmailServiceProject;
using Google.Apis.Gmail.v1;
using ProjectTools;
using System;
using System.Threading.Tasks;

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
        private System.Timers.Timer _updateTimer;

        public GoogleAPI GoogleAPI { get; private set; }

        /// <summary>
        /// Gets a value indicating whether [Google service initialized].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [Google service initialized]; otherwise, <c>false</c>.
        /// </value>
        public bool GoogleServiceInitialized { get; private set; } = false;
        #endregion

        #region Constructor
        public NoteViewerViewModel() : base(nameof(NoteViewerViewModel))
        {

            LogInformation($"---------------------------Starting application---------------------------");
            _updateTimer = new System.Timers.Timer(100);
            _updateTimer.Elapsed += UpdateTimer_Elapsed;
            _updateTimer.Enabled = true;
        }

        /// <summary>
        /// Checks for any new emails
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Timers.ElapsedEventArgs"/> instance containing the event data.</param>
        private void UpdateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                _updateTimer.Stop();
                
                if (GoogleServiceInitialized)
                {

                }
            }
            catch (Exception exc)
            {
                LogError(exc);
                throw;
            }
            finally 
            {
                _updateTimer.Start();
            }
        }
        #endregion

        /// <summary>
        /// Initializes the Gmail service asynchronous.
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
    }
}
