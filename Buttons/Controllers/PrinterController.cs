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
        private readonly PasswordManager passwordManager;

        private Session Session => new(HttpContext.Session, context);

        public PrinterController(
            ButtonContext context,
            ILogger<ButtonController> logger,
            Configuration configuration,
            PasswordManager passwordManager)
        {
            this.context = context;
            this.logger = logger;
            this.configuration = configuration;
            this.passwordManager = passwordManager;
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

        private bool HasAccess
        {
            get
            {
                int? sessionAccessVersion = Session.GetAdminAccessVersion();
                return sessionAccessVersion != null &&
                    sessionAccessVersion == passwordManager.CurrentAccessVersion;
            }
        }

        public IActionResult Index()
        {
            if (!passwordManager.HasPassword)
            {
                return RedirectToAction(nameof(SetPassword));
            }

            if (!HasAccess)
            {
                return RedirectToAction(nameof(Login));
            }

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
            if (!HasAccess)
            {
                return RedirectToAction(nameof(Login));
            }

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

            if (buttonEntities.Any())
            {
                await MarkButtonsAsPrinted(buttonEntities);
            }

            var buttonViewModels = buttonEntities.Select(CreateViewModel).ToList();
            return View(new AdminButtonListViewModel(buttonViewModels));
        }

        public IActionResult SetPassword()
        {
            if (passwordManager.HasPassword)
            {
                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(string password)
        {
            if (passwordManager.HasPassword || password == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var (success, accessVersion) = await passwordManager.SetPasswordAsync(password);

            if (!success)
            {
                return RedirectToAction(nameof(SetPassword));
            }

            logger.LogInformation("Password was set by {}", HttpContext.Connection.RemoteIpAddress);
            Session.SetAdminAccessVersion(accessVersion);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Login()
        {
            if (!passwordManager.HasPassword)
            {
                return RedirectToAction(nameof(SetPassword));
            }

            if (HasAccess)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string password)
        {
            if (!passwordManager.HasPassword)
            {
                return RedirectToAction(nameof(SetPassword));
            }

            if (HasAccess || password == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var (success, accessVersion) = await passwordManager.CheckPasswordAsync(password);

            if (!success)
            {
                logger.LogWarning("Failed login attempt from {}", HttpContext.Connection.RemoteIpAddress);
                return View(new LoginViewModel("Entered password is invalid."));
            }

            logger.LogInformation("Successful login from {}", HttpContext.Connection.RemoteIpAddress);
            Session.SetAdminAccessVersion(accessVersion);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult ChangePassword()
        {
            if (!passwordManager.HasPassword)
            {
                return RedirectToAction(nameof(SetPassword));
            }

            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword)
        {
            if (!passwordManager.HasPassword)
            {
                return RedirectToAction(nameof(SetPassword));
            }

            if (oldPassword == null || newPassword == null)
            {
                return RedirectToAction(nameof(ChangePassword));
            }

            var (success, accessVersion) = await passwordManager.ChangePasswordAsync(oldPassword, newPassword);

            if (!success)
            {
                logger.LogWarning("Failed to change password from {}", HttpContext.Connection.RemoteIpAddress);
                return View(new ChangePasswordViewModel("Entered password is invalid."));
            }

            logger.LogInformation("Successful password change from {}", HttpContext.Connection.RemoteIpAddress);
            Session.SetAdminAccessVersion(accessVersion);
            return RedirectToAction(nameof(Index));
        }
    }
}
