using Buttons.Data;

namespace Buttons.Services
{
    public class Session
    {
        private const string UserIdKey = "UserId";
        private const string AdminAccessVersionKey = "AdminAccessVersion";

        private readonly ISession session;
        private readonly ButtonContext context;

        public Session(ISession session, ButtonContext context)
        {
            this.session = session;
            this.context = context;
        }

        /// <summary>
        /// Get the user id from the session data or generate one if it does not exist yet.
        /// </summary>
        private string GetUserId()
        {
            var id = session.GetString(UserIdKey);
            if (string.IsNullOrEmpty(id))
            {
                id = Guid.NewGuid().ToString();
                session.SetString(UserIdKey, id);
            }

            return id;
        }

        /// <summary>
        /// Get the owner from the session data and database or generate one if it does not exist yet.
        /// </summary>
        public async Task<Owner> GetOrCreateOwnerAsync()
        {
            var userId = GetUserId();
            var owner = context.Owners.SingleOrDefault(o => o.Id == userId);
            if (owner == null)
            {
                owner = new Owner() { Id = userId, Name = string.Empty };
                context.Owners.Add(owner);
                await context.SaveChangesAsync();
            }
            return owner;
        }

        public int? GetAdminAccessVersion() => session.GetInt32(AdminAccessVersionKey);

        public void SetAdminAccessVersion(int accessVersion) => session.SetInt32(AdminAccessVersionKey, accessVersion);
    }
}
