using System;

namespace FirebaseAdmin.Auth
{
    public interface IUserInfo
    {
        String getUid();
        String getDisplayName();
        String getEmail();
        String getPhoneNumber();
        String getPhotoUrl();
        String getProviderId();
    }
}
