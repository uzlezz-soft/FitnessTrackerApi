using FitnessTrackerApi.Configs;
using FitnessTrackerApi.Exceptions;
using FitnessTrackerApi.Models;
using FitnessTrackerApi.Services;
using FitnessTrackerApi.Services.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace FitnessTrackerApi.Test;

public class TokenProviderTests
{
    private readonly TokenProvider _tokenProvider;
    private readonly AppDbContext _context;

    public TokenProviderTests()
    {
        var contextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(contextOptions);

        var authConfig = new AuthConfig
        {
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            Key = "SuperSecretKey1234567890_=+-!@#$%^",
            AccessLifetimeMinutes = 5,
            RefreshLifetimeHours = 1
        };

        var options = Options.Create(authConfig);
        var logger = Mock.Of<ILogger<TokenProvider>>();
        _tokenProvider = new TokenProvider(options, _context, logger);
    }

    [Fact]
    public async Task GenerateRefreshToken_ShouldCreateAndSaveToken()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid().ToString(), UserName = "test", Email = "test@example.com" };

        // Act
        var (refreshToken, tokenString) = await _tokenProvider.GenerateRefreshTokenAsync(user);

        // Assert
        Assert.NotNull(refreshToken);
        Assert.Equal(RefreshTokenStatus.Valid, refreshToken.Status);
        Assert.NotNull(tokenString);
        Assert.Single(_context.RefreshTokens);
    }

    [Fact]
    public async Task ValidateRefreshToken_ShouldReturnTokenWhenValid()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid().ToString(), UserName = "test" };
        var (refreshToken, tokenString) = await _tokenProvider.GenerateRefreshTokenAsync(user);

        // Act
        var validated = await _tokenProvider.ValidateRefreshTokenAsync(tokenString);

        // Assert
        Assert.Equal(refreshToken.Token, validated.Token);
        Assert.Equal(refreshToken.User.Id, validated.User.Id);
    }

    [Fact]
    public async Task ValidateRefreshToken_ShouldThrowWhenInvalid()
    {
        // Act + Assert
        await Assert.ThrowsAsync<InvalidRefreshTokenException>(() => _tokenProvider.ValidateRefreshTokenAsync("invalid-refresh-token"));
    }

    [Fact]
    public async Task GenerateAccessToken_ShouldRevokeAndRotateRefreshToken()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid().ToString(), UserName = "test" };
        var (refreshToken, _) = await _tokenProvider.GenerateRefreshTokenAsync(user);

        // Act
        var tokens = await _tokenProvider.GenerateAccessTokenAsync(refreshToken);

        // Assert
        Assert.NotEmpty(tokens.AccessToken);
        Assert.NotEmpty(tokens.RefreshToken);
        Assert.Equal(RefreshTokenStatus.Revoked, refreshToken.Status);
    }

    [Fact]
    public async Task RevokeAsync_ShouldRevokeValidToken()
    {
        // Arrange
        var user = new User { UserName = "test" };
        var (refreshToken, tokenString) = await _tokenProvider.GenerateRefreshTokenAsync(user);

        // Act
        await _tokenProvider.RevokeAsync(tokenString);

        // Assert
        var revoked = await _context.RefreshTokens.FirstAsync(TestContext.Current.CancellationToken);
        Assert.Equal(RefreshTokenStatus.Revoked, revoked.Status);
    }

    [Fact]
    public async Task RevokeAsync_ShouldThrowWhenInvalidBase64()
    {
        // Act + Assert
        await Assert.ThrowsAsync<InvalidRefreshTokenException>(() => _tokenProvider.RevokeAsync("not-base64"));
    }

    [Fact]
    public async Task RevokeAsync_ShouldThrowWhenTokenNotFound()
    {
        // Arrange
        var randomToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

        // Act + Assert
        await Assert.ThrowsAsync<InvalidRefreshTokenException>(() => _tokenProvider.RevokeAsync(randomToken));
    }
}
