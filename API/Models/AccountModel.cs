using System.ComponentModel.DataAnnotations;
using TMS_IDP.Attributes;

namespace TMS_IDP.Models
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
