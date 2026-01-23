using System.ComponentModel.DataAnnotations;

namespace NG.ControlCenter.WebSite.Models
{
    /// <summary>
    /// Base model with common validation attributes
    /// </summary>
    public abstract class BaseValidationModel
    {
        [Required(ErrorMessage = "This field is required")]
        [StringLength(255, ErrorMessage = "Maximum length is 255 characters")]
        public string? Name { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
    }

    /// <summary>
    /// Example: Login model with validation
    /// </summary>
    public class LoginModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, 
            ErrorMessage = "Password must be between 6 and 100 characters")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    /// <summary>
    /// Example: Registration model with validation
    /// </summary>
    public class RegisterModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name is too long")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name is too long")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8, 
            ErrorMessage = "Password must be between 8 and 100 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Password must contain uppercase, lowercase, number, and special character")]
        public string Password { get; set; } = string.Empty;

        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "You must accept the terms")]
        public bool AcceptTerms { get; set; }
    }

    /// <summary>
    /// Example: Organization model with validation
    /// </summary>
    public class OrganizationModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Organization name is required")]
        [StringLength(100, MinimumLength = 3,
            ErrorMessage = "Organization name must be between 3 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description is too long")]
        public string? Description { get; set; }

        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Please enter a valid phone number")]
        public string? PhoneNumber { get; set; }

        [Url(ErrorMessage = "Please enter a valid URL")]
        public string? Website { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid option")]
        public int? CountryId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
