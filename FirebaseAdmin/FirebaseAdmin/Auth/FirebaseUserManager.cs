using FirebaseAdmin.Auth.Internal;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using static FirebaseAdmin.Auth.UserRecord;

namespace FirebaseAdmin.Auth
{
    internal class FirebaseUserManager : IDisposable
    {
        static readonly String USER_NOT_FOUND_ERROR = "user-not-found";
        static readonly String INTERNAL_ERROR = "internal-error";
        static readonly String ID_TOKEN_REVOKED_ERROR = "id-token-revoked";
        static readonly String SESSION_COOKIE_REVOKED_ERROR = "session-cookie-revoked";

        private static readonly Dictionary<String, String> ERROR_CODES = new Dictionary<String, String>()
        {
            { "CLAIMS_TOO_LARGE", "claims-too-large"},
            { "CONFIGURATION_NOT_FOUND", "project-not-found" },
            { "INSUFFICIENT_PERMISSION", "insufficient-permission"},
            { "DUPLICATE_EMAIL", "email-already-exists" },
            { "DUPLICATE_LOCAL_ID", "uid-already-exists"},
            { "EMAIL_EXISTS", "email-already-exists"},
            { "INVALID_CLAIMS", "invalid-claims"},
            { "INVALID_EMAIL", "invalid-email"},
            { "INVALID_PAGE_SELECTION", "invalid-page-token"},
            { "INVALID_PHONE_NUMBER", "invalid-phone-number"},
            { "PHONE_NUMBER_EXISTS", "phone-number-already-exists"},
            { "PROJECT_NOT_FOUND", "project-not-found"},
            { "USER_NOT_FOUND", USER_NOT_FOUND_ERROR},
            { "WEAK_PASSWORD", "invalid-password"},
            { "UNAUTHORIZED_DOMAIN", "unauthorized-continue-uri"},
            { "INVALID_DYNAMIC_LINK_DOMAIN", "invalid-dynamic-link-domain"}
        };

        static readonly int MAX_LIST_USERS_RESULTS = 1000;
        static readonly int MAX_IMPORT_USERS = 1000;

        public static readonly List<string> RESERVED_CLAIMS = new List<string>() {
            "amr", "at_hash", "aud", "auth_time", "azp", "cnf", "c_hash", "exp", "iat",
            "iss", "jti", "nbf", "nonce", "sub", "firebase"};

        private const string ID_TOOLKIT_URL = "https://identitytoolkit.googleapis.com/v1/projects/{0}";

        private static readonly string CLIENT_VERSION_HEADER = "X-Client-Version";

        private readonly string baseUrl;
        //private readonly JsonFactory jsonFactory;
        //private readonly HttpRequestFactory requestFactory;
        //private readonly String clientVersion = "Java/Admin/" + SdkUtils.getVersion();

        //private HttpResponseInterceptor interceptor;
        private readonly ConfigurableHttpClient _httpClient;
        private readonly string _baseUrl;

        internal FirebaseUserManager(FirebaseUserManagerArgs args)
        {
            _httpClient = args.ClientFactory.CreateAuthorizedHttpClient(args.Credential);
            _baseUrl = string.Format(ID_TOOLKIT_URL, args.ProjectId);
        }
        internal static FirebaseUserManager Create(FirebaseApp app)
        {
            var projectId = app.GetProjectId();
            if (string.IsNullOrEmpty(projectId))
            {
                throw new ArgumentException(
                    "Must initialize FirebaseApp with a project ID to manage users.");
            }

            var args = new FirebaseUserManagerArgs
            {
                ClientFactory = new HttpClientFactory(),
                Credential = app.Options.Credential,
                ProjectId = projectId,
            };

            return new FirebaseUserManager(args);
        }


