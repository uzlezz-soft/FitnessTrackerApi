using FitnessTrackerApi.DTOs;
using FitnessTrackerApi.Exceptions;
using FitnessTrackerApi.Services.Auth;
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
        builder.MapPost("/sessions", Login)
            .WithName("Login")
            .WithOpenApi();
        builder.MapPut("/sessions/{refreshToken}", GetAccessToken)
            .WithName("RefreshToken")
            .WithOpenApi();
        builder.MapDelete("/sessions/{refreshToken}", LogOut)
            .WithName("LogOut")
            .WithOpenApi();
    }

    private static async Task<Results<Created<Tokens>, Conflict, BadRequest>> Register(
        IAuthService authService, [FromBody] UserRegister request)
    {
        try
        {
            var tokens = await authService.RegisterAsync(request);
            return TypedResults.Created("/sessions", tokens);
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

    private static async Task<Results<Ok<Tokens>, BadRequest, UnauthorizedHttpResult>> Login(
        IAuthService authService, [FromBody] UserLogin request, HttpContext context)
    {
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

    private static async Task<Results<Ok<Tokens>, UnauthorizedHttpResult>> GetAccessToken(
        IAuthService authService, [FromRoute] string refreshToken)
    {
        try
        {
            var token = await authService.GenerateAccessToken(refreshToken);
            return TypedResults.Ok(token);
        }
        catch (InvalidRefreshTokenException)
        {
            return TypedResults.Unauthorized();
        }
    }

    private static async Task<Results<Ok, UnauthorizedHttpResult>> LogOut(
        IAuthService authService, [FromRoute] string refreshToken)
    {
        try
        {
            await authService.LogOutAsync(refreshToken);
            return TypedResults.Ok();
        }
        catch (InvalidRefreshTokenException)
        {
            return TypedResults.Unauthorized();
        }
    }
}
