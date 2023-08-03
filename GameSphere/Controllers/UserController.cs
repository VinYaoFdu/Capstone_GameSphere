using GameSphere.Data;
using GameSphere.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Threading.Tasks;

namespace GameSphere.Controllers
{
    [Authorize(Roles = "admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public UsersController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        public IActionResult Create()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserView newUser)
        {
            if (!ModelState.IsValid)
            {
                return View(newUser);
            }

            var user = await _userManager.FindByNameAsync(newUser.Email);

            if (user != null)
            {
                ViewBag.Error = "This Email already exists.";
                return View(newUser);
            }

            user = new IdentityUser
            {
                UserName = newUser.Email,
                Email = newUser.Email,
                EmailConfirmed = true
            };
            var result = await _userManager.CreateAsync(user, newUser.Password);

            if (result.Succeeded)
            {
                var memberExists = await _roleManager.RoleExistsAsync(UserRole.member.ToString());
                if (!memberExists)
                {
                    await _roleManager.CreateAsync(new IdentityRole(UserRole.member.ToString()));
                }

                await _userManager.AddToRoleAsync(user, UserRole.member.ToString());

                return RedirectToAction(nameof(Index));
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(newUser);
            }
        }

        public async Task<IActionResult> Edit(string Id)
        {
            var user = await _userManager.Users
            .Where(x => x.Id == Id)
            .SingleOrDefaultAsync();


            if (user == null)
            {
                ViewBag.Error = "User does not exist.";
                return View();
            }

            return View(new UserView
            {
                Email = user.UserName,
                Password = "",
                ConfirmPassword = ""
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserView editUser)
        {
            if (!ModelState.IsValid)
            {
                return View(editUser);
            }

            var user = await _userManager.Users
            .Where(x => x.UserName == editUser.Email)
            .SingleOrDefaultAsync();

            if (user == null)
            {
                ViewBag.Error = "User does not exist.";
                return View(editUser);
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, editUser.Password);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(string Id)
        {
            var user = await _userManager.Users
            .Where(x => x.Id == Id)
            .SingleOrDefaultAsync();


            if (user == null)
            {
                ViewBag.Error = "User does not exist.";
                return View();
            }

            var roles = await _context.Roles
                .Join(_context.UserRoles,
                    roles => roles.Id,
                    userRoles => userRoles.RoleId,
                    (roles, userRoles) => new {
                        roles.Name,
                        userRoles.UserId
                    })
                .Where(x => x.UserId == user.Id)
                .Select(r => new
                {
                    r.Name
                }).ToListAsync();

            List<string> roleList = new List<string>();
            foreach (var item in roles)
            {
                roleList.Add(item.Name);
            }

            return View(new UserView
            {
                Email = user.UserName,
                Password = "",
                ConfirmPassword = "",
                Role = roleList
            });
        }

        public async Task<IActionResult> Delete(string Id)
        {
            var user = await _userManager.Users
            .Where(x => x.Id == Id)
            .SingleOrDefaultAsync();


            if (user == null)
            {
                ViewBag.Error = "User does not exist.";
                return View();
            }

            var roles = await _context.Roles
                .Join(_context.UserRoles,
                    roles => roles.Id,
                    userRoles => userRoles.RoleId,
                    (roles, userRoles) => new {
                        roles.Name,
                        userRoles.UserId
                    })
                .Where(x => x.UserId == user.Id)
                .Select(r => new
                {
                    r.Name
                }).ToListAsync();

            List<string> roleList = new List<string>();
            foreach (var item in roles)
            {
                roleList.Add(item.Name);
            }

            return View(new UserView
            {
                Id = user.Id,
                Email = user.UserName,
                Password = "",
                ConfirmPassword = "",
                Role = roleList
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string Id)
        {
            var user = await _userManager.Users
            .Where(x => x.Id == Id)
            .SingleOrDefaultAsync();


            if (user == null)
            {
                ViewBag.Error = "User does not exist.";
                return RedirectToAction(nameof(Delete));
            }


            var rolesForUser = await _userManager.GetRolesAsync(user);

            using (var transaction = _context.Database.BeginTransaction())
            {
                if (rolesForUser.Count() > 0)
                {
                    foreach (var item in rolesForUser.ToList())
                    {
                        // item should be the name of the role
                        var result = await _userManager.RemoveFromRoleAsync(user, item);
                    }
                }

                await _userManager.DeleteAsync(user);
                transaction.Commit();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
