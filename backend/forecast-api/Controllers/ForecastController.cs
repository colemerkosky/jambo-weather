using System.Globalization;
using forecast_api.Extensions;
using forecast_api.Models;
using forecast_api.Services;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace forecast_api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class ForecastController(IConfiguration configuration, IForecastService forecastService, IIPGeolocationService geolocationService, ICityService cityService): ControllerBase
    {
        [HttpGet("{cityId}")]
        public async Task<IActionResult> GetForecastByCityId(string cityId, [FromQuery] DateOnly? forecastDate)
        {
            var geolocation = cityService.GetGeolocationByCityId(cityId);
            if(geolocation == null)
            {
                return NotFound();
            }

            return await GetForecast(geolocation, forecastDate);
        }

        [HttpGet("current")]
        public async Task<IActionResult> GetForecastByIP([FromQuery] DateOnly? forecastDate)
        {
            var ipAddress = HttpContext.GetIpAddress() ?? configuration["FallbackIPAddress"];
            if(ipAddress == null)
            {
                return NotFound();
            }

            var geolocation = await geolocationService.GetGeolocationOfIpAddressAsync(ipAddress);
            if(geolocation == null)
            {
                return NotFound();
            }

            return await GetForecast(geolocation, forecastDate);
        }

        private async Task<IActionResult> GetForecast(Geolocation geolocation, DateOnly? forecastDate)
        {  
            var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(geolocation.Timezone);
            var convertedTime = TimeZoneInfo.ConvertTime(DateTime.Now, tzInfo);

            var forecastData = await forecastService.GetForecastDataAsync(geolocation, forecastDate ?? DateOnly.FromDateTime(convertedTime));
            if(forecastData is null)
            {
                return NotFound();
            }

            return Ok(forecastData);
        }
    }
}