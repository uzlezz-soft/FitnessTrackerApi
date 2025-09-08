using FluentValidation;
using System.Security.Cryptography;

namespace FitnessTrackerApi.DTOs;

public record UserRegisterDto(string Email, string UserName, string Password);
public record UserLoginDto(string UserName, string Password);
public record RefreshTokenDto(string Token);

public record TokensDto(string RefreshToken, string AccessToken);

public static class UserCredentialsValidators
{
    public static IRuleBuilderOptions<T, string> UserName<T>(this IRuleBuilder<T, string> ruleBuilder)
        => ruleBuilder.NotEmpty().WithMessage("{PropertyName} is required")
            .Length(3, 32).WithMessage("{PropertyName} must be from 3 to 32 characters long")
            .Matches("^[a-zA-Z0-9_]+$").WithMessage("{PropertyName} can only contain letters, numbers and underscores");
    public static IRuleBuilderOptions<T, string> Password<T>(this IRuleBuilder<T, string> ruleBuilder)
        => ruleBuilder.NotEmpty().WithMessage("{PropertyName} is required")
            .MinimumLength(6).WithMessage("{PropertyName} must be at least 6 characters long")
            .MaximumLength(64).WithMessage("{PropertyName} must be at max 64 characters long");
}

public class UserRegisterValidator : AbstractValidator<UserRegisterDto>
{
    public UserRegisterValidator()
    {
        RuleFor(x => x.Email)
            .EmailAddress()
            .NotEmpty().WithMessage("{PropertyName} is required");
        RuleFor(x => x.UserName).UserName();
        RuleFor(x => x.Password).Password();
    }
}

public class UserLoginValidator : AbstractValidator<UserLoginDto>
{
    public UserLoginValidator()
    {
        RuleFor(x => x.UserName).UserName();
        RuleFor(x => x.Password).Password();
    }
}

public class RefreshTokenValidator : AbstractValidator<RefreshTokenDto>
{
    public RefreshTokenValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .Custom((x, context) =>
            {
                Span<byte> token = stackalloc byte[SHA256.HashSizeInBytes];
                if (!Convert.TryFromBase64String(x, token, out int bytesWritten) || bytesWritten < token.Length)
                    context.AddFailure("Token is malformed");
            });
    }
}