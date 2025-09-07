using FitnessTrackerApi.DTOs;
using FitnessTrackerApi.Models;
using FitnessTrackerApi.Services.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace FitnessTrackerApi.Test;

public class AuthServiceTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<ITokenProvider> _tokenProviderMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userManagerMock = MockHelpers.MockUserManager<User>();
        _tokenProviderMock = new Mock<ITokenProvider>();
        var logger = Mock.Of<ILogger<AuthService>>();

        _authService = new AuthService(logger, _userManagerMock.Object, _tokenProviderMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnTokensWhenSuccessful()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid().ToString(), UserName = "test" };
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _tokenProviderMock.Setup(x => x.GenerateRefreshTokenAsync(It.IsAny<User>()))
            .ReturnsAsync((new RefreshToken(), "refresh"));
        _tokenProviderMock.Setup(x => x.GenerateAccessTokenAsync(It.IsAny<RefreshToken>(), false))
            .ReturnsAsync(new Tokens("", "access"));

        // Act
        var result = await _authService.RegisterAsync(new UserRegister("test", "test@example.com", "123456"));

        // Assert
        Assert.Equal("access", result.AccessToken);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowWhenPasswordInvalid()
    {
        // Arrange
        var user = new User { UserName = "test" };
        _userManagerMock.Setup(x => x.FindByNameAsync("test")).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, "123")).ReturnsAsync(false);

        // Act + Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(() =>
            _authService.LoginAsync(new UserLogin("test", "123")));
    }

    [Fact]
    public async Task GenerateAccessToken_ShouldValidateRefreshToken()
    {
        // Arrange
        var refreshToken = new RefreshToken();
        _tokenProviderMock.Setup(x => x.ValidateRefreshToken("token")).ReturnsAsync(refreshToken);
        _tokenProviderMock.Setup(x => x.GenerateAccessTokenAsync(refreshToken, true))
            .ReturnsAsync(new Tokens("new-refresh", "new-access"));

        // Act
        var result = await _authService.GenerateAccessToken("token");

        // Assert
        Assert.Equal("new-access", result.AccessToken);
        _tokenProviderMock.Verify(x => x.ValidateRefreshToken("token"), Times.Once);
    }

}
