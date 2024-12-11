using System.ComponentModel.DataAnnotations;

namespace TMS_IDP.Attributes
{
    public class NoWhiteSpaceOrEmptyAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
           if (value is string str && string.IsNullOrWhiteSpace(str))
           {
                return new ValidationResult($"{validationContext.DisplayName} cannot be empty or whitespace");
           }
           return ValidationResult.Success;
        }
    } 
}