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
            .ReturnsAsync(new TokensDto("", "access"));

        // Act
        var result = await _authService.RegisterAsync(new UserRegisterDto("test", "test@example.com", "123456"));

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
            _authService.LoginAsync(new UserLoginDto("test", "123")));
    }

    [Fact]
    public async Task GenerateAccessToken_ShouldValidateRefreshToken()
    {
        // Arrange
        var refreshToken = new RefreshToken();
        _tokenProviderMock.Setup(x => x.ValidateRefreshTokenAsync("token")).ReturnsAsync(refreshToken);
        _tokenProviderMock.Setup(x => x.GenerateAccessTokenAsync(refreshToken, true))
            .ReturnsAsync(new TokensDto("new-refresh", "new-access"));

        // Act
        var result = await _authService.GenerateAccessToken("token");

        // Assert
        Assert.Equal("new-access", result.AccessToken);
        _tokenProviderMock.Verify(x => x.ValidateRefreshTokenAsync("token"), Times.Once);
    }

    [Fact]
    public async Task LogOut_ShouldCallRevokeAsync()
    {
        // Arrange
        const string refreshToken = "refresh-token";
        _tokenProviderMock.Setup(x => x.RevokeAsync(refreshToken)).Returns(Task.CompletedTask);

        // Act
        await _authService.LogOutAsync(refreshToken);

        // Assert
        _tokenProviderMock.Verify(x => x.RevokeAsync(refreshToken), Times.Once);
    }


}
