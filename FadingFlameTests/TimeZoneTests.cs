using System.Globalization;
using FadingFlame.Players;
using NUnit.Framework;

namespace FadingFlameTests
{
    [TestFixture]
    public class TimeZoneTests
    {
        [Ignore("so dumb")]
        [Test]
        public void CreateFirstPlayoffs()
        {
            var geoLocationService = new GeoLocationService(null, null, null);

            var regionInfos = geoLocationService.GetCountries();

            foreach (var regionInfo in regionInfos)
            {
                var info = new RegionInfo(regionInfo.TwoLetterISORegionName);
                Assert.AreEqual(info.TwoLetterISORegionName, regionInfo.TwoLetterISORegionName);
            }
        }
    }
}