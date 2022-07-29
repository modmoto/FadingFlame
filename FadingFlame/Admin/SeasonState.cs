using System;
using System.Collections.Generic;

namespace FadingFlame.Admin;

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
}