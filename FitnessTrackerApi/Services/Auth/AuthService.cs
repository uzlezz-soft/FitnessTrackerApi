using FitnessTrackerApi.DTOs;
using FitnessTrackerApi.Exceptions;
using FitnessTrackerApi.Models;
using Microsoft.AspNetCore.Identity;

namespace FitnessTrackerApi.Services.Auth;

public class AuthService(ILogger<AuthService> logger, UserManager<User> userManager, ITokenProvider tokenProvider) : IAuthService
{
    public async Task<Tokens> RegisterAsync(UserRegister request)
    {
        var user = new User { UserName = request.UserName, Email = request.Email };
        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var error = result.Errors.First();
            if (error.Code == userManager.ErrorDescriber.DuplicateUserName(request.UserName).Code
                || error.Code == userManager.ErrorDescriber.DuplicateEmail(request.Email).Code)
                throw new DuplicateUserException();
            if (error.Code == userManager.ErrorDescriber.InvalidUserName(request.UserName).Code)
                throw new InvalidUserNameException();
            throw new BadHttpRequestException("Invalid register attempt");
        }

        logger.LogInformation("User registered: {UserId} {UserName}", user.Id, user.UserName);
        return await GenerateTokens(user);
    }

    public async Task<Tokens> LoginAsync(UserLogin request)
    {
        var user = await userManager.FindByNameAsync(request.UserName)
            ?? throw new BadHttpRequestException("Username or password invalid");
        if (!(await userManager.CheckPasswordAsync(user, request.Password)))
            throw new BadHttpRequestException("Username or password invalid", 401);

        logger.LogInformation("User logged in: {UserId} {UserName}", user.Id, user.UserName);
        return await GenerateTokens(user);
    }

    private async Task<Tokens> GenerateTokens(User user)
    {
        var (refreshToken, refreshTokenString) = await tokenProvider.GenerateRefreshTokenAsync(user);

        var (_, accessToken) = await tokenProvider.GenerateAccessTokenAsync(refreshToken, false);
        return new Tokens(refreshTokenString, accessToken);
    }

    public async Task<Tokens> GenerateAccessToken(string refreshToken)
    {
        var token = await tokenProvider.ValidateRefreshToken(refreshToken);
        return await tokenProvider.GenerateAccessTokenAsync(token);
    }
}
