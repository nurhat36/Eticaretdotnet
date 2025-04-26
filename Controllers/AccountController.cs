using ETicaret.Data;
using ETicaret.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;

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
            Console.WriteLine("hash  " + user.PasswordHash + "  pasvord  " + CreatePasswordHash(password));

            if (user != null && VerifyPasswordHash(password, user.PasswordHash))
            {
                // Başarılı giriş
                // Kullanıcıyı Session'a alıyoruz
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("UserName", user.UserName);
                HttpContext.Session.SetString("UserRole", user.Role);

                // Eğer salt geçerli değilse hash'i güncelle
                if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    user.PasswordHash = CreatePasswordHash(password);
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();
                }
                Console.WriteLine("hash  "+user.PasswordHash);

                // Giriş başarılıysa anasayfaya yönlendiriyoruz
                return RedirectToAction("Index", "Home");
            }
            else
            {
                Console.WriteLine("hash girmiyor " + user.PasswordHash+"  pasvord  "+CreatePasswordHash(password));
                // Email ya da şifre hatalıysa hata mesajı ekliyoruz
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
        public async Task<IActionResult> Register(string userName, string email, string password, string confirmPassword)
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

            var user = new User
            {
                UserName = userName,
                Email = email,
                PasswordHash = CreatePasswordHash(password),
                Role = "Customer" // Kayıt olan kullanıcılar default "Customer" olacak
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }

        // LOGOUT
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // Şifreyi hashleyen fonksiyon
        private string CreatePasswordHash(string password)
        {
            // BCrypt otomatik olarak salt oluşturur, ek bir salt parametresine gerek yok
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Şifre doğrulayan fonksiyon
        private bool VerifyPasswordHash(string password, string storedHash)
        {
            // Şifre doğrulama işlemi
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }

    }
}
