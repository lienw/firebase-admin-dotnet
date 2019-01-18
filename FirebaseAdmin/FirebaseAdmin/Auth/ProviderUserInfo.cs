using FirebaseAdmin.Auth.Internal;
using System;

namespace FirebaseAdmin.Auth
{
    public class ProviderUserInfo : IUserInfo
    {
        private readonly String uid;
        private readonly String displayName;
        private readonly String email;
        private readonly String phoneNumber;
        private readonly String photoUrl;
        private readonly String providerId;

        public ProviderUserInfo(GetAccountInfoResponse.Provider response)
        {
            this.uid = response.getUid();
            this.displayName = response.getDisplayName();
            this.email = response.getEmail();
            this.phoneNumber = response.getPhoneNumber();
            this.photoUrl = response.getPhotoUrl();
            this.providerId = response.getProviderId();
        }
        public String getUid()
        {
            return uid;
        }
        public String getDisplayName()
        {
            return displayName;
        }
        public String getEmail()
        {
            return email;
        }
        public String getPhoneNumber()
        {
            return phoneNumber;
        }
        public String getPhotoUrl()
        {
            return photoUrl;
        }
        public String getProviderId()
        {
            return providerId;
        }
    }
}
