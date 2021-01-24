using GmailServiceProject;
using Google.Apis.Gmail.v1;
using ProjectTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractKindleNotes
{
    public class GoogleServiceHelper : BaseClass
    {
        /// <summary>
        /// Gets the google API.
        /// </summary>
        /// <value>
        /// The google API.
        /// </value>
        public GoogleAPI GoogleAPI { get; private set; }
        
        /// <summary>
        /// Gets the users resource.
        /// </summary>
        /// <value>
        /// The users resource.
        /// </value>
        public UsersResource UsersResource { get; private set; }
        
        /// <summary>
        /// Gets a value indicating whether [google service initialized].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [google service initialized]; otherwise, <c>false</c>.
        /// </value>
        public bool GoogleServiceInitialized { get; private set; }

        /// <summary>
        /// Initializes the G-mail service asynchronous.
        /// </summary>
        public async Task InitializeGmailServiceAsync()
        {
            try
            {
                GoogleAPI = new GoogleAPI();
                await GoogleAPI.CreateUserCredentialAsync();
                UsersResource = GoogleAPI.GetGmailUserResource();
                GoogleServiceInitialized = true;
            }
            catch (Exception ex)
            {
                LogError(ex);
                GoogleServiceInitialized = false;
                throw;
            }
        }

        public GoogleServiceHelper() : base(nameof(GoogleServiceHelper))
        {
            InitializeGmailServiceAsync();
        }
    }
}