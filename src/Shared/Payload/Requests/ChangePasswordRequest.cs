using System.ComponentModel.DataAnnotations;

namespace Shared.Payload.Requests;

public class ChangePasswordRequest
{
    public string OldPassword { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Password is required.")]
    [MinLength(8, ErrorMessage = "The password must be more than 8 characters long")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$", 
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
    public string NewPassword { get; set; } = string.Empty;
    
    [Compare("NewPassword", ErrorMessage = "Passwords are not the same.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}