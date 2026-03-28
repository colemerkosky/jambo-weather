using forecast_api.Models;
using forecast_api.Models.Dto;
using forecast_api.Services;

namespace forecast_api.UnitTests.Services;

public class DummyAuthenticationServiceTests
{
    private readonly DummyAuthenticationService _service;

    public DummyAuthenticationServiceTests()
    {
        _service = new DummyAuthenticationService();
    }

    [Fact]
    public void IsAuthenticated_WithCorrectPassword_ReturnsTrue()
    {
        // Arrange
        var request = new AuthRequest { Username = "testuser", Password = "Sup3rSe_cr3tP@ssw0rd" };

        // Act
        var result = _service.IsAuthenticated(request);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsAuthenticated_WithIncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var request = new AuthRequest { Username = "testuser", Password = "wrongpassword" };

        // Act
        var result = _service.IsAuthenticated(request);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsAuthenticated_WithEmptyPassword_ReturnsFalse()
    {
        // Arrange
        var request = new AuthRequest { Username = "testuser", Password = "" };

        // Act
        var result = _service.IsAuthenticated(request);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsAuthenticated_WithNullPassword_ReturnsFalse()
    {
        // Arrange
        var request = new AuthRequest { Username = "testuser", Password = null! };

        // Act
        var result = _service.IsAuthenticated(request);

        // Assert
        Assert.False(result);
    }
}
