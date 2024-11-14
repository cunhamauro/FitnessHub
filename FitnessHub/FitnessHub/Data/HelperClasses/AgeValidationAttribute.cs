using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Data.HelperClasses
{
    public class AgeValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime birthDate)
            {
                var age = DateTime.Now.Year - birthDate.Year;

                // Adjust age if the birthday has not occurred yet this year
                if (DateTime.Now < birthDate.AddYears(age))
                {
                    age--;
                }

                if (age < 18)
                {
                    return new ValidationResult("User must be at least 18 years old.");
                }
            }

            return ValidationResult.Success;
        }
    }
}
