﻿using Microsoft.AspNetCore.Http;
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

        private Session Session => new Session(HttpContext.Session, context);

        public ButtonController(
            ButtonContext context,
            ILogger<ButtonController> logger,
            IWebHostEnvironment environment)
        {
            this.context = context;
            this.logger = logger;
            buttonsPath = Path.Combine(environment.WebRootPath, ButtonsFolder);
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

        private ButtonViewModel CreateViewModel(Button button) =>
            new(button.Id, button.Path, button.Crop.Clone());

        public async Task<ActionResult> Index()
        {
            var owner = await Session.GetOrCreateOwnerAsync();
            var buttons = context.Buttons
                .Where(b => b.OwnerId == owner.Id)
                .Select(CreateViewModel)
                .ToList();
            return View(new ButtonListViewModel(buttons));
        }

        public async Task<ActionResult> Name()
        {
            var owner = await Session.GetOrCreateOwnerAsync();
            return View(new NameViewModel(owner.Name));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Name(string username)
        {
            var owner = await Session.GetOrCreateOwnerAsync();
            if (owner == null || string.IsNullOrEmpty(username))
            {
                return RedirectToAction(nameof(Name));
            }

            owner.Name = username;
            await context.SaveChangesAsync();

            return RedirectToAction(nameof(Create));
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(IFormCollection collection)
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
                var owner = await Session.GetOrCreateOwnerAsync();
                var button = new Button(owner)
                {
                    Name = file.FileName,
                    Path = fileName,
                    Status = ButtonStatus.Uploaded,
                };
                context.Buttons.Add(button);
                await context.SaveChangesAsync();

                logger.LogInformation("Created button {} with filename '{}' for owner {}", button.Id, targetPath, owner.Id);
                return RedirectToAction(nameof(Crop), null, new { id = button.Id });
            }
            catch
            {
                return View();
            }
        }

        public async Task<ActionResult> Crop(int id)
        {
            var owner = await Session.GetOrCreateOwnerAsync();
            var button = context.Buttons.SingleOrDefault(b => b.Id == id);
            if (button == null || button.OwnerId != owner.Id)
            {
                logger.LogWarning("Button {} not found or tried to access unowned button by {}!", id, owner.Id);
                return RedirectToAction(nameof(Index));
            }

            return View(CreateViewModel(button));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Crop(int id, double x, double y, double width, double height, double scaleX, double scaleY)
        {
            var owner = await Session.GetOrCreateOwnerAsync();
            var button = context.Buttons.SingleOrDefault(b => b.Id == id);
            if (button == null || button.OwnerId != owner.Id)
            {
                logger.LogWarning("Button {} not found or tried to access unowned button by {}!", id, owner.Id);
                return RedirectToAction(nameof(Index));
            }

            // Cannot change a button that has already been printed.
            //
            // This is only handled on the post action:
            // For printed buttons no edit link should be shown,
            // so this action should not be reached through regular means.
            if (button.Status == ButtonStatus.Printed)
            {
                return RedirectToAction(nameof(Index));
            }

            button.Crop = new Crop()
            {
                Offset = (x, y),
                Size = (width, height),
                Scale = (scaleX, scaleY),
            };
            button.Status = ButtonStatus.Uploaded;
            await context.SaveChangesAsync();

            return RedirectToAction(nameof(Confirm), null, new { id });
        }

        public async Task<ActionResult> Confirm(int id)
        {
            var owner = await Session.GetOrCreateOwnerAsync();
            var button = context.Buttons.SingleOrDefault(b => b.Id == id);
            if (button == null || button.OwnerId != owner.Id)
            {
                logger.LogWarning("Button {} not found or tried to access unowned button by {}!", id, owner.Id);
                return RedirectToAction(nameof(Index));
            }

            return View(CreateViewModel(button));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Confirm(int id, IFormCollection collection)
        {
            if (!collection.ContainsKey("confirm"))
            {
                return RedirectToAction(nameof(Crop), null, new { id });
            }

            var owner = await Session.GetOrCreateOwnerAsync();
            var button = context.Buttons.SingleOrDefault(b => b.Id == id);
            if (button == null || button.OwnerId != owner.Id)
            {
                logger.LogWarning("Button {} not found or tried to access unowned button by {}!", id, owner.Id);
                return RedirectToAction(nameof(Index));
            }

            // Cannot change a button that has already been printed.
            if (button.Status == ButtonStatus.Printed)
            {
                return RedirectToAction(nameof(Index));
            }

            button.Status = ButtonStatus.Confirmed;
            await context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
