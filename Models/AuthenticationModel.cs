using System.ComponentModel.DataAnnotations;

namespace TMS_API.Models
{
    public class AuthModel
    {

        [Required]
        [EmailAddress(ErrorMessage = "Email address provided is in the incorrect format")]
        [StringLength(100, ErrorMessage = "Email should not exceed 100 characters.")]
        public required string Email { get; set; }

        [StringLength(100, ErrorMessage = "Password should not exceed 100 characters.")]
        public string? Password { get; set; } 

        [StringLength(512, ErrorMessage = "Refresh Token should not exceed 512 characters.")]
        public string? RefreshToken { get; set; } 

        [Required(ErrorMessage = "ClientId is required")]
        [StringLength(100, ErrorMessage = "ClientId should not exceed 100 characters.")]
        public required string ClientId { get; set; }

        [Required(ErrorMessage = "ClientSecret is required")]
        [StringLength(100, ErrorMessage = "ClientSecret should not exceed 100 characters.")]
        public required string ClientSecret { get; set; } 

        [Required(ErrorMessage = "GrantType is required")]
        [StringLength(100, ErrorMessage = "GrantType should not exceed 100 characters.")]
        public required string GrantType { get; set; }
    }
}

