using Buttons.Data;
using Buttons.Models;
using Buttons.Services;
using Microsoft.AspNetCore.Mvc;

namespace Buttons.Controllers
{
    public class PrinterController : Controller
    {
        private readonly ButtonContext context;
        private readonly ILogger<ButtonController> logger;
        private readonly Configuration configuration;

        private Session Session => new(HttpContext.Session, context);

        public PrinterController(
            ButtonContext context,
            ILogger<ButtonController> logger,
            Configuration configuration)
        {
            this.context = context;
            this.logger = logger;
            this.configuration = configuration;
        }

        private ButtonViewModel CreateViewModel(Button button) =>
            new(button.Id, button.Path, button.Status.ToString(), button.Crop.Clone());

        public IActionResult Index()
        {
            var buttons = context.Buttons
                .Where(b => b.Status == ButtonStatus.Confirmed)
                .Select(CreateViewModel)
                .ToList();
            return View(new ButtonListViewModel(buttons));
        }
    }
}
