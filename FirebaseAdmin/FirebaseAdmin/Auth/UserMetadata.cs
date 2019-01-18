namespace FirebaseAdmin.Auth
{
    public class UserMetadata
    {
        private readonly long creationTimestamp;
        private readonly long lastSignInTimestamp;

        public UserMetadata(long creationTimestamp = 0L)
        {
            this.creationTimestamp = creationTimestamp;
        }

        public UserMetadata(long creationTimestamp, long lastSignInTimestamp)
        {
            this.creationTimestamp = creationTimestamp;
            this.lastSignInTimestamp = lastSignInTimestamp;
        }
        public long getCreationTimestamp()
        {
            return creationTimestamp;
        }
        public long getLastSignInTimestamp()
        {
            return lastSignInTimestamp;
        }
    }
}
