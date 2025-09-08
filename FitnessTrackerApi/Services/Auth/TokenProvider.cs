using FitnessTrackerApi.Configs;
using FitnessTrackerApi.DTOs;
using FitnessTrackerApi.Exceptions;
using FitnessTrackerApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace FitnessTrackerApi.Services.Auth;

public class TokenProvider(IOptions<AuthConfig> authOptions, AppDbContext context, ILogger<TokenProvider> logger) : ITokenProvider
{
    private readonly AuthConfig _config = authOptions.Value;
    private readonly SymmetricSecurityKey _key = new(Encoding.UTF8.GetBytes(authOptions.Value.Key));

    public async Task<Tokens> GenerateAccessTokenAsync(RefreshToken token, bool revokeRefreshToken = true)
    {
        Claim[] claims = [
            new(ClaimTypes.NameIdentifier, token.User.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        ];

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _config.Issuer,
            Audience = _config.Audience,
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddMinutes(_config.AccessLifetimeMinutes),
            SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var accessToken = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));

        if (revokeRefreshToken)
        {
            logger.LogInformation("Generated access token for user {UserId}, rotating refresh token", token.User.Id);
            token.Status = RefreshTokenStatus.Revoked;
            context.Update(token);
            var (_, refreshTokenString) = await GenerateRefreshTokenAsync(token.User);
            return new(refreshTokenString, accessToken);
        }

        return new(string.Empty, accessToken);
    }

    public async Task<(RefreshToken, string Token)> GenerateRefreshTokenAsync(User user)
    {
        var randomNumber = new byte[32];
        using var randomNumberGenerator = RandomNumberGenerator.Create();
        randomNumberGenerator.GetBytes(randomNumber);

        var validUntil = DateTime.UtcNow.AddHours(_config.RefreshLifetimeHours);

        var tokenString = Convert.ToBase64String(randomNumber);
        var hashed = HashToken(randomNumber);

        var token = new RefreshToken
        {
            Token = hashed,
            User = user,
            ValidUntil = validUntil,
            Status = RefreshTokenStatus.Valid
        };
        await context.RefreshTokens.AddAsync(token);
        await context.SaveChangesAsync();

        return (token, tokenString);
    }

    public async Task<RefreshToken> ValidateRefreshTokenAsync(string refreshToken)
    {
        string tokenHash;
        try
        {
            tokenHash = HashToken(Convert.FromBase64String(refreshToken));
        }
        catch (FormatException)
        {
            throw new InvalidRefreshTokenException();
        }

        var token = await context.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x =>
                x.Token == tokenHash
                && x.Status == RefreshTokenStatus.Valid
                && x.ValidUntil > DateTime.UtcNow);

        return token ?? throw new InvalidRefreshTokenException();
    }

    public async Task RevokeAsync(string refreshToken)
    {
        string tokenHash;
        try
        {
            tokenHash = HashToken(Convert.FromBase64String(refreshToken));
        }
        catch (FormatException)
        {
            throw new InvalidRefreshTokenException();
        }

        var token = await context.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x =>
                x.Token == tokenHash
                && x.Status == RefreshTokenStatus.Valid
                && x.ValidUntil > DateTime.UtcNow) ?? throw new InvalidRefreshTokenException();
        token.Status = RefreshTokenStatus.Revoked;
        await context.SaveChangesAsync();
    }

    public async Task<int> CleanupOldTokensAsync(CancellationToken stoppingToken)
    {
        var cutoffDate = DateTime.UtcNow - TimeSpan.FromDays(_config.RefreshTokenCleanupAfterDays);
        return await context.RefreshTokens
            .Where(x => x.Status != RefreshTokenStatus.Valid
                && x.ValidUntil <= cutoffDate)
            .ExecuteDeleteAsync(stoppingToken);
    }

    private static string HashToken(byte[] token) => Convert.ToBase64String(SHA256.HashData(token));
}
