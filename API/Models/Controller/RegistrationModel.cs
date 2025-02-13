using System.ComponentModel.DataAnnotations;

namespace TMS_IDP.Models.Controllers
{
    public class RegModel
    {
        
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Email provided is in the incorrect format")]
        [StringLength(100, ErrorMessage = "Email should not exceed 100 characters.")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public required string Password  { get; set; }

        [Required(ErrorMessage = "ClientId is required")]
        [StringLength(100, ErrorMessage = "ClientId should not exceed 100 characters.")]
        public required string ClientId { get; set; }

        [Required(ErrorMessage = "ClientSecret is required")]
        [StringLength(100, ErrorMessage = "ClientSecret should not exceed 100 characters.")]
        public required string ClientSecret { get; set; } 

    }
}

