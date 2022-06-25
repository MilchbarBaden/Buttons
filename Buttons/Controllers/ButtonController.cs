using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;

namespace Buttons.Controllers
{
    public class ButtonController : Controller
    {
        private const string ButtonsPath = "buttons";

        private readonly ILogger<ButtonController> logger;
        private readonly IWebHostEnvironment environment;

        public ButtonController(ILogger<ButtonController> logger, IWebHostEnvironment environment)
        {
            this.logger = logger;
            this.environment = environment;
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
                var files = HttpContext.Request.Form.Files;
                if (!ModelState.IsValid || files.Count != 1)
                {
                    return View();
                }

                var file = files.Single();
                if (file == null || file.Length == 0)
                {
                    return View();
                }

                string buttonsFolder = Path.Combine(environment.WebRootPath, ButtonsPath);
                string fileName = Guid.NewGuid().ToString("N");
                string targetPath = Path.Combine(buttonsFolder, fileName);

                using var fileStream = new FileStream(targetPath, FileMode.Create);
                await file.CopyToAsync(fileStream);

                return RedirectToAction(nameof(Crop));
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Crop()
        {
            return View();
        }
    }
}
