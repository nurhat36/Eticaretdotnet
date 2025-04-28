using ETicaret.Data;
using ETicaret.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ETicaret.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AccountController(AppDbContext context, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // LOGIN GET
        public IActionResult Login()
        {
            return View();
        }

        // LOGIN POST
        // POST Login işlemi:
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                Console.WriteLine("Email ve şifre gereklidir.");
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                Console.WriteLine("Email .");
                var result = await _signInManager.PasswordSignInAsync(user, password, isPersistent: false, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    // Session bilgileri kaydet
                    HttpContext.Session.SetString("UserId", user.Id);
                    HttpContext.Session.SetString("UserName", user.UserName);
                    HttpContext.Session.SetString("UserRole", user.Role);

                    return RedirectToAction("Index", "Home");
                }
                else if (result.IsLockedOut)
                {
                    Console.WriteLine( "Hesabınız kilitlendi. Lütfen daha sonra tekrar deneyin.");
                    return View();
                }
            }

            Console.WriteLine("Email veya şifre hatalı.");
            return View();
        }


        // REGISTER GET
        public IActionResult Register()
        {
            return View();
        }

        // REGISTER POST
        [HttpPost]
        public async Task<IActionResult> Register(string userName, string email, string password, string confirmPassword, IFormFile profileImage)
        {
            if (password != confirmPassword)
            {
                ModelState.AddModelError("", "Şifreler uyuşmuyor.");
                Console.WriteLine("Şifreler");
                return View();
            }

            if (await _userManager.FindByEmailAsync(email) != null)
            {
                ModelState.AddModelError("", "Bu email zaten kullanılıyor.");
                Console.WriteLine("kullanılıyor");
                return View();
            }

            string profileImagePath = null;

            if (profileImage != null && profileImage.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profiles");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(profileImage.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(fileStream);
                }

                profileImagePath = "/images/profiles/" + uniqueFileName;
            }

            var user = new User
            {
                UserName = userName,
                Email = email,
                Role = "Customer",
                ProfileImagePath = profileImagePath
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                // İstersen direkt login yapabilirsin
                await _signInManager.SignInAsync(user, isPersistent: false);

                return RedirectToAction("Index", "Home");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            Console.WriteLine("hiçbiri");

            return View();
        }

        // LOGOUT
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
