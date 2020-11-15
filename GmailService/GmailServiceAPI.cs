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
        /// Gets the Gmail service user resource used for mail manipulation.
        /// </summary>
        /// <returns></returns>
        public UsersResource GetGmailUserResource()
        {
            try
            {
                UsersResource user = new GmailService(new BaseClientService.Initializer()).Users;
                LogInformation($"Successfully created Gmail Service");
                return user;
            }
            catch (Exception ex)
            {
                LogError(ex, "Failed to create Gmail service");
                throw;
            }
        }

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
        #endregion

        #region Constructor
        public GoogleAPI() : base(nameof(GoogleAPI))
        {
            InitializeCredentials();

        }
        #endregion

        /// <summary>
        /// Creates the user credential for Gmail service authentication.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        /// For class {nameof(GmailServiceAPI)} credentials were not initialized. Implement {nameof(InitializeCredentials)}, undefined object: {nameof(_credentialFileStream)}
        /// or
        /// For class {nameof(GmailServiceAPI)} credentials were not initialized. Implement {nameof(InitializeCredentials)},undefined object: {nameof(_GmailToken)}
        /// </exception>
        public async Task CreateUserCredentialAsync()
        {
            try
            {
                if (_credentialFileStream is null)
                    throw new InvalidOperationException($"For class {nameof(GoogleAPI)} credentials were not initialized. Implement {nameof(InitializeCredentials)}, undefined object: {nameof(_credentialFileStream)}");

                if (_GmailToken is null)
                    throw new InvalidOperationException($"For class {nameof(GoogleAPI)} credentials were not initialized. Implement {nameof(InitializeCredentials)},undefined object: {nameof(_GmailToken)}");


                var loadedSecerts = GoogleClientSecrets.Load(_credentialFileStream).Secrets;

                var scopes = new string[] { GmailService.Scope.GmailReadonly };

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
        public void InitializeCredentials()
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
