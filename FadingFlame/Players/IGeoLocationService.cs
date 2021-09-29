using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace FadingFlame.Players
{
    public interface IGeoLocationService
    {
        Task<Location> GetLoggedInUserLocation();
        Task<DateTimeOffset> GetLoggedInUserTime();
    }

    public class GeoLocationService : IGeoLocationService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _accessor;
        private readonly ILogger<IGeoLocationService> _logger;

        public GeoLocationService(IJSRuntime jsRuntime, HttpClient httpClient, IHttpContextAccessor accessor, ILogger<IGeoLocationService> logger)
        {
            _jsRuntime = jsRuntime;
            _httpClient = httpClient;
            _accessor = accessor;
            _logger = logger;
        }
        
        public async Task<Location> GetLoggedInUserLocation()
        {
            var userIpAdress = _accessor.HttpContext?.Request.Headers["X-Forwarded-For"];
            var decodedIp = HttpUtility.UrlEncode(userIpAdress?.ToString());
            _logger.LogInformation($"using ip: {decodedIp}");
            var httpResponseMessage = await _httpClient.GetAsync($"?ip={decodedIp}");
            var content = await httpResponseMessage.Content.ReadAsStringAsync();
            var info = JsonConvert.DeserializeObject<LocationDto>(content);
            _logger.LogInformation($"return value geocode: {content}");
            _logger.LogInformation($"item: {JsonConvert.SerializeObject(info)}");
            var timeZoneInfos = TimeZoneInfo.GetSystemTimeZones();
            var strings = info.Timezone?.Split("/");
            if (strings?.Length > 1)
            {
                info.Timezone = strings[1];
            }
            var location = new Location
            {
                TimezoneRaw = info.Timezone != null ? timeZoneInfos.FirstOrDefault(ti => ti.DisplayName.Contains(info.Timezone))?.Id : null,
                Country = info.CuntryCode != null ? new RegionInfo(info.CuntryCode) : null
            };

            if (location.Timezone == null)
            {
                var timeDiff = await _jsRuntime.InvokeAsync<int>("GetTimezoneOffset");
                _logger.LogInformation($"offset: {timeDiff}");
                var timeSpanDiff = TimeSpan.FromMinutes(-timeDiff);
                location.TimezoneRaw = timeZoneInfos.FirstOrDefault(ti => ti.BaseUtcOffset == timeSpanDiff)?.Id;
            }

            return location;
        }

        public async Task<DateTimeOffset> GetLoggedInUserTime()
        {
            // this is in the main layout class hard coded
            var timeDiff = await _jsRuntime.InvokeAsync<int>("GetTimezoneOffset");
            var timeSpanDiff = TimeSpan.FromMinutes(-timeDiff);
            return DateTimeOffset.Now.ToOffset(timeSpanDiff);
        }
    }

    public class Location
    {
        public RegionInfo Country { get; set; }

        [BsonIgnore]
        public TimeZoneInfo Timezone => TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(ti => ti.Id == TimezoneRaw);
        public string TimezoneRaw { get; set; }
    }

    public class LocationDto
    {
        [JsonProperty("geoplugin_countryCode")]
        public string CuntryCode { get; set; }
        [JsonProperty("geoplugin_timezone")]
        public string Timezone { get; set; }
    }
}