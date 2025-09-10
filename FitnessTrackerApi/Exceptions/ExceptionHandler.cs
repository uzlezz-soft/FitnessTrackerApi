using Microsoft.AspNetCore.Diagnostics;

namespace FitnessTrackerApi.Exceptions;

internal sealed class ExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, message) = exception switch
        {
            DuplicateUserException => (409, "User with this login or email is already registered"),
            InvalidUserNameException => (400, "Username is invalid"),
            InvalidRefreshTokenException => (400, "Refresh token is invalid"),
            InvalidCredentialsException => (400, "Username or password is invalid"),
            InvalidRegisterAttemptException => (400, "Bad email/username/password"),
            UserNotFoundException => (404, string.Empty),
            WorkoutNotFoundException => (404, "Workout not found"),
            UnknownImageFormatException => (400, "Unknown image format"),
            ImageTooLargeException => (400, "Image is too large"),
            ImageNotFoundException => (404, string.Empty),
            _ => (-1, string.Empty)
        };
        if (statusCode == -1) return false;

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(message, cancellationToken);
        return true;
    }
}