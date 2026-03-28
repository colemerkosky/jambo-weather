using forecast_api.Controllers;
using forecast_api.Models;
using forecast_api.Models.Dto;
using forecast_api.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace forecast_api.UnitTests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IAuthenticationService> _authenticationServiceMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _authenticationServiceMock = new Mock<IAuthenticationService>();
        
        // Setup configuration mocks
        _configurationMock.Setup(x => x["Jwt:Key"]).Returns("my_secret_key_that_is_long_enough_for_hmac");
        _configurationMock.Setup(x => x["Jwt:Issuer"]).Returns("forecast-api");
        _configurationMock.Setup(x => x["Jwt:Audience"]).Returns("forecast-api");
        
        _controller = new AuthController(_configurationMock.Object, _authenticationServiceMock.Object);
    }

    [Fact]
    public void Token_WithValidCredentials_ReturnsOkWithAuthResponse()
    {
        // Arrange
        var request = new AuthRequest { Username = "testuser", Password = "password123" };
        _authenticationServiceMock.Setup(x => x.IsAuthenticated(request)).Returns(true);

        // Act
        var result = _controller.Token(request);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        Assert.NotNull(okResult?.Value);
        var authResponse = okResult.Value as AuthResponse;
        Assert.NotNull(authResponse);
        Assert.NotEmpty(authResponse.Token);
        Assert.NotEmpty(authResponse.RefreshToken);
        Assert.Equal("Bearer", authResponse.TokenType);
    }

    [Fact]
    public void Token_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var request = new AuthRequest { Username = "testuser", Password = "wrongpassword" };
        _authenticationServiceMock.Setup(x => x.IsAuthenticated(request)).Returns(false);

        // Act
        var result = _controller.Token(request);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public void Token_WithNullRequest_ReturnsUnauthorized()
    {
        // Act
        var result = _controller.Token(null!);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public void Token_WithEmptyUsername_ReturnsUnauthorized()
    {
        // Arrange
        var request = new AuthRequest { Username = "", Password = "password123" };

        // Act
        var result = _controller.Token(request);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public void Token_WithEmptyPassword_ReturnsUnauthorized()
    {
        // Arrange
        var request = new AuthRequest { Username = "testuser", Password = "" };

        // Act
        var result = _controller.Token(request);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public void Token_ReturnsAccessTokenWithCorrectExpiration()
    {
        // Arrange
        var request = new AuthRequest { Username = "testuser", Password = "password123" };
        _authenticationServiceMock.Setup(x => x.IsAuthenticated(request)).Returns(true);
        var beforeCall = DateTime.UtcNow;

        // Act
        var result = _controller.Token(request);
        var afterCall = DateTime.UtcNow;

        // Assert
        var okResult = result as OkObjectResult;
        var authResponse = okResult?.Value as AuthResponse;
        Assert.NotNull(authResponse);
        Assert.True(authResponse.ExpiresAtUtc > beforeCall.AddHours(1));
        Assert.True(authResponse.ExpiresAtUtc < afterCall.AddHours(1));
    }

    [Fact]
    public void Token_ReturnsRefreshTokenWithCorrectExpiration()
    {
        // Arrange
        var request = new AuthRequest { Username = "testuser", Password = "password123" };
        _authenticationServiceMock.Setup(x => x.IsAuthenticated(request)).Returns(true);
        var beforeCall = DateTime.UtcNow;

        // Act
        var result = _controller.Token(request);
        var afterCall = DateTime.UtcNow;

        // Assert
        var okResult = result as OkObjectResult;
        var authResponse = okResult?.Value as AuthResponse;
        Assert.NotNull(authResponse);
        Assert.True(authResponse.RefreshTokenExpiresAtUtc > beforeCall.AddDays(7));
        Assert.True(authResponse.RefreshTokenExpiresAtUtc < afterCall.AddDays(7));
    }

    [Fact]
    public void RefreshToken_WithValidRefreshToken_ReturnsNewAccessToken()
    {
        // Arrange
        var request = new AuthRequest { Username = "testuser", Password = "password123" };
        _authenticationServiceMock.Setup(x => x.IsAuthenticated(request)).Returns(true);
        
        // First, get initial tokens
        var tokenResult = _controller.Token(request) as OkObjectResult;
        var authResponse = tokenResult?.Value as AuthResponse;
        var refreshToken = authResponse!.RefreshToken;

        // Now refresh
        var refreshRequest = new RefreshTokenRequest { RefreshToken = refreshToken };

        // Act
        var result = _controller.RefreshToken(refreshRequest);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        var newAuthResponse = okResult?.Value as AuthResponse;
        Assert.NotNull(newAuthResponse);
        Assert.NotEmpty(newAuthResponse.Token);
        Assert.NotEmpty(newAuthResponse.RefreshToken);
    }

    [Fact]
    public void RefreshToken_WithInvalidRefreshToken_ReturnsUnauthorized()
    {
        // Arrange
        var refreshRequest = new RefreshTokenRequest { RefreshToken = "invalid_token_12345" };

        // Act
        var result = _controller.RefreshToken(refreshRequest);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public void RefreshToken_WithNullRequest_ReturnsBadRequest()
    {
        // Act
        var result = _controller.RefreshToken(null!);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void RefreshToken_WithEmptyRefreshToken_ReturnsBadRequest()
    {
        // Arrange
        var refreshRequest = new RefreshTokenRequest { RefreshToken = "" };

        // Act
        var result = _controller.RefreshToken(refreshRequest);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}
