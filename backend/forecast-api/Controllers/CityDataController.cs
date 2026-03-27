using System.Net;
using forecast_api.Models;
using forecast_api.Services;
using forecast_api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace forecast_api.Controllers 
{
    [Authorize]
    [ApiController]
    [Route("api/city")]
    public class CityDataController(
        ICityDataService cityDataService,
        ICityService cityService, 
        IIPGeolocationService geolocationService,
        IConfiguration configuration
    ): ControllerBase
    {
        [HttpGet]
        public IActionResult GetCityList()
        {
            return Ok(cityService.GetCityList());
        }

        [HttpGet("data/{cityId}")]
        public async Task<IActionResult> GetCityDataByCityId(string cityId)
        {
            var geolocation = cityService.GetGeolocationByCityId(cityId);
            return await GetCityData(geolocation);
        }

        [HttpGet("data/current")]
        public async Task<IActionResult> GetCityDataByIP()
        {
            var ipAddress = HttpContext.GetIpAddress() ?? configuration["FallbackIPAddress"];
            if(ipAddress == null)
            {
                return NotFound();
            }

            var geolocation = await geolocationService.GetGeolocationOfIpAddressAsync(ipAddress);
            return await GetCityData(geolocation);
        }

        private async Task<IActionResult> GetCityData(Geolocation? geolocation)
        {
            if(geolocation == null)
            {
                return NotFound();
            }

            var cityData = await cityDataService.GetCityDataAsync(geolocation);
            if(cityData == null)
            {
                return NotFound();
            }

            return Ok(cityData);
        }
    }
}