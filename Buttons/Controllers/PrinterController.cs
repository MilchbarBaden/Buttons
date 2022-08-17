using Buttons.Data;
using Buttons.Models;
using Buttons.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        private AdminButtonViewModel CreateViewModel(Button button) =>
            new(button.Id, button.Path, button.Status.ToString(), button.Crop.Clone(), button.Name, button.Owner.Id, button.Owner.Name);

        private async Task MarkButtonsAsPrinted(IEnumerable<Button> buttons)
        {
            foreach (var button in buttons)
            {
                button.Status = ButtonStatus.Printed;
            }
            await context.SaveChangesAsync();
        }

        private async Task<IActionResult> DeleteButtons(int[] buttons)
        {
            HashSet<int> buttonsToPrint = new(buttons);
            var selectedButtons = context.Buttons
                .Where(b => b.Status >= ButtonStatus.Confirmed)
                .Where(b => buttonsToPrint.Contains(b.Id));

            context.Buttons.RemoveRange(selectedButtons);
            await context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Index()
        {
            var buttons = context.Buttons
                .Include(b => b.Owner)
                .Where(b => b.Status >= ButtonStatus.Confirmed)
                .OrderByDescending(b => b.LastModified)
                .Select(CreateViewModel)
                .ToList();
            return View(new AdminButtonListViewModel(buttons));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Print(int[] buttons, IFormCollection collection)
        {
            if (collection.ContainsKey("delete-selection"))
            {
                return await DeleteButtons(buttons);
            }

            IQueryable<Button> buttonQuery;
            if (collection.ContainsKey("print-all"))
            {
                buttonQuery = context.Buttons
                    .Where(b => b.Status == ButtonStatus.Confirmed);
            }
            else if (collection.ContainsKey("print-selection"))
            {
                if (buttons == null || buttons.Length == 0)
                {
                    return RedirectToAction(nameof(Index));
                }
                HashSet<int> buttonsToPrint = new(buttons);

                buttonQuery = context.Buttons
                    .Where(b => b.Status >= ButtonStatus.Confirmed)
                    .Where(b => buttonsToPrint.Contains(b.Id));
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }

            // Make sure the query is only evaluated once.
            var buttonEntities = buttonQuery
                .Include(b => b.Owner)
                .OrderBy(b => b.Created)
                .ToList();

            if (!buttonEntities.Any())
            {
                logger.LogWarning("No buttons found for printing");
                return RedirectToAction(nameof(Index)); // TODO: Close instead of redirect
            }

            await MarkButtonsAsPrinted(buttonEntities);

            var buttonViewModels = buttonEntities.Select(CreateViewModel).ToList();
            return View(new AdminButtonListViewModel(buttonViewModels));
        }
    }
}
