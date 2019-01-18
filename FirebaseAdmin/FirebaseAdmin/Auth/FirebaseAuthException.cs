using System;

namespace FirebaseAdmin.Auth
{
    public class FirebaseAuthException : Exception
    {
        private String errorCode;

        public FirebaseAuthException(String errorCode, String detailMessage) : base(errorCode, new Exception(detailMessage))
        {
            this.errorCode = errorCode;
        }
        public String getErrorCode()
        {
            return errorCode;
        }
    }
}
