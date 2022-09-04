namespace Buttons.Services
{
    public class PasswordManager
    {
        record Password(string Value, int AccessVersion);

        private const int InitialAccessVersion = 1;
        private readonly SemaphoreSlim passwordLock = new SemaphoreSlim(1, 1);
        private Password? password = null;

        public bool HasPassword => password != null;
        public int CurrentAccessVersion => password?.AccessVersion ?? 0;

        public async Task<(bool success, int accessVersion)> SetPasswordAsync(string password)
        {
            await passwordLock.WaitAsync();
            try
            {
                if (this.password == null)
                {
                    Password initialPassword = new(password, InitialAccessVersion);
                    var original = Interlocked.CompareExchange(ref this.password, initialPassword, null);
                    return (original == null, initialPassword.AccessVersion);
                }
                return (false, 0);
            }
            finally
            {
                passwordLock.Release();
            }
        }

        public async Task<(bool success, int accessVersion)> CheckPasswordAsync(string passwordToCheck)
        {
            await passwordLock.WaitAsync();
            try
            {
                // Bruteforce guessing protection. (Allows for only 1 guess per ~500ms.)
                await Task.Delay(500);
                var password = this.password;
                var success = passwordToCheck.Equals(password?.Value, StringComparison.InvariantCulture);
                return (success, success ? password?.AccessVersion ?? 0 : 0);
            }
            finally
            {
                passwordLock.Release();
            }
        }

        public async Task<(bool success, int accessVersion)> ChangePasswordAsync(string oldPassword, string newPassword)
        {
            await passwordLock.WaitAsync();
            try
            {
                Password? initialValue, computedValue;
                do
                {
                    initialValue = password;
                    await Task.Delay(500);
                    if (!oldPassword.Equals(initialValue?.Value, StringComparison.InvariantCulture))
                    {
                        return (false, 0);
                    }
                    computedValue = new Password(newPassword, initialValue.AccessVersion + 1);
                } while (!ReferenceEquals(initialValue, Interlocked.CompareExchange(ref password, computedValue, initialValue)));
                return (true, computedValue.AccessVersion);
            }
            finally
            {
                passwordLock.Release();
            }
        }
    }
}
