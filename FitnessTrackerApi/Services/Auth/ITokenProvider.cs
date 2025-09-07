using FitnessTrackerApi.DTOs;
using FitnessTrackerApi.Models;

namespace FitnessTrackerApi.Services.Auth;

public interface ITokenProvider
{
    Task<(RefreshToken, string Token)> GenerateRefreshTokenAsync(User user);
    Task<Tokens> GenerateAccessTokenAsync(RefreshToken token, bool revokeRefreshToken = true);

    Task<RefreshToken> ValidateRefreshToken(string refreshToken);
}
