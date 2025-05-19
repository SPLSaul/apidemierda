using System;
using System.ComponentModel.DataAnnotations;
namespace apiDDKMA.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required, MinLength(3)]
        public string Username { get; set; }
        [Required]
        public string PasswordHash { get; set; }
        public string Rol { get; set; } = "cliente"; // Valor por defecto
        public DateTime CreatedDT { get; set; } = DateTime.UtcNow;
        public string ProfilePicture { get; set; }
        public string Telefono { get; set; }
    }

    public class LoginRequest
    {
        [Required]
        public string UsernameOrEmail { get; set; }
        [Required]
        public string Password { get; set; }
    }

    public class RegisterRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, MinLength(3)]
        public string Username { get; set; }

        [Required, MinLength(6)]
        public string Password { get; set; }

        public string ProfilePicture { get; set; }

        public string Telefono { get; set; }
    }
}