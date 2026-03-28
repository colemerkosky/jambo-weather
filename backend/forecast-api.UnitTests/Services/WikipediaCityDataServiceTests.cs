using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using forecast_api.Models;
using forecast_api.Services;
using Moq;
using Moq.Protected;

namespace forecast_api.UnitTests.Services;

public class WikipediaCityDataServiceTests
{
    [Fact]
    public async Task GetCityDataAsync_WithValidWikiHtml_ReturnsCityData()
    {
        // Arrange
        var cityName = "Test City";
        var html = @"<html><body>
            <p>Test City is a sample city in a fictional world.[1]</p>
            <table class='infobox'>
                <tr><th>Population</th></tr>
                <tr><th>Metro</th><td>123,456</td></tr>
                <tr><th>Urban</th><td>100,000</td></tr>
            </table>
            </body></html>";

        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Get && r.RequestUri!.ToString().Contains("/Test_City/html")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(html, Encoding.UTF8, "text/html")
            })
            .Verifiable();

        var httpClient = new HttpClient(handler.Object);
        var service = new WikipediaCityDataService(httpClient);

        var geolocation = new Geolocation
        {
            CityName = cityName,
            CountryCode = "TT",
            Coordinates = new Coordinates { Latitude = 1.0, Longitude = 2.0 },
            Timezone = "UTC"
        };

        // Act
        var result = await service.GetCityDataAsync(geolocation);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test City is a sample city in a fictional world.", result!.Blurb);
        Assert.Equal(123456, result.Population);
        Assert.Equal("https://en.wikipedia.org/wiki/Test_City", result.LearnMoreUrl);
        Assert.Equal(geolocation, result.Geolocation);

        handler.Protected().Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetCityDataAsync_WhenWikiPageNotFound_ReturnsNull()
    {
        // Arrange
        var cityName = "Missing City";

        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Get && r.RequestUri!.ToString().Contains("/Missing_City/html")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound))
            .Verifiable();

        var httpClient = new HttpClient(handler.Object);
        var service = new WikipediaCityDataService(httpClient);

        var geolocation = new Geolocation
        {
            CityName = cityName,
            CountryCode = "TT",
            Coordinates = new Coordinates { Latitude = 3.0, Longitude = 4.0 },
            Timezone = "UTC"
        };

        // Act
        var result = await service.GetCityDataAsync(geolocation);

        // Assert
        Assert.Null(result);

        handler.Protected().Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
    }
}
