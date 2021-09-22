using System;

namespace FadingFlame.Admin
{
    public class SeasonState
    {
        public event EventHandler SeasonsChanged;

        public virtual void SetSeasons(Season currentSeason, Season nextSeason)
        {
            CurrentSeason = currentSeason;
            NextSeason = nextSeason;
            SeasonsChanged?.Invoke(this, EventArgs.Empty);
        }

        public Season CurrentSeason { get; private set; } = new();
        public Season NextSeason { get; private set; } = new();
    }

    public static class DateExtensions
    {
        public static string ToMyDate(this DateTime time)
        {
            return time.ToString("dd.MM.");
        }
    }
}