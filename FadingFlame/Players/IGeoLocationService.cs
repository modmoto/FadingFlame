using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.JSInterop;

namespace FadingFlame.Players;

public interface IGeoLocationService
{
    Task<Location> GetLoggedInUserLocation();
    Task<DateTimeOffset> GetLoggedInUserTime();
    List<RegionInfo> GetCountries();
    List<TimeZoneInfo> GetTimeZones();
    TimeSpan GetTimeDiff(DateTimeOffset? timeOfUser, string timeZoneOfSelectedPlayer);
}

public class GeoLocationService : IGeoLocationService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _accessor;

    public GeoLocationService(IJSRuntime jsRuntime, HttpClient httpClient, IHttpContextAccessor accessor)
    {
        _jsRuntime = jsRuntime;
        _httpClient = httpClient;
        _accessor = accessor;
    }
        
    public TimeSpan GetTimeDiff(DateTimeOffset? timeOfUser, string timeZoneOfSelectedPlayer)
    {
        var timeZoneInfos = GetTimeZones();
        var timeZoneOfPlayer = timeZoneInfos.FirstOrDefault(ti => ti.Id == timeZoneOfSelectedPlayer);
        if (timeOfUser != null && timeZoneOfPlayer != null)
        {
            if (timeZoneOfPlayer.IsDaylightSavingTime(DateTimeOffset.UtcNow))
            {
                return timeZoneOfPlayer.BaseUtcOffset + TimeSpan.FromHours(1) - timeOfUser.Value.Offset;
            }
            
            return timeZoneOfPlayer.BaseUtcOffset - timeOfUser.Value.Offset;
        }
        
        return TimeSpan.Zero;
    }
    public async Task<Location> GetLoggedInUserLocation()
    {
        var userIpAdress = _accessor.HttpContext?.Request.Headers["X-Forwarded-For"];
        var decodedIp = HttpUtility.UrlEncode(userIpAdress?.ToString());
        var httpResponseMessage = await _httpClient.GetAsync($"?ip={decodedIp}");
        var content = await httpResponseMessage.Content.ReadAsStringAsync();
        var info = JsonSerializer.Deserialize<LocationDto>(content);
        var timeZoneInfos = GetTimeZones();

        var location = new Location();
        if (info != null)
        {
            location.TimezoneRaw = info.Timezone != null
                ? timeZoneInfos.FirstOrDefault(ti => ti.Id == info.Timezone)?.Id
                : null;
            try
            {

                location.Country = info.CuntryCode != null 
                    ? new RegionInfo(info.CuntryCode) 
                    : null;
            }
            catch (Exception)
            {
                // ignored because region info sucks ass
            }
        }
            
        if (location.TimezoneRaw == null)
        {
            var timeDiff = await _jsRuntime.InvokeAsync<int>("GetTimezoneOffset");
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

    public List<RegionInfo> GetCountries()
    {
        return CultureInfo.GetCultures(CultureTypes.SpecificCultures)
            .Where(c => c.LCID != 4096)
            .Select(x => new RegionInfo(x.LCID))
            .OrderBy(i => i.EnglishName)
            .Distinct()
            .ToList();
    }

    public List<TimeZoneInfo> GetTimeZones()
    {
        return TimeZoneInfo.GetSystemTimeZones().OrderBy(t => t.Id).ToList();
    }
}

public class LocationDto
{
    [JsonPropertyName("geoplugin_countryCode")]
    public string CuntryCode { get; set; }
    [JsonPropertyName("geoplugin_timezone")]
    public string Timezone { get; set; }
}