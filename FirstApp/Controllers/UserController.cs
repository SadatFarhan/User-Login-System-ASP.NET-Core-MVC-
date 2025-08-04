using FirstApp.Data;
using FirstApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FirstApp.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _context;
        public UserController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            // এখানে সমস্ত ব্যবহারকারী লোড করা হচ্ছে, কোনো ফিল্টার ছাড়াই।
            var users = await _context.Users
                .Select(u => new Users
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    LastSeen = u.LastSeen,
                    IsBlocked = u.IsBlocked // নিশ্চিত করুন যে IsBlocked প্রপার্টিও সিলেক্ট করা হচ্ছে
                })
                .ToListAsync();
            return View(users);
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(Registration model)
        {
            if (ModelState.IsValid)
            {
                Users user = new Users();

                user.Name = model.Name;
                user.Email = model.Email;
                user.Password = model.Password;
                user.ConfirmPassword = model.ConfirmPassword;


                try
                {
                    _context.Users.Add(user);
                    _context.SaveChanges();
                    ModelState.Clear();

                    ViewBag.Message = $"{user.Name}{user.Email}Registration successful!";

                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Registration failed. The email or username may already be in use.");
                    return View(model);
                }
                return View();
            }
            return View(model);
        }



        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "User");
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(Login model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users.Where(u => (u.Email == model.NameOrEmail || u.Name == model.NameOrEmail) && u.Password == model.Password)
                    .FirstOrDefault();

                if (user != null)
                {
                    if (user.IsBlocked)
                    {
                        ModelState.AddModelError(string.Empty, "Your account is blocked.");
                        return View(model);
                    }

                    user.LastSeen = DateTime.UtcNow;
                    _context.SaveChanges();

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.Email),
                        new Claim("Name", user.Name),
                        new Claim(ClaimTypes.Role, "User"),
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid email or password.");
                }
            }
            return View(model);
        }



        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }






        public async Task<IActionResult> Block(int? id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.IsBlocked = true;
            _context.Update(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "User");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUsersStatus(string action, int[] userIds)
        {
            if (userIds == null || userIds.Length == 0)
            {
                return RedirectToAction(nameof(Index));
            }

            var usersToUpdate = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();

            if (usersToUpdate.Count == 0)
            {
                return RedirectToAction(nameof(Index));
            }

            switch (action)
            {
                case "Block":
                    foreach (var user in usersToUpdate)
                    {
                        user.IsBlocked = true;
                    }
                    break;
                case "Unblock":
                    foreach (var user in usersToUpdate)
                    {
                        user.IsBlocked = false;
                    }
                    break;
                case "Delete":
                    _context.Users.RemoveRange(usersToUpdate);
                    break;
                default:
                    break;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}