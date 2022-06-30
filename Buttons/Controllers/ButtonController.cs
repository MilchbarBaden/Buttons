using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Buttons.Data;
using Buttons.Models;

namespace Buttons.Controllers
{
    public class ButtonController : Controller
    {
        private const string ButtonsFolder = "buttons";
        private readonly string[] allowedExtensions = new[] { ".png", ".jpg", ".jpeg" };

        private readonly ButtonContext context;
        private readonly ILogger<ButtonController> logger;
        private readonly string buttonsPath;

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

                var extension = Path.GetExtension(file.FileName);
                if (!allowedExtensions.Contains(extension, StringComparer.InvariantCultureIgnoreCase))
                {
                    extension = allowedExtensions.FirstOrDefault();
                }

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
    }
}
