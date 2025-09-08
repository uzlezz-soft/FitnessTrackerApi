using FitnessTrackerApi.DTOs;
using FitnessTrackerApi.Models;

namespace FitnessTrackerApi.Services.Auth;

public interface ITokenProvider
{
    Task<(RefreshToken, string Token)> GenerateRefreshTokenAsync(User user);
    Task<Tokens> GenerateAccessTokenAsync(RefreshToken token, bool revokeRefreshToken = true);

    Task<RefreshToken> ValidateRefreshTokenAsync(string refreshToken);
    Task RevokeAsync(string refreshToken);

    Task<int> CleanupOldTokensAsync(CancellationToken stoppingToken);
}
