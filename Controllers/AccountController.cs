using ETicaret.Data;
using ETicaret.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace ETicaret.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // LOGIN GET
        public IActionResult Login()
        {
            return View();
        }

        // LOGIN POST
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Email ve şifre gereklidir.");
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user != null && VerifyPasswordHash(password, user.PasswordHash))
            {
                // Claims oluştur
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // Kullanıcıyı oturum aç
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity)
                );

                // İstersen Session'a da yaz
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("UserName", user.UserName);
                HttpContext.Session.SetString("UserRole", user.Role);

                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("", "Email veya şifre hatalı.");
                return View();
            }
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
                return View();
            }

            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                ModelState.AddModelError("", "Bu email zaten kullanılıyor.");
                return View();
            }

            string profileImagePath = null;

            if (profileImage != null && profileImage.Length > 0)
            {
                // Kullanıcının fotoğrafını wwwroot/images/profiles klasörüne kaydedelim
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profiles");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Dosya adını benzersiz yapalım
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(profileImage.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(fileStream);
                }

                // Veritabanına kaydedeceğimiz yol
                profileImagePath = "/images/profiles/" + uniqueFileName;
            }

            var user = new User
            {
                UserName = userName,
                Email = email,
                PasswordHash = CreatePasswordHash(password),
                Role = "Customer", // Varsayılan kullanıcı rolü
                ProfileImagePath = profileImagePath
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }

        // LOGOUT
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        private string CreatePasswordHash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPasswordHash(string password, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }
    }
}
