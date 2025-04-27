using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace ETicaret.Models
{
    public class User : IdentityUser
    {
        // Bu alanı IdentityUser zaten sağladığı için silinebilir:
        // public int Id { get; set; }

        // Bu özellikler zaten IdentityUser'da mevcut, dolayısıyla tekrar tanımlamaya gerek yok:
        // public string UserName { get; set; }
        // public string Email { get; set; }
        // public string PasswordHash { get; set; }

        [Required]
        [MaxLength(50)]
        public string Role { get; set; }  // Admin, Customer, Seller gibi

        public string? ProfileImagePath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
