using System;
using System.Collections.Generic;

namespace FadingFlame.Admin
{
    public class SeasonState
    {
        public event EventHandler SeasonsChanged;

        public virtual void SetSeasons(List<Season> seasons)
        {
            Seasons = seasons;
            NextSeason = seasons[0];
            CurrentSeason = seasons[1];
            SeasonsChanged?.Invoke(this, EventArgs.Empty);
        }

        public Season CurrentSeason { get; private set; } = new();
        public Season NextSeason { get; private set; } = new();
        public List<Season> Seasons { get; private set; } = new();

        public void AppendSeason(Season season)
        {
            Seasons.Insert(0, season);
            SetSeasons(Seasons);
        }

        public Season PopLastSeason()
        {
            var season = Seasons[0];
            Seasons.RemoveAt(0);
            SetSeasons(Seasons);
            return season;
        }
    }

    public static class DateExtensions
    {
        public static string ToMyDate(this DateTime time)
        {
            return time.ToString("dd.MM.");
        }
        
        public static string ToMyDateYear(this DateTime time)
        {
            return time.ToString("dd.MM.yyyy");
        }
        
        public static string ToMyDateTime(this DateTime time)
        {
            return time.ToString("dd.MM. hh:mm");
        }
    }
}