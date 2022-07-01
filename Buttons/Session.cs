namespace Buttons
{
    public class Session
    {
        private const string UserIdKey = "UserId";
        private readonly ISession session;

        public Session(ISession session) => this.session = session;

        /// <summary>
        /// Get the user id from the session data or generate one if it does not exist yet.
        /// </summary>
        public string GetUserId()
        {
            var id = session.GetString(UserIdKey);
            if (string.IsNullOrEmpty(id))
            {
                id = Guid.NewGuid().ToString();
                session.SetString(UserIdKey, id);
            }

            return id;
        }
    }
}
