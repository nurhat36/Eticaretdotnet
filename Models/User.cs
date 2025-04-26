using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ETicaret.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string UserName { get; set; }

        [Required]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }  // Şifreyi hashleyerek tutacağız, plaintext saklamıyoruz!

        [Required]
        [MaxLength(50)]
        public string Role { get; set; }  // Admin, Customer, Seller gibi

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
