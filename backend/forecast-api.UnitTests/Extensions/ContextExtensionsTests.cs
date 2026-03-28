using System.Net;
using forecast_api.Extensions;
using Microsoft.AspNetCore.Http;
using Moq;

namespace forecast_api.UnitTests.Extensions;

public class ContextExtensionsTests
{
    [Fact]
    public void GetIpAddress_WithRemoteIpAddress_ReturnsRemoteIp()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse("203.0.113.45");

        // Act
        var result = httpContext.GetIpAddress();

        // Assert
        Assert.Equal("203.0.113.45", result);
    }

    [Fact]
    public void GetIpAddress_WithLoopbackAddress_ReturnsNull()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.Loopback;

        // Act
        var result = httpContext.GetIpAddress();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetIpAddress_WithNoIpInfo_ReturnsNull()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();

        // Act
        var result = httpContext.GetIpAddress();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetIpAddress_WithIPv6Address_ReturnsIPv6()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse("2001:0db8:85a3:0000:0000:8a2e:0370:7334");

        // Act
        var result = httpContext.GetIpAddress();

        // Assert
        Assert.Equal("2001:db8:85a3::8a2e:370:7334", result);
    }
}
