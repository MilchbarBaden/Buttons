using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Buttons.Data;
using Buttons.Models;

namespace Buttons.Controllers
{
    public class ButtonController : Controller
    {
        private const string ButtonsFolder = "buttons";
        private readonly string buttonsPath;
        private readonly string[] allowedExtensions = new[] { ".png", ".jpg", ".jpeg" };

        private readonly ButtonContext context;
        private readonly ILogger<ButtonController> logger;

        private Session Session => new Session(HttpContext.Session);

        public ButtonController(
            ButtonContext context,
            ILogger<ButtonController> logger,
            IWebHostEnvironment environment)
        {
            this.context = context;
            this.logger = logger;
            buttonsPath = Path.Combine(environment.WebRootPath, ButtonsFolder);
        }

        public ActionResult Index()
        {
            return View();
        }

        private string GetSanitisedExtension(string fileName)
        {
            string? input = null;
            try { input = Path.GetExtension(fileName); }
            catch { } // Ignore invalid user input

            return allowedExtensions.FirstOrDefault(
                e => e.Equals(input, StringComparison.InvariantCultureIgnoreCase),
                allowedExtensions.First()
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(IFormCollection collection)
        {
            try
            {
                var files = collection.Files;
                if (!ModelState.IsValid || files.Count != 1)
                {
                    return View();
                }

                var file = files.Single();
                if (file == null || file.Length == 0)
                {
                    return View();
                }

                var extension = GetSanitisedExtension(file.FileName);
                string fileName = $"{Guid.NewGuid():N}{extension}";
                string targetPath = Path.Combine(buttonsPath, fileName);

                using var fileStream = new FileStream(targetPath, FileMode.Create);
                await file.CopyToAsync(fileStream);

                // Add button metadata to database.
                var button = new Button
                {
                    Name = file.FileName,
                    Path = fileName,
                    Status = ButtonStatus.Uploaded,
                    OwnerUserId = Session.GetUserId(),
                };
                context.Buttons.Add(button);
                await context.SaveChangesAsync();

                logger.LogInformation("Created button {} with filename '{}'", button.Id, targetPath);
                return RedirectToAction(nameof(Crop), null, new { id = button.Id });
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Crop(int id)
        {
            var button = context.Buttons.SingleOrDefault(b => b.Id == id);
            if (button == null)
            {
                logger.LogWarning("Button {} not found!", id);
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new ButtonViewModel(button.Id, button.Path, button.Crop.Clone());
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Crop(int id, double x, double y, double width, double height, double scaleX, double scaleY)
        {
            var button = context.Buttons.SingleOrDefault(b => b.Id == id);
            if (button == null)
            {
                logger.LogWarning("Button {} not found!", id);
                return RedirectToAction(nameof(Index));
            }

            button.Crop = new Crop()
            {
                Offset = (x, y),
                Size = (width, height),
                Scale = (scaleX, scaleY),
            };
            await context.SaveChangesAsync();

            return RedirectToAction(); // TODO
        }
    }
}
