using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FadingFlame.Admin;
using FadingFlame.Lists;
using FadingFlame.Matchups;
using FadingFlame.Players;
using FadingFlame.Repositories;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FadingFlame.Leagues;

public class League : IIdentifiable, IVersionable
{
    public const int MaxPlayerCount = 6;
    public const int GamesCount = MaxPlayerCount - 1;
    public string Name { get; set; }
    public int Version { get; set; } = 0;
    public int Season { get; set; }
    [BsonId]
    public ObjectId Id { get; set; }
    public List<PlayerInLeague> Players { get; set; } = new();
    public string DivisionId { get; set; }
    public List<GameDay> GameDays { get; set; } = new();
        
    [BsonIgnore]
    public List<Matchup> RelegationMatches => GameDays.Count == MaxPlayerCount 
        ? GameDays.Last().Matchups 
        : new List<Matchup>();

    [BsonIgnore]
    public Matchup RelegationMatchOverOneLeague => RelegationMatches.LastOrDefault();
    [BsonIgnore]
    public Matchup RelegationMatchOverTwoLeagues => RelegationMatches.SkipLast(1).LastOrDefault();

    public DateTime StartDate { get; set; }
    public DateTime RelegationDeadLine { get; set; }

    public void RemoveFromLeague(ObjectId playerId)
    {
        SwapPlayer(playerId, ObjectId.Empty);
    }

    private void SwapPlayer(ObjectId oldPlayer, ObjectId newPlayer)
    {
        if (Players.Any(p => p.Id == newPlayer)) return; 
        var player = Players.Single(p => p.Id == oldPlayer);

        player.Id = newPlayer;

        var matchesOfPlayer = GameDays.SelectMany(gameDay => gameDay.Matchups)
            .Where(m => m.Player1 == oldPlayer || m.Player2 == oldPlayer);
        foreach (var matchup in matchesOfPlayer)
        {
            if (matchup.Player1 == oldPlayer)
            {
                matchup.Player1 = newPlayer;
            }

            if (matchup.Player2 == oldPlayer)
            {
                matchup.Player2 = newPlayer;
            }
        }
    }

    private Matchup GetMatchup(ObjectId matchId)
    {
        return GameDays.SelectMany(g => g.Matchups).Single(m => m.Id == matchId);
    }

    public async Task<MatchResult> ReportGame(
        IMmrRepository mmrRepository, 
        MatchResultDto matchResultDto, 
        Mmr player1Mmr,
        Mmr player2Mmr,
        GameList player1List, 
        GameList player2List)
    {
        var player1Result = matchResultDto.Player1;
        var player2Result = matchResultDto.Player2;

        var match = GetMatchup(matchResultDto.MatchId);
            
        var result = await MatchResult.Create(mmrRepository, matchResultDto.SecondaryObjective, player1Mmr, player2Mmr, player1Result, player2Result, player1List, player2List, matchResultDto.WasDefLoss);
        match.RecordResult(result);

        return result;
    }

    public void AddPlayer(Player player)
    {
        if (Players.Count == MaxPlayerCount) return;
            
        Players = Players.Where(p => p.Id != player.Id).ToList();
        var playerInLeague = PlayerInLeague.Create(player.Id);
        Players.Add(playerInLeague);
    }

    public static League Create(int season, DateTime startDate, string divisionId, string name)
    {
        return new()
        {
            Season = season,
            DivisionId = divisionId,
            Name = name,
            StartDate = startDate
        };
    }

