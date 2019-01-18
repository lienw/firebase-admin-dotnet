using System;

namespace FirebaseAdmin
{
    public static class Preconditions
    {
        public static void checkArgument(bool status, string ex)
        {
            if (status == false)
                throw new Exception(ex);
        }
        public static void checkNotNull(object obj, string ex)
        {
            if (obj == null)
                throw new Exception(ex);
        }
    }
}
