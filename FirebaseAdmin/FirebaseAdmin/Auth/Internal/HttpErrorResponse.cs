using System;
using System.Collections.Generic;
using System.Text;

namespace FirebaseAdmin.Auth.Internal
{
    public class HttpErrorResponse : Exception
    {
        public HttpError Error { get; set; } = null;
    }
    public class HttpError
    {
        public List<HttpErrorInfo> Errors { get; set; } = null;
        public int Code { get; set; }
        public string Message { get; set; }
    }
    public class HttpErrorInfo
    {
        public string Domain { get; set; }
        public string Reason { get; set; }
        public string Message { get; set; }
    }
}
