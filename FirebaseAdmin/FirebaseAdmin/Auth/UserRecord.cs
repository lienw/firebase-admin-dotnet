using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using static FirebaseAdmin.Auth.Internal.GetAccountInfoResponse;
using static FirebaseAdmin.Preconditions;
namespace FirebaseAdmin.Auth
{
    public class UserRecord : IUserInfo
    {
        private static readonly String PROVIDER_ID = "firebase";
        private static readonly Dictionary<String, String> REMOVABLE_FIELDS = new Dictionary<string, string>()
        {
            { "displayName", "DISPLAY_NAME" }, { "photoUrl", "PHOTO_URL" }
        };
        static readonly String CUSTOM_ATTRIBUTES = "customAttributes";
        private static readonly int MAX_CLAIMS_PAYLOAD_SIZE = 1000;

        private String uid;
        private String email;
        private String phoneNumber;
        private bool emailVerified;
        private String displayName;
        private String photoUrl;
        private bool disabled;
        private ProviderUserInfo[] providers;
        private long tokensValidAfterTimestamp;
        private UserMetadata userMetadata;
        private readonly Dictionary<string, object> customClaims;
        public UserRecord() { }
        public UserRecord(User response)
        {
            checkNotNull(response, "response must not be null");
            checkArgument(!String.IsNullOrEmpty(response.getUid()), "uid must not be null or empty");
            this.uid = response.getUid();
            this.email = response.getEmail();
            this.phoneNumber = response.getPhoneNumber();
            this.emailVerified = response.isEmailVerified();
            this.displayName = response.getDisplayName();
            this.photoUrl = response.getPhotoUrl();
            this.disabled = response.isDisabled();
            if (response.getProviders() == null || response.getProviders().Length == 0)
            {
                this.providers = new ProviderUserInfo[0];
            }
            else
            {
                this.providers = new ProviderUserInfo[response.getProviders().Length];
                for (int i = 0; i < this.providers.Length; i++)
                {
                    this.providers[i] = new ProviderUserInfo(response.getProviders()[i]);
                }
            }
            this.tokensValidAfterTimestamp = response.getValidSince() * 1000;
            this.userMetadata = new UserMetadata(response.getCreatedAt(), response.getLastLoginAt());
            this.customClaims = parseCustomClaims(response.getCustomClaims());
        }

        private Dictionary<string, object> parseCustomClaims(string customClaims)
        {
            if (String.IsNullOrEmpty(customClaims))
            {
                return null;
            }
            try
            {
                //Dictionary<String, Object> parsed = new Dictionary<string, object>();
                //JsonConvert.DeserializeObject.createJsonParser(customClaims).parseAndClose(parsed);
                var values = JsonConvert.DeserializeObject<Dictionary<string, object>>(customClaims);
                return values;
            }
            catch (Exception e)
            {
                throw new ArgumentException("Failed to parse custom claims json", e);
            }
        }

        public string getUid()
        {
            return uid;
        }

        public string getProviderId()
        {
            return PROVIDER_ID;
        }

        public string getEmail()
        {
            return email;
        }

        public string getPhoneNumber()
        {
            return phoneNumber;
        }

        public bool isEmailVerified()
        {
            return emailVerified;
        }

        public string getDisplayName()
        {
            return displayName;
        }

        public string getPhotoUrl()
        {
            return photoUrl;
        }

        public bool isDisabled()
        {
            return disabled;
        }

        public IUserInfo[] getProviderData()
        {
            return providers;
        }

        public long getTokensValidAfterTimestamp()
        {
            return tokensValidAfterTimestamp;
        }

        public UserMetadata getUserMetadata()
        {
            return this.userMetadata;
        }

        public Dictionary<String, Object> getCustomClaims()
        {
            return customClaims;
        }

        public UpdateRequest updateRequest()
        {
            return new UpdateRequest(uid);
        }

        static void checkUid(String uid)
        {
            checkArgument(!String.IsNullOrEmpty(uid), "uid cannot be null or empty");
            checkArgument(uid.Length <= 128, "UID cannot be longer than 128 characters");
        }

        static void checkEmail(String email)
        {
            checkArgument(!String.IsNullOrEmpty(email), "email cannot be null or empty");
            //checkArgument(RegularExpressions(email.matches("^[^@]+@[^@]+$"));
        }

        static void checkPhoneNumber(String phoneNumber)
        {
            // Phone number verification is very lax here. Backend will enforce E.164 spec compliance, and
            // normalize accordingly.
            checkArgument(!String.IsNullOrEmpty(phoneNumber), "phone number cannot be null or empty");
            checkArgument(phoneNumber.StartsWith("+"),
                "phone number must be a valid, E.164 compliant identifier starting with a '+' sign");
        }

        //static void checkUrl(String photoUrl)
        //{
        //    checkArgument(!String.IsNullOrEmpty(photoUrl), "url cannot be null or empty");
        //    try
        //    {
        //        new URL(photoUrl);
        //    }
        //    catch (MalformedURLException e)
        //    {
        //        throw new IllegalArgumentException("malformed url string", e);
        //    }
        //}
        private static void checkPassword(String password)
        {
            checkArgument(!String.IsNullOrEmpty(password), "password cannot be null or empty");
            checkArgument(password.Length >= 6, "password must be at least 6 characters long");
        }

        static void checkCustomClaims(Dictionary<String, Object> customClaims)
        {
            if (customClaims == null)
            {
                return;
            }
            foreach (String key in customClaims.Keys)
            {
                checkArgument(!String.IsNullOrEmpty(key), "Claim names must not be null or empty");
                checkArgument(!FirebaseUserManager.RESERVED_CLAIMS.Contains(key),
                    "Claim '" + key + "' is reserved and cannot be set");
            }
        }

