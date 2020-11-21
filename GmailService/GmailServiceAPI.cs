using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using ProjectTools;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GmailServiceProject
{
    public class GoogleAPI : BaseClass
    {
        #region Fields and Properties
        const string CredentialFileName = "credentials.json";
        const string CredentialsFolderName = "Credentials";
        const string TokensFolderName = "Credentials";
        private FileStream _credentialFileStream;
        private FileDataStore _GmailToken;
        private UserCredential _userCredential;


        /// <summary>
        /// Gets the credential file name with path.
        /// </summary>
        /// <value>
        /// The credential file name with path.
        /// </value>
        public string CredentialFileNameWithPath => Path.Combine(Directory.GetCurrentDirectory(), CredentialsFolderName, CredentialFileName);

        /// <summary>
        /// Gets the token file name path.
        /// </summary>
        /// <value>
        /// The token file name path.
        /// </value>
        public string TokenFileNamePath => Path.Combine(Directory.GetCurrentDirectory(), "Token");

        /// <summary>
        /// Gets the G-mail service user resource used for mail manipulation.
        /// </summary>
        /// <returns></returns>
        public UsersResource GetGmailUserResource()
        {
            try
            {
                UsersResource user = new GmailService(new BaseClientService.Initializer() { HttpClientInitializer = _userCredential}).Users;
                LogInformation($"Successfully created Gmail Service");
                return user;
            }
            catch (Exception ex)
            {
                LogError(ex, "Failed to create Gmail service");
                throw;
            }
        }
        #endregion

        #region Constructor
        public GoogleAPI() : base(nameof(GoogleAPI))
        {
        }
        #endregion

        /// <summary>
        /// Creates the user credential asynchronous.
        /// </summary>
        public async Task CreateUserCredentialAsync()
        {
            try
            {
                InitializeCredentials();

                 var loadedSecerts = GoogleClientSecrets.Load(_credentialFileStream).Secrets;

                var scopes = new string[] { GmailService.Scope.GmailReadonly , GmailService.Scope.GmailModify};

                _userCredential = await GoogleWebAuthorizationBroker.AuthorizeAsync(loadedSecerts, scopes, "user", CancellationToken.None, _GmailToken);
            }
            catch (Exception exc)
            {
                LogError(exc);
                throw;
            }
        }

        /// <summary>
        /// Initializes the credentials.
        /// </summary>
        internal void InitializeCredentials()
        {
            try
            {
                if (File.Exists(CredentialFileNameWithPath) is false)
                    throw new Exception($"{CredentialFileName} does not exist at {CredentialFileNameWithPath}");

                _credentialFileStream = new FileStream(CredentialFileNameWithPath, FileMode.Open, FileAccess.Read);

                LogInformation($"Successfully loaded {CredentialFileName} from {CredentialFileNameWithPath}");

                _GmailToken = new FileDataStore(TokenFileNamePath, true);

                LogInformation($"Successfully created Gmail Token at {TokenFileNamePath}");
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }
    }
}
