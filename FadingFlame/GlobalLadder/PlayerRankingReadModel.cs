using System.Collections.Generic;
using System.Linq;
using FadingFlame.Matchups;
using FadingFlame.Players;
using FadingFlame.Repositories;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FadingFlame.GlobalLadder;

public class PlayerRankingReadModel : IIdentifiable
{
    public ObjectId Id { get; set; }
    public string Name { get; set; }
    public int Wins { get; set; }
    public double WinRate { get; set; }
    public int Losses { get; set; }
    public int Draws { get; set; }
    public int Mmr { get; set; }
        
    [BsonIgnore]
    public int MatchCount => Wins + Losses + Draws;

    public Location Location { get; set; }

    public static PlayerRankingReadModel Create(Player player, List<MatchResult> playersMatches)
    {
        var wins = playersMatches.Count(m => m.Winner == player.Id);
        var losses = playersMatches.Count(m => m.Winner != player.Id && !m.IsDraw);
        var winRate = losses + wins != 0 ? wins / (double) (wins + losses) : 0;
            
        return new PlayerRankingReadModel
        {
            Draws = playersMatches.Count(m => m.IsDraw),
            Wins = wins,
            Mmr = (int) player.Mmr.Rating,
            WinRate = winRate * 100,
            Losses = losses,
            Id = player.Id,
            Name = player.DisplayName,
            Location = player.Location
        };
    }
}