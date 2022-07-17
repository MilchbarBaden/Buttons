namespace Buttons.Services
{
    public class Configuration
    {
        private const string ButtonsFolder = "buttons";

        public string ButtonsPath { get; init; }

        public Configuration(IWebHostEnvironment environment)
        {
            ButtonsPath = Path.Combine(environment.WebRootPath, ButtonsFolder);
        }
    }
}
