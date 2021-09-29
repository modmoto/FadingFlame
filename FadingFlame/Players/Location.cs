using System;
using System.Globalization;

namespace FadingFlame.Players
{
    public class Location
    {
        public const string NotSelected = "nono";

        public static Location Create(string country, string timeZone)
        {
            var location = new Location();
            if (country != NotSelected)
            {
                try
                {
                    location.Country = new RegionInfo(country);
                }
                catch (Exception)
                {
                    // ignored as Regioninfo has no try parse
                }
            }
            
            if (timeZone != NotSelected)
            {
                location.TimezoneRaw = timeZone;
            }

            return location;
        }
        public RegionInfo Country { get; set; }
        public string TimezoneRaw { get; set; }
    }
}