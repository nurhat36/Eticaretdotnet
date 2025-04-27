using Microsoft.AspNetCore.Identity;

namespace ETicaret.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Kullanıcının profil resmi yolu
        public string? ProfileImagePath { get; set; }

        // Diğer ihtiyaca göre özelleştirilebilecek alanlar
        // Örneğin:
        // public string FullName { get; set; }
        // public DateTime DateOfBirth { get; set; }
    }
}
