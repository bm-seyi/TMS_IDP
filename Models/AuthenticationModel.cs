using System.ComponentModel.DataAnnotations;
using TMS_API.Attributes;

namespace TMS_API.Models
{
    public class AuthModel
    {
        [Required]
        [EmailAddress(ErrorMessage = "Email address provided is in the incorrect format")]
        [StringLength(100, ErrorMessage = "Email should not exceed 100 characters.")]
        [NoWhiteSpaceOrEmpty]
        public required string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Password should not exceed 100 characters.")]
        [NoWhiteSpaceOrEmpty]
        public required string Password { get; set; }

        [Required(ErrorMessage = "ClientId is required")]
        [StringLength(100, ErrorMessage = "ClientId should not exceed 100 characters.")]
        [NoWhiteSpaceOrEmpty]
        public required string ClientId { get; set; }

        [Required(ErrorMessage = "ClientSecret is required")]
        [StringLength(100, ErrorMessage = "ClientSecret should not exceed 100 characters.")]
        [NoWhiteSpaceOrEmpty]
        public required string ClientSecret { get; set; }
    }
}

