using System.ComponentModel.DataAnnotations;

namespace TMS_IDP.Models.ViewModel
{
    public class RegistrationViewModel
    {

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Email provided is in the incorrect format")]
        [StringLength(100, ErrorMessage = "Email should not exceed 100 characters.")]
        [DataType(DataType.EmailAddress)]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [DataType(DataType.Password)]
        public required string ConfirmPassword { get; set; }
        
        [DataType(DataType.Url)]
        public string? ReturnUrl { get; set; }
    }
}

