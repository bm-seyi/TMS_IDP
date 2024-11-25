using System.ComponentModel.DataAnnotations;
using TMS_API.Attributes;

namespace TMS_API.Models
{
    public class Login
    {
        [Required]
        [EmailAddress(ErrorMessage = "Email address provided is in the incorrect format")]
        [NoWhiteSpaceOrEmpty]
        [DataType(DataType.EmailAddress)]
        public required string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [NoWhiteSpaceOrEmpty]
        public required string Password { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
