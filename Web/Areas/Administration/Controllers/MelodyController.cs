using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xiphos.Areas.Administration.Models;
using Xiphos.Data.Models;
using Xiphos.Data.ProductDatabase;
using Xiphos.Shared.Authentication;

namespace Xiphos.Areas.Administration.Controllers
{

    // --Notable--
    // The controller is marked with attribute Authorize and requires a logged in user.
    // This means all routes will undertake prior check for such. For post requests
    // authorization is mandatory unless it is some unauthorized public API.
    //
    // The whole controller or it's particular routes may/may not require a policy or a role.
    // https://docs.microsoft.com/en-us/aspnet/core/security/authorization/roles?view=aspnetcore-5.0
    // https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-5.0
    //
    // There are alternative approaches to authorization checks on a method level such as:
    //  - Simple authentication check - User.Identity.IsAuthenticated
    //  - Role check - User.IsInRole(...)
    //  - Use claims to carry an additional information - User.Claims.Where(...)
    // Note that method level checks will introduce new objects you need to mock in tests.
    //
    // Additionally POST should be secured with anti-XSRF measures like ValidateAntiForgeryToken attribute.
    // https://docs.microsoft.com/en-us/aspnet/core/security/anti-request-forgery?view=aspnetcore-5.0
    // ! Not rendering the UI does not prevent a custom HTTP request crafting.

    /// <summary>
    /// Melody administration controller
    /// </summary>
    [Authorize(Roles = Authorize.UserOrAdministrator)]
    [Area("Administration")]
    public class MelodyController : Controller
    {
        private readonly ILogger _logger;
        private readonly ProductDbContext _dbContext;

        private const string CreateOperationName = "CreateMelody";
        private const string EditOperationName = "EditMelody";

        // --Notable--
        // Property binding is another way how to carry data from query, header or forms.
        // Default support is for POST only.
        [BindProperty(SupportsGet = true)]
        [FromQuery]
        public IDictionary<string, string> Query { get; set; }

        public MelodyController(ProductDbContext dbContext, ILogger<MelodyController> logger)
            => (_dbContext, _logger) =
                (
                    dbContext ?? throw new ArgumentNullException(nameof(dbContext)),
                    logger ?? throw new ArgumentNullException(nameof(logger))
                );

        /// <summary>
        /// Melody indexing action returns a grid view with melodies in database.
        /// </summary>
        /// <param name="sort">Optional sorting property and direction</param>
        /// <param name="filter">Optional filtering string</param>
        /// <param name="pageIndex">Optional page index</param>
        /// <param name="pageSize">Optional page size</param>
        /// <returns>Melody grid view</returns>
        [HttpGet]
        public async Task<ActionResult> Index(string sort, string filter, int? pageIndex, int? pageSize)
        {
            // --Notable--
            //  Edit elements are hidden for non-admins.
            //  View data will allow to adjust the visuals.
            ViewBag.IsAdmin = User.IsInRole(UserRoles.Administrator);

            // Used for delete action link so we wont loose the view configuration
            ViewBag.QueryString = Request.QueryString;

            var pagedModel = await MelodyListModel.FetchAsync(
                 _dbContext.Melodies.AsQueryable(),
                 sort,
                 filter,
                 pageIndex,
                 pageSize);

            return View(pagedModel);
        }

        // --Notable--
        //   Create action is administrator only option which is simply ensured by the role attribute.
        /// <summary>
        /// Displays R/W melody editor with default values
        /// </summary>
        /// <param name="query">Query string as dictionary</param>
        /// <returns>Editor view</returns>
        [HttpGet]
        [Authorize(Roles = Authorize.Administrator)]
        public ActionResult Create()
        {
            StoreReturnUrl();

            ViewBag.Header = "Create New Melody";
            ViewBag.Operation = CreateOperationName;
            ViewBag.ReadOnly = false;

            return View("~/Areas/Administration/Views/Melody/MelodyEditor.cshtml", null);
        }



