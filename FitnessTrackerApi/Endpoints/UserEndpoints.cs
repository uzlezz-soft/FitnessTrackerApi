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
            .WithOpenApi()
            .RequireRateLimiting("create-or-refresh");
        builder.MapPost("/sessions", Login)
            .WithName("Login")
            .WithOpenApi()
            .RequireRateLimiting("create-or-refresh");
        builder.MapPut("/sessions/{refreshToken}", GetAccessToken)
            .WithName("RefreshToken")
            .WithOpenApi()
            .RequireRateLimiting("create-or-refresh");
        builder.MapDelete("/sessions/{refreshToken}", LogOut)
            .WithName("LogOut")
            .WithOpenApi();
    }

    private static async Task<Results<Created<TokensDto>, Conflict, BadRequest, ValidationProblem>> Register(
        IAuthService authService, [FromBody] UserRegisterDto request, IValidator<UserRegisterDto> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return TypedResults.ValidationProblem(validationResult.ToDictionary());

        var tokens = await authService.RegisterAsync(request);
        return TypedResults.Created("/sessions", tokens);
    }

    private static async Task<Results<Ok<TokensDto>, BadRequest, UnauthorizedHttpResult, ValidationProblem>> Login(
        IAuthService authService, [FromBody] UserLoginDto request, HttpContext context, IValidator<UserLoginDto> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return TypedResults.ValidationProblem(validationResult.ToDictionary());

        var tokens = await authService.LoginAsync(request);
        return TypedResults.Ok(tokens);
    }

    private static async Task<Results<Ok<TokensDto>, UnauthorizedHttpResult>> GetAccessToken(
        IAuthService authService, [FromRoute] string refreshToken)
    {
        var token = await authService.GenerateAccessToken(refreshToken);
        return TypedResults.Ok(token);
    }

    private static async Task<Results<Ok, UnauthorizedHttpResult>> LogOut(
        IAuthService authService, [FromRoute] string refreshToken)
    {
        await authService.LogOutAsync(refreshToken);
        return TypedResults.Ok();
    }
}
