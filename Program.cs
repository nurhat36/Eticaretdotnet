using ETicaret.Data; // AppDbContext'i kullanmak için
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Veritabaný baðlantýsýný ekle
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// MVC servisini ekle
builder.Services.AddControllersWithViews();

// Session iþlemleri için gerekli servisleri ekle
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session 30 dakika aktif kalýr
    options.Cookie.HttpOnly = true; // Çerez sadece HTTP üzerinden eriþilebilir
    options.Cookie.IsEssential = true; // Kullanýcý onayý gerekmeden çalýþýr
});

var app = builder.Build();

// Hata yönetimi ve HTTPS ayarlarý
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// UseAuthorization'dan önce mutlaka Session'ý aktive etmeliyiz
app.UseSession();

app.UseAuthorization();

// Varsayýlan route ayarý
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