    public void CreateGameDays()
    {
        var teams = Players.ToList();
        if (teams.Count % 2 != 0)
        {
            teams.Add(PlayerInLeague.Create(ObjectId.Empty));
        }
        var numberOfRounds = teams.Count - 1;
        var numberOfMatchesInARound = teams.Count / 2;

        var teamsTemp = new List<PlayerInLeague>();

        teamsTemp.AddRange(teams.Skip(numberOfMatchesInARound).Take(numberOfMatchesInARound));
        teamsTemp.AddRange(teams.Skip(1).Take(numberOfMatchesInARound - 1).Reverse());

        var numberOfPlayers = teamsTemp.Count;

        var gameDays = new List<GameDay>();

        for (var gameDayIndex = 0; gameDayIndex < numberOfRounds; gameDayIndex++)
        {
            var matchups = new List<Matchup>();

            var playerIndex = gameDayIndex % numberOfPlayers;

            var offset = StartDate.AddDays(14 * gameDayIndex);
            var matchup = Matchup.CreateForLeague(teamsTemp[playerIndex], teams.First());
            matchups.Add(matchup);

            for (var index = 1; index < numberOfMatchesInARound; index++)
            {
                var firstPlayerIndex = (gameDayIndex + index) % numberOfPlayers;
                var secondPlayerIndex = (gameDayIndex + numberOfPlayers - index) % numberOfPlayers;

                var playerInLeague = teamsTemp[firstPlayerIndex];
                var playerAsGuest = teamsTemp[secondPlayerIndex];
                if (playerInLeague.Id == ObjectId.Empty || playerAsGuest.Id == ObjectId.Empty)
                {
                    continue;
                }

                var matchupInner = Matchup.CreateForLeague(playerInLeague, playerAsGuest);
                matchups.Add(matchupInner);
            }

            var round = GameDay.Create(offset, matchups);
            gameDays.Add(round);
        }

        Players = Players.Where(l => l.Id != ObjectId.Empty).ToList();
        GameDays = gameDays;
    }

    public void PenaltyPointsForPlayer(ObjectId playerId, int penaltyPoints)
    {
        var player = Players.Single(p => p.Id == playerId);
        player.Penalty(penaltyPoints);
    }

    public void DeleteGameReport(ObjectId matchId)
    {
        var match = GetMatchup(matchId);
        match.DeleteResult();
    }

    public void ReorderPlayers()
    {
        foreach (var playerInLeague in Players)
        {
            var gamesOfPlayer = GameDays.SelectMany(m => m.Matchups).Where(m => 
                   (m.Player1 == playerInLeague.Id || m.Player2 == playerInLeague.Id) 
                   && m.IsFinished);
            foreach (var matchup in gamesOfPlayer)
            {
                playerInLeague.CountPoints(matchup.Player1 == playerInLeague.Id
                    ? matchup.Result.Player1
                    : matchup.Result.Player2);
            }
        }

        Players = Players.OrderBy(c => c, new PlayerPlacementComparer(this)).ToList();
    }

    public void ReplaceDummyPlayer(ObjectId playerId)
    {
        SwapPlayer(ObjectId.Empty, playerId);
    }

    public void SetScenarioAndDeployments(List<SecondaryObjective> secondaryObjectives, List<Deployment> deployments)
    {
        for (int i = 0; i < GameDays.Count; i++)
        {
            GameDays[i].SetScenarioAndDeployments(secondaryObjectives[i], deployments[i]);
        }
    }

    public void SetZeroToZero(ObjectId matchId)
    {
        var match = GetMatchup(matchId);
        match.SetZeroToZero();
    }

    public void CreateRelegations(League oneLeagueBelow, League twoLeagueBelow, bool isUneven)
    {
        GameDays = GameDays.Take(MaxPlayerCount - 1).ToList();
            
        var relegationMatches = new List<Matchup>();
                
        if (twoLeagueBelow != null && !DivisionId.StartsWith("1"))
        {
            relegationMatches.Add(Matchup.CreateRelegationGame(Players[3].Id, twoLeagueBelow.Players[0].Id));
        }
                
        if (oneLeagueBelow != null)
        {
            relegationMatches.Add(Matchup.CreateRelegationGame(Players[4].Id, oneLeagueBelow.Players[1].Id));
        }

        GameDays.Add(GameDay.Create(GameDays.Last().StartDate.AddDays(14), relegationMatches));
    }

    public void ResetZeroToZero(ObjectId matchId)
    {
        var match = GetMatchup(matchId);
        match.ResetGame();
    }
}