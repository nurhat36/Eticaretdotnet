using ETicaret.Data; // AppDbContext'i kullanmak i�in
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Veritaban� ba�lant�s�n� ekle
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// MVC servisini ekle
builder.Services.AddControllersWithViews();

// Session i�lemleri i�in gerekli servisleri ekle
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session 30 dakika aktif kal�r
    options.Cookie.HttpOnly = true; // �erez sadece HTTP �zerinden eri�ilebilir
    options.Cookie.IsEssential = true; // Kullan�c� onay� gerekmeden �al���r
});

var app = builder.Build();

// Hata y�netimi ve HTTPS ayarlar�
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// UseAuthorization'dan �nce mutlaka Session'� aktive etmeliyiz
app.UseSession();

app.UseAuthorization();

// Varsay�lan route ayar�
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
