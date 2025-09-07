using FluentValidation;

namespace FitnessTrackerApi.DTOs;

public record UserRegister(string Email, string UserName, string Password);
public record UserLogin(string UserName, string Password);

public record Tokens(string RefreshToken, string AccessToken);

public static class UserCredentialsValidators
{
    public static IRuleBuilderOptions<T, string> UserName<T>(this IRuleBuilder<T, string> ruleBuilder)
        => ruleBuilder.NotEmpty().WithMessage("Username is required")
            .Length(3, 32).WithMessage("Username must be from 3 to 32 characters long")
            .Matches("^[a-zA-Z0-9_]+$").WithMessage("Username can only contain letters, numbers and underscores");
    public static IRuleBuilderOptions<T, string> Password<T>(this IRuleBuilder<T, string> ruleBuilder)
        => ruleBuilder.NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long")
            .MinimumLength(64).WithMessage("Password must be at max 64 characters long");
}

public class UserRegisterValidator : AbstractValidator<UserRegister>
{
    public UserRegisterValidator()
    {
        RuleFor(x => x.Email)
            .EmailAddress().NotEmpty().WithMessage("Email is required");
        RuleFor(x => x.UserName).UserName();
        RuleFor(x => x.Password).Password();
    }
}

public class UserLoginValidator : AbstractValidator<UserLogin>
{
    public UserLoginValidator()
    {
        RuleFor(x => x.UserName).UserName();
        RuleFor(x => x.Password).Password();
    }
}