using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xiphos.Shared.Authentication;

namespace Xiphos.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;

        public ConfirmEmailModel(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {
                await AssignDefaultRoles(user);
                StatusMessage = "Thank you for confirming your email.";
            }
            else
            {
                StatusMessage = "Error confirming your email.";
            }

            return Page();
        }

        private async Task AssignDefaultRoles(IdentityUser user)
        {
            // --Notable--
            // There is no role management in place in this rather simple app.
            // For the sake of simplicity, we will assume the first registered user is admin, the rest will be just users.
            if (_userManager.Users.Count() == 1)
            {
                await _userManager.AddToRolesAsync(user, new[] { UserRoles.User, UserRoles.Administrator });
            }
            else
            {
                await _userManager.AddToRoleAsync(user, UserRoles.User);
            }
        }
    }
}
