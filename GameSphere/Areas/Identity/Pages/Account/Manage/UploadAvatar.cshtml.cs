using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GameSphere.Areas.Identity.Pages.Account.Manage
{
    public class UploadAvatarModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public UploadAvatarModel(UserManager<IdentityUser> userManager, IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _environment = environment;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Avatar")]
            public IFormFile Avatar { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var uploadsDirectory = Path.Combine(_environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsDirectory))
            {
                Directory.CreateDirectory(uploadsDirectory);
            }

            var fileExtension = Path.GetExtension(Input.Avatar.FileName).ToLower();
            var avatarPath = Path.Combine(uploadsDirectory, $"{user.Id}_avatar{fileExtension}");
            using (var stream = new FileStream(avatarPath, FileMode.Create))
            {
                await Input.Avatar.CopyToAsync(stream);
            }

            // Save the avatar path and extension to the user's profile or database if needed
            // For example, you can add properties to the IdentityUser class to store the avatar path and extension.

            return RedirectToPage("./Index");
        }
    }
}
