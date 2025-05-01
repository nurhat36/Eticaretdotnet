using ETicaret.Data;
using ETicaret.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using static System.Net.WebRequestMethods;

namespace ETicaret.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(AppDbContext context, UserManager<User> userManager, SignInManager<User> signInManager, ILogger<AccountController> logger)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }
        [HttpGet]
        public IActionResult GoogleLogin()
        {
            try
            {
                var redirectUrl = "https://localhost:5191/Account/GoogleResponse";
                _logger.LogInformation("Google authentication started. Redirect URL: {RedirectUrl}", redirectUrl);

                var properties = _signInManager.ConfigureExternalAuthenticationProperties(
                    "Google",
                    redirectUrl);

                return new ChallengeResult("Google", properties);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Google login initiation failed");
                TempData["ErrorMessage"] = "Google girişi başlatılamadı.";
                return RedirectToAction("Login");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GoogleResponse()
        {
            try
            {
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    _logger.LogWarning("External login info is null");
                    TempData["ErrorMessage"] = "Google bilgileri alınamadı.";
                    return RedirectToAction("Login");
                }

                // Mevcut kullanıcıyı kontrol et
                var signInResult = await _signInManager.ExternalLoginSignInAsync(
                    info.LoginProvider,
                    info.ProviderKey,
                    isPersistent: false);

                if (signInResult.Succeeded)
                {
                    _logger.LogInformation("User logged in with Google: {Email}", info.Principal.FindFirstValue(ClaimTypes.Email));
                    return RedirectToAction("Index", "Home");
                }

                // Yeni kullanıcı oluştur
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogError("Google account email not found");
                    TempData["ErrorMessage"] = "Google hesabınızda email bulunamadı.";
                    return RedirectToAction("Login");
                }

                var user = new User
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true // Google ile giriş yapanların email'i doğrulanmış sayılır
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    _logger.LogError("User creation failed: {Errors}", errors);
                    TempData["ErrorMessage"] = "Kullanıcı oluşturulamadı: " + errors;
                    return RedirectToAction("Register");
                }

                // Google bilgilerini ekle ve giriş yap
                await _userManager.AddLoginAsync(user, info);
                await _signInManager.SignInAsync(user, isPersistent: false);

                _logger.LogInformation("New user registered with Google: {Email}", email);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Google authentication failed");
                TempData["ErrorMessage"] = "Google girişi sırasında hata oluştu.";
                return RedirectToAction("Login");
            }
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
            Console.WriteLine(email);
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
                    HttpContext.Session.SetString("ProfileImagePath", user.ProfileImagePath);

                    return RedirectToAction("Privacy", "Home");
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
                Console.WriteLine("Şifreler uyuşmuyor.");
                return View();
            }

            if (await _userManager.FindByEmailAsync(email) != null)
            {
                Console.WriteLine( "Bu email zaten kullanılıyor.");
                return View();
            }
            if (await _userManager.FindByNameAsync(userName) != null)
            {
                ModelState.AddModelError("", "Bu kullanıcı adı zaten kullanılıyor.");
                return View();
            }


            string profileImagePath = null;

            if (profileImage != null && profileImage.Length > 0)
            {
                Console.WriteLine("resim");
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
                Role = "Customer",  // Varsayılan olarak Customer rolü atandı
                ProfileImagePath = profileImagePath
            }; 
            Console.WriteLine("user " + user.Email+"normal email "+user.NormalizedEmail + "hash code " + user.GetHashCode());

            var result = await _userManager.CreateAsync(user, password);
            Console.WriteLine("Olsana  "+ result+"  "+result.Succeeded);

            if (result.Succeeded)
            {
                // Eğer kullanıcı başarılı bir şekilde oluşturulmuşsa, rol ataması yapılır
                var roleResult = await _userManager.AddToRoleAsync(user, "Customer");
                if (!roleResult.Succeeded)
                {
                    foreach (var error in roleResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }

                // İsterseniz direkt olarak giriş yapabilirsiniz
                await _signInManager.SignInAsync(user, isPersistent: false);

                return RedirectToAction("Index", "Home");
            }
            else
            {
                // Hata mesajlarını model state'e ekle
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            Console.WriteLine("Olmadı");
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
        public IActionResult MyProfil()
        {
            // Kullanıcı verilerini model ile gönderebilirsin
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            // Şifre doğrulama
            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Yeni şifreler eşleşmiyor.");
                return View();
            }

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

            if (result.Succeeded)
            {
                // Başarılı işlem
                return RedirectToAction("MyProfil");
            }

            // Hata durumunda
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(string fullName, IFormFile profileImage)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            user.UserName = fullName;
            

            // Profil resmi güncelleniyorsa, resmi kaydet
            if (profileImage != null)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", profileImage.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(stream);
                }
                user.ProfileImagePath = "/images/" + profileImage.FileName;
            }

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                // Başarılı işlem
                return RedirectToAction("MyProfil");
            }
            Console.WriteLine("olmadı");
            // Hata durumunda
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(user);
        }

    }
}
