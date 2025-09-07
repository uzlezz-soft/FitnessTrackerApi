using FitnessTrackerApi.DTOs;

namespace FitnessTrackerApi.Services.Auth;

public interface IAuthService
{
    Task<Tokens> RegisterAsync(UserRegister request);
    Task<Tokens> LoginAsync(UserLogin login);
    Task<Tokens> GenerateAccessToken(string refreshToken);
}