        // --Notable--
        //   Parsing querystring as dictionary is easily doable with FromQuery attribute.
        /// <summary>
        /// Displays R/W melody editor
        /// </summary>
        /// <param name="id">Edited melody Id</param>
        /// <param name="query">Query string as dictionary</param>
        /// <returns>Editor view</returns>
        [HttpGet]
        [Authorize(Roles = Authorize.Administrator)]
        public async Task<ActionResult> Edit(int id)
        {
            StoreReturnUrl();

            var melody = await _dbContext.Melodies.FirstOrDefaultAsync(m => m.Id == id);

            if (melody == null)
                throw new ArgumentException($"Melody {id} not found");

            // --Notable--
            // ViewBag and ViewData serves the same purpose.
            // ViewData is a dictionary, ViewBag is a dynamic object easing the syntax a bit.

            ViewBag.Header = "Edit Melody";
            ViewBag.Operation = EditOperationName;
            ViewBag.ReadOnly = false;

            return View("~/Areas/Administration/Views/Melody/MelodyEditor.cshtml", melody);
        }

        /// <summary>
        /// Displays read-only melody editor for melody with given Id
        /// </summary>
        /// <param name="id">Melody id</param>
        /// <param name="query">Query string as a dictionary</param>
        /// <returns>Read-only editor view</returns>
        [HttpGet]
        public async Task<ActionResult> View(int id)
        {
            StoreReturnUrl();

            var melody = await _dbContext.Melodies.FirstOrDefaultAsync(m => m.Id == id);

            if (melody == null)
                throw new ArgumentException($"Melody {id} not found");

            ViewBag.Header = "Melody Details";
            ViewBag.ReadOnly = true;

            return View("~/Areas/Administration/Views/Melody/MelodyEditor.cshtml", melody);
        }

        // --Notable--
        // An implicit advantage, when using Html templating, is that you get model binding for free.
        // I can now add a function argument MelodyModel melodyModel that will be bind with form data.
        // Binding mechanics give more flexibility and multiple ways to achieve the data transfer.
        //
        // More info:
        // https://docs.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-5.0
        //
        // You can also inject a generic collection IFormCollection collection that parses data from
        // the request or with HttpRequest directly.
        /// <summary>
        /// Saves given melody to the database
        /// </summary>
        /// <param name="operation">Save operation type (create|edit)</param>
        /// <param name="returnUrl">Where to go next</param>
        /// <param name="melodyModel">Data model</param>
        /// <returns>Index view with given query</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Authorize.Administrator)]
        public async Task<ActionResult> Save(string operation, string returnUrl, MelodyModel melodyModel)
        {
            _logger.LogInformation("Write operation requested {0}");

            // ModelState.IsValid reflects prior model validation. 
            // Invalid model could be caused by data annotation violation, conversion errors etc.
            //
            // See:
            // https://docs.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-5.0
            if (!ModelState.IsValid)
            {
                var errors = string.Join(Environment.NewLine,
                    ModelState.Values.Where(v => v.ValidationState == ModelValidationState.Invalid)
                        .SelectMany(p => p.Errors.Select(e => e.ErrorMessage)));

                throw new InvalidOperationException($"Operation {operation} failed. Model is invalid.{Environment.NewLine}{errors}");
            }

            melodyModel.Data = MelodyHelper.FixMelodyFormat(melodyModel.Data);

            switch (operation)
            {
                case CreateOperationName:
                    {
                        await _dbContext.Melodies.AddAsync(melodyModel);
                        await _dbContext.SaveChangesAsync();
                    }
                    break;
                case EditOperationName:
                    {
                        _dbContext.Melodies.Update(melodyModel);
                        await _dbContext.SaveChangesAsync();
                    }
                    break;

                default: throw new InvalidOperationException($"Operation {operation} is invalid.");
            }

            return SafeRedirect(returnUrl);
        }

        /// <summary>
        /// Deletes a melody from the database
        /// </summary>
        /// <param name="id"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Authorize.Administrator)]
        public async Task<ActionResult> Delete(int id, string returnUrl)
        {
            var melody = await _dbContext.Melodies.FirstOrDefaultAsync(m => m.Id == id);

            if (melody == null)
                throw new ArgumentException($"Melody {id} not found");

            _dbContext.Melodies.Remove(melody);
            await _dbContext.SaveChangesAsync();

            return SafeRedirect(returnUrl);
        }

        private void StoreReturnUrl()
        {
            if (Query.TryGetValue("returnUrl", out string url))
            {
                ViewBag.ReturnUrl = url;
            }
            else
            {
                ViewBag.ReturnUrl = string.Empty;
            }
        }

        private ActionResult SafeRedirect(string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl))
            {
                // go to default view
                return View();
            }

            return Redirect(returnUrl);
        }
    }
}
