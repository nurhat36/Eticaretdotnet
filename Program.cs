using ETicaret.Data;
using ETicaret.Models; // ApplicationUser modelini kullanmak i�in
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1) Veritaban� ba�lant�s�n� ekle
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2) Identity ayarlar� ve �ifre politikalar�
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // �ifre ayarlar�
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // Giri� yaparken email onay� zorunlu mu?
    options.SignIn.RequireConfirmedAccount = false;

    // Hesap kilitleme ayarlar�
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // 5 dakika kilitli kal�r
    options.Lockout.MaxFailedAccessAttempts = 5; // 5 yanl�� giri�ten sonra kilitlenir
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders(); // �ifre resetleme, email onay� gibi token i�lemleri i�in

// 3) Identity Cookie ayarlar� (Giri�-��k�� Yollar�)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // 1 saat boyunca oturum aktif
    options.SlidingExpiration = true; // S�re boyunca kullan�c� aktifse s�re uzar
});

// 4) MVC Controller + Views
builder.Services.AddControllersWithViews();

// 5) Session ayarlar�
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

app.UseSession();           // Session kullan�lacaksa �nce bunu yaz
app.UseAuthentication();    // Kimlik do�rulama
app.UseAuthorization();     // Yetkilendirme

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
