using Microsoft.EntityFrameworkCore;
using ETicaret.Models;  // Modeli kullanabilmek için gerekli
using BCrypt.Net;

namespace ETicaret.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSet'ler burada tanýmlanýr
        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    UserName = "admin",
                    Email = "admin@eticaret.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), // Þifreyi sabitledik
                    Role = "Admin",
                    CreatedAt = new DateTime(2025, 4, 26)
                }
            );
        }


    }
}