        private async Task<JObject> PostAsync(string path, UserRecord user)
        {
            var requestUri = $"{_baseUrl}{path}";
            HttpResponseMessage response = null;
            try
            {
                response = await _httpClient.PostJsonAsync(requestUri, user, default);
                var json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JObject.Parse(json);
                }
                else
                {
                    var error = "Response status code does not indicate success: "
                            + $"{(int)response.StatusCode} ({response.StatusCode})"
                            + $"{Environment.NewLine}{json}";
                    throw new FirebaseException(error);
                }
            }
            catch (HttpRequestException e)
            {
                throw new FirebaseException("Error while calling Firebase Auth service", e);
            }
        }
        private async Task<JObject> PostAsync(string path, Dictionary<string, object> payload)
        {
            var requestUri = $"{_baseUrl}{path}";
            HttpResponseMessage response = null;
            try
            {
                response = await _httpClient.PostJsonAsync(requestUri, payload, default);
                var json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JObject.Parse(json);
                }
                else
                {
                    var error = "Response status code does not indicate success: "
                            + $"{(int)response.StatusCode} ({response.StatusCode})"
                            + $"{Environment.NewLine}{json}";
                    throw new FirebaseException(error);
                }
            }
            catch (HttpRequestException e)
            {
                throw new FirebaseException("Error while calling Firebase Auth service", e);
            }
        }
        public async Task<UserRecord> GetUserByIdAsync(string uid)
        {
            Dictionary<string, object> payload = new Dictionary<string, object>() { { "localId", uid } };
            var json = await PostAsync("/accounts:lookup", payload);
            var response = json.ToObject<GetAccountInfoResponse>();
            if (response == null || response.getUsers() == null || response.getUsers().Count == 0)
            {
                throw new FirebaseAuthException(USER_NOT_FOUND_ERROR,
                    "No user record found for the provided user ID: " + uid);
            }
            return new UserRecord(response.getUsers()[0]);
        }
        public async Task<UserRecord> GetUserByEmailAsync(string email)
        {
            Dictionary<string, object> payload = new Dictionary<string, object>() { { "email", email } };
            var json = await PostAsync("/accounts:lookup", payload);
            var response = json.ToObject<GetAccountInfoResponse>();
            if (response == null || response.getUsers() == null || response.getUsers().Count == 0)
                throw new FirebaseAuthException(USER_NOT_FOUND_ERROR,
                    "No user record found for the provided email: " + email);
            return new UserRecord(response.getUsers()[0]);
        }
        public async Task<UserRecord> GetUserByPhoneNumber(String phoneNumber)
        {
            Dictionary<string, object> payload = new Dictionary<string, object>() { { "phoneNumber", phoneNumber } };
            var json = await PostAsync("/accounts:lookup", payload);
            var response = json.ToObject<GetAccountInfoResponse>();
            if (response == null || response.getUsers() == null || response.getUsers().Count == 0)
                throw new FirebaseAuthException(USER_NOT_FOUND_ERROR,
                    "No user record found for the provided phone number: " + phoneNumber);
            return new UserRecord(response.getUsers()[0]);
        }

        public async Task<string> CreateUserAsync(CreateRequest request)
        {
            var response = await PostAsync("/accounts", request.getProperties());
            if (response != null)
            {
                string uid = (String)response.GetValue("localId");
                if (!string.IsNullOrEmpty(uid))
                {
                    return uid;
                }
            }
            throw new FirebaseAuthException(INTERNAL_ERROR, "Failed to create new user");
        }
        public async Task UpdateUserAsync(UserRecord user)
        {
            var resopnse = await PostAsync("/accounts:update", user);

            try
            {
                var userResponse = resopnse.ToObject<UserRecord>();
                if (userResponse.getUid() != user.getUid())
                {
                    throw new FirebaseException($"Failed to update user: {user.getUid()}");
                }
            }
            catch (Exception e)
            {
                throw new FirebaseException("Error while calling Firebase Auth service", e);
            }
        }
        public async Task DeleteUserAsync(string uid)
        {
            Dictionary<string, object> payload = new Dictionary<string, object>() { { "localId", uid } };
            var response = await PostAsync("/accounts:delete", payload);
            if (response == null || response.GetValue("kind") == null)
            {
                throw new FirebaseAuthException(INTERNAL_ERROR, "Failed to delete user: " + uid);
            }
        }
        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }

    internal sealed class FirebaseUserManagerArgs
    {
        public HttpClientFactory ClientFactory { get; set; }
        public GoogleCredential Credential { get; set; }
        public string ProjectId { get; set; }
    }
}
