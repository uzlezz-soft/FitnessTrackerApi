using FitnessTrackerApi.DTOs;
using FitnessTrackerApi.Exceptions;
using FitnessTrackerApi.Services.Auth;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FitnessTrackerApi.Endpoints;

public static class UserEndpoints
{
    public static void RegisterUserEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/users", Register)
            .WithName("Register")
            .WithOpenApi();
        builder.MapPost("/auth/login", Login)
            .WithName("Login")
            .WithOpenApi();
        builder.MapPost("/auth/refresh", GetAccessToken)
            .WithName("RefreshToken")
            .WithOpenApi();
        builder.MapPost("/auth/logout", LogOut)
            .WithName("LogOut")
            .WithOpenApi();
    }

    private static async Task<Results<Created<TokensDto>, Conflict, BadRequest, ValidationProblem>> Register(
        IAuthService authService, [FromBody] UserRegisterDto request, IValidator<UserRegisterDto> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return TypedResults.ValidationProblem(validationResult.ToDictionary());

        try
        {
            var tokens = await authService.RegisterAsync(request);
            return TypedResults.Created("/auth/login", tokens);
        }
        catch (DuplicateUserException)
        {
            return TypedResults.Conflict();
        }
        catch (InvalidUserNameException)
        {
            return TypedResults.BadRequest();
        }
    }

    private static async Task<Results<Ok<TokensDto>, BadRequest, UnauthorizedHttpResult, ValidationProblem>> Login(
        IAuthService authService, [FromBody] UserLoginDto request, HttpContext context, IValidator<UserLoginDto> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return TypedResults.ValidationProblem(validationResult.ToDictionary());

        try
        {
            var tokens = await authService.LoginAsync(request);
            return TypedResults.Ok(tokens);
        }
        catch (InvalidRefreshTokenException)
        {
            return TypedResults.Unauthorized();
        }
    }

    private static async Task<Results<Ok<TokensDto>, UnauthorizedHttpResult, ValidationProblem>> GetAccessToken(
        IAuthService authService, [FromBody] RefreshTokenDto refreshToken, IValidator<RefreshTokenDto> validator)
    {
        var validationResult = await validator.ValidateAsync(refreshToken);
        if (!validationResult.IsValid)
            return TypedResults.ValidationProblem(validationResult.ToDictionary());

        try
        {
            var token = await authService.GenerateAccessToken(refreshToken.Token);
            return TypedResults.Ok(token);
        }
        catch (InvalidRefreshTokenException)
        {
            return TypedResults.Unauthorized();
        }
    }

    private static async Task<Results<Ok, UnauthorizedHttpResult, ValidationProblem>> LogOut(
        IAuthService authService, [FromBody] RefreshTokenDto refreshToken, IValidator<RefreshTokenDto> validator)
    {
        var validationResult = await validator.ValidateAsync(refreshToken);
        if (!validationResult.IsValid)
            return TypedResults.ValidationProblem(validationResult.ToDictionary());

        try
        {
            await authService.LogOutAsync(refreshToken.Token);
            return TypedResults.Ok();
        }
        catch (InvalidRefreshTokenException)
        {
            return TypedResults.Unauthorized();
        }
    }
}