        //private static void checkValidSince(long epochSeconds)
        //{
        //    checkArgument(epochSeconds > 0, "validSince (seconds since epoch) must be greater than 0: "
        //        + Long.toString(epochSeconds));
        //}

        //static String serializeCustomClaims(IDictionary customClaims, JsonFactory jsonFactory)
        //{
        //    checkNotNull(jsonFactory, "JsonFactory must not be null");
        //    if (customClaims == null || customClaims.isEmpty())
        //    {
        //        return "{}";
        //    }

        //    try
        //    {
        //        String claimsPayload = jsonFactory.toString(customClaims);
        //        checkArgument(claimsPayload.Length <= MAX_CLAIMS_PAYLOAD_SIZE,
        //            "customClaims payload cannot be larger than " + MAX_CLAIMS_PAYLOAD_SIZE + " characters");
        //        return claimsPayload;
        //    }
        //    catch (Exception e)
        //    {
        //        throw new ArgumentException("Failed to serialize custom claims into JSON", e);
        //    }
        //}

        public class CreateRequest
        {
            private Dictionary<String, Object> properties = new Dictionary<string, object>();
            public CreateRequest()
            {
            }
            public CreateRequest setUid(String uid)
            {
                checkUid(uid);
                properties.Add("localId", uid);
                return this;
            }
            public CreateRequest setEmail(String email)
            {
                checkEmail(email);
                properties.Add("email", email);
                return this;
            }
            public CreateRequest setPhoneNumber(String phone)
            {
                checkPhoneNumber(phone);
                properties.Add("phoneNumber", phone);
                return this;
            }
            public CreateRequest setEmailVerified(bool emailVerified)
            {
                properties.Add("emailVerified", emailVerified);
                return this;
            }
            public CreateRequest setDisplayName(String displayName)
            {
                checkArgument(!String.IsNullOrEmpty(displayName), "displayName cannot be null or empty");
                properties.Add("displayName", displayName);
                return this;
            }
            public CreateRequest setPhotoUrl(String photoUrl)
            {
                //checkUrl(photoUrl);
                properties.Add("photoUrl", photoUrl);
                return this;
            }
            public CreateRequest setDisabled(bool disabled)
            {
                properties.Add("disabled", disabled);
                return this;
            }
            public CreateRequest setPassword(String password)
            {
                checkPassword(password);
                properties.Add("password", password);
                return this;
            }
            public Dictionary<String, Object> getProperties()
            {
                return properties;
            }
        }
        public class UpdateRequest
        {
            private Dictionary<String, Object> properties = new Dictionary<string, object>();
            public UpdateRequest(String uid)
            {
                checkArgument(!String.IsNullOrEmpty(uid), "uid must not be null or empty");
                properties.Add("localId", uid);
            }

            String getUid()
            {
                return (string)properties["localId"];
            }

            public UpdateRequest setEmail(String email)
            {
                checkEmail(email);
                properties.Add("email", email);
                return this;
            }
            public UpdateRequest setPhoneNumber(String phone)
            {
                if (phone != null)
                {
                    checkPhoneNumber(phone);
                }
                properties.Add("phoneNumber", phone);
                return this;
            }
            public UpdateRequest setEmailVerified(bool emailVerified)
            {
                properties.Add("emailVerified", emailVerified);
                return this;
            }
            public UpdateRequest setDisplayName(String displayName)
            {
                properties.Add("displayName", displayName);
                return this;
            }

            public UpdateRequest setPhotoUrl(String photoUrl)
            {
                // This is allowed to be null
                //if (photoUrl != null)
                //{
                //    checkUrl(photoUrl);
                //}
                properties.Add("photoUrl", photoUrl);
                return this;
            }
            public UpdateRequest setDisabled(bool disabled)
            {
                properties.Add("disableUser", disabled);
                return this;
            }
            public UpdateRequest setPassword(String password)
            {
                checkPassword(password);
                properties.Add("password", password);
                return this;
            }
            public UpdateRequest setCustomClaims(Dictionary<String, Object> customClaims)
            {
                checkCustomClaims(customClaims);
                properties.Add(CUSTOM_ATTRIBUTES, customClaims);
                return this;
            }
            public UpdateRequest setValidSince(long epochSeconds)
            {
                //checkValidSince(epochSeconds);
                properties.Add("validSince", epochSeconds);
                return this;
            }

            //public Dictionary<String, Object> getProperties(JsonFactory jsonFactory)
            //{
            //    Dictionary<String, Object> copy = new Dictionary<string, object>(properties);
            //    List<String> remove = new List<String>();
            //    for (Dictionary.Entry<String, String> entry : REMOVABLE_FIELDS.entrySet())
            //    {
            //        if (copy.containsKey(entry.getKey()) && copy.get(entry.getKey()) == null)
            //        {
            //            remove.add(entry.getValue());
            //            copy.remove(entry.getKey());
            //        }
            //    }

            //    if (!remove.isEmpty())
            //    {
            //        copy.put("deleteAttribute", ImmutableList.copyOf(remove));
            //    }

            //    if (copy.containsKey("phoneNumber") && copy.get("phoneNumber") == null)
            //    {
            //        copy.put("deleteProvider", ImmutableList.of("phone"));
            //        copy.remove("phoneNumber");
            //    }

            //    if (copy.containsKey(CUSTOM_ATTRIBUTES))
            //    {
            //        Map customClaims = (Map)copy.remove(CUSTOM_ATTRIBUTES);
            //        copy.put(CUSTOM_ATTRIBUTES, serializeCustomClaims(customClaims, jsonFactory));
            //    }
            //    return ImmutableMap.copyOf(copy);
            //}
        }
    }
}