using System.Collections.Generic;

namespace FirebaseAdmin.Auth.Internal
{
    public class GetAccountInfoResponse
    {
        public string kind;
        public List<User> users;
        public string getKind()
        {
            return kind;
        }
        public List<User> getUsers()
        {
            return users;
        }
        public class User
        {
            public string localId;
            public string email;

            public string phoneNumber;
            public bool emailVerified;
            public string displayName;
            public string photoUrl;
            public bool disabled;
            public Provider[] providers;
            public long createdAt;
            public long lastLoginAt;
            public long validSince;
            public string customClaims;
            public string getUid()
            {
                return localId;
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
            public Provider[] getProviders()
            {
                return providers;
            }
            public long getCreatedAt()
            {
                return createdAt;
            }
            public long getLastLoginAt()
            {
                return lastLoginAt;
            }
            public long getValidSince()
            {
                return validSince;
            }
            public string getCustomClaims()
            {
                return customClaims;
            }
        }
        public class Provider
        {
            public string uid;

            public string displayName;

            public string email;

            public string phoneNumber;

            public string photoUrl;

            public string providerId;

            public string getUid()
            {
                return uid;
            }

            public string getDisplayName()
            {
                return displayName;
            }

            public string getEmail()
            {
                return email;
            }

            public string getPhoneNumber()
            {
                return phoneNumber;
            }

            public string getPhotoUrl()
            {
                return photoUrl;
            }

            public string getProviderId()
            {
                return providerId;
            }
        }
    }
}
