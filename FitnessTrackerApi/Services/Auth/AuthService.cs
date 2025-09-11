using FitnessTrackerApi.Configs;
using FitnessTrackerApi.DTOs;
using FitnessTrackerApi.Exceptions;
using FitnessTrackerApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace FitnessTrackerApi.Services.Auth;

public class AuthService(
    ILogger<AuthService> logger,
    UserManager<User> userManager,
    ITokenProvider tokenProvider) : IAuthService
{
    public async Task<TokensDto> RegisterAsync(UserRegisterDto request)
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
            throw new InvalidRegisterAttemptException();
        }

        logger.LogInformation("User registered: {UserId} {UserName}", user.Id, user.UserName);
        
        return await GenerateTokens(user);
    }

    public async Task<TokensDto> LoginAsync(UserLoginDto request)
    {
        var user = await userManager.FindByNameAsync(request.UserName)
            ?? throw new InvalidCredentialsException();
        if (!await userManager.CheckPasswordAsync(user, request.Password))
            throw new InvalidCredentialsException();

        logger.LogInformation("User logged in: {UserId} {UserName}", user.Id, user.UserName);
        return await GenerateTokens(user);
    }

    private async Task<TokensDto> GenerateTokens(User user)
    {
        var (refreshToken, refreshTokenString) = await tokenProvider.GenerateRefreshTokenAsync(user);

        var (_, accessToken) = await tokenProvider.GenerateAccessTokenAsync(refreshToken, false);

        return new TokensDto(refreshTokenString, accessToken);
    }

    public async Task<TokensDto> GenerateAccessToken(string refreshToken)
    {
        var token = await tokenProvider.ValidateRefreshTokenAsync(refreshToken);
        return await tokenProvider.GenerateAccessTokenAsync(token);
    }

    public async Task LogOutAsync(string refreshToken)
    {
        await tokenProvider.RevokeAsync(refreshToken);
    }
}
