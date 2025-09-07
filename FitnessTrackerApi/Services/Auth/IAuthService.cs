using FitnessTrackerApi.DTOs;

namespace FitnessTrackerApi.Services.Auth;

public interface IAuthService
{
    Task<TokensDto> RegisterAsync(UserRegisterDto request);
    Task<TokensDto> LoginAsync(UserLoginDto login);
    Task<TokensDto> GenerateAccessToken(string refreshToken);
    Task LogOutAsync(string refreshToken);
}
