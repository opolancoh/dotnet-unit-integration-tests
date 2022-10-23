using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace EmployeeWebApiTesting.Web.DTOs;

public class EmployeeCreateUpdateDto : IValidatableObject
{
    [Required] public string? Name { get; set; }
    [Required] public int? Age { get; set; }
    [Required] public DateTime? HireDate { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Regex.IsMatch(Name,  "^[a-zA-Z ']*$"))
        {
            yield return new ValidationResult(
                $"The {nameof(Name)} field is invalid.", new[] {nameof(Name)});
        }

        if (Age < 18)
        {
            yield return new ValidationResult(
                $"The {nameof(Age)} field is invalid.", new[] {nameof(Age)});
        }
        
        if (HireDate == default(DateTime))
        {
            yield return new ValidationResult(
                $"The {nameof(HireDate)} field is invalid.", new[] {nameof(HireDate)});
        }
    }
}