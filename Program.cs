using ETicaret.Data;
using ETicaret.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1) Veritabaný baðlantýsýný ekle
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddLogging(loggingBuilder => {
    loggingBuilder.AddConsole();
    loggingBuilder.AddDebug();
});

// 2) Identity ayarlarý ve þifre politikalarý
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // Þifre ayarlarý
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // Giriþ yaparken email onayý zorunlu mu?
    options.SignIn.RequireConfirmedAccount = false;

    // Hesap kilitleme ayarlarý
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // 5 dakika kilitli kalýr
    options.Lockout.MaxFailedAccessAttempts = 5; // 5 yanlýþ giriþten sonra kilitlenir
    options.Lockout.AllowedForNewUsers = true;
})

.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// 3) Identity Cookie ayarlarý (Giriþ-Çýkýþ Yollarý)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // 1 saat boyunca oturum aktif
    options.SlidingExpiration = true; // Süre boyunca kullanýcý aktifse süre uzar
});
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    options.CallbackPath = "/Account/GoogleResponse"; // Google paneline bu URL'yi eklemelisin

    options.SaveTokens = true;

    // Additional scopes if needed
    options.Scope.Add("profile");
    options.Scope.Add("email");
});

// 4) MVC Controller + Views
builder.Services.AddControllersWithViews();

// 5) Session ayarlarý
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // 30 dk oturum
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// 6) Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();           // Session kullanýlacaksa önce bunu yaz
app.UseAuthentication();    // Kimlik doðrulama
app.UseAuthorization();     // Yetkilendirme

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 7) SeedData çalýþtýrma
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var userManager = services.GetRequiredService<UserManager<User>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    // Admin kullanýcýsýný oluþturma iþlemi
    await SeedData.Initialize(services, userManager, roleManager);
}

app.Run();
