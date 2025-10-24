using FluentValidation;
using SoNice.Application.DTOs;

namespace SoNice.Application.Validators;

/// <summary>
/// Validator for user registration
/// </summary>
public class RegisterUserDtoValidator : AbstractValidator<RegisterUserDto>
{
    public RegisterUserDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters");

        RuleFor(x => x.FullName)
            .MaximumLength(255).WithMessage("Full name cannot exceed 255 characters")
            .When(x => !string.IsNullOrEmpty(x.FullName));

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(10).WithMessage("Phone number cannot exceed 10 characters")
            .Matches(@"^\d+$").WithMessage("Phone number must contain only digits")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}

/// <summary>
/// Validator for user login
/// </summary>
public class LoginUserDtoValidator : AbstractValidator<LoginUserDto>
{
    public LoginUserDtoValidator()
    {
        RuleFor(x => x.Identifier)
            .NotEmpty().WithMessage("Email or phone number is required");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}

/// <summary>
/// Validator for update user
/// </summary>
public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserDtoValidator()
    {
        RuleFor(x => x.FullName)
            .MaximumLength(255).WithMessage("Full name cannot exceed 255 characters")
            .When(x => !string.IsNullOrEmpty(x.FullName));

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(10).WithMessage("Phone number cannot exceed 10 characters")
            .Matches(@"^\d+$").WithMessage("Phone number must contain only digits")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}

/// <summary>
/// Validator for change password
/// </summary>
public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordDtoValidator()
    {
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required")
            .Equal(x => x.Password).WithMessage("Password and confirm password do not match");
    }
}

/// <summary>
/// Validator for forgot password
/// </summary>
public class ForgotPasswordDtoValidator : AbstractValidator<ForgotPasswordDto>
{
    public ForgotPasswordDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");
    }
}

/// <summary>
/// Validator for verify OTP
/// </summary>
public class VerifyOtpDtoValidator : AbstractValidator<VerifyOtpDto>
{
    public VerifyOtpDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Otp)
            .NotEmpty().WithMessage("OTP is required")
            .InclusiveBetween(100000, 999999).WithMessage("OTP must be a 6-digit number");
    }
}

/// <summary>
/// Validator for reset password
/// </summary>
public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
{
    public ResetPasswordDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters");
    }
}
