using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FadingFlame.Leagues;
using FadingFlame.Lists;
using FadingFlame.Matchups;
using FadingFlame.Players;
using FadingFlame.Repositories;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FadingFlame.Playoffs;

public class Playoff : IIdentifiable, IVersionable
{
    private static readonly List<int> NormalRounds = new() { 256, 128, 64, 32, 16, 8, 4, 2 };
        
    [BsonId]
    public ObjectId Id { get; set; }
    public int Season { get; set; }
    public int Version { get; set; } = 0;
    public List<Round> Rounds { get; set; }

    public static async Task<Playoff> Create(
        IMmrRepository mmrRepository, 
        int season,
        List<League> leagues)
    {
        var betterPlayers = new List<PlayerInLeague>();
        // first leagues
        betterPlayers.Add(BetterOf(leagues, 1, 1));
        betterPlayers.Add(BetterOf(leagues, 2, 2));
        betterPlayers.Add(WeekerOf(leagues, 1, 2));
        betterPlayers.Add(BetterOf(leagues, 2, 1));
        betterPlayers.Add(WeekerOf(leagues, 1, 1));
        betterPlayers.Add(BetterOf(leagues, 1, 3));
        betterPlayers.Add(BetterOf(leagues, 1, 2));
        betterPlayers.Add(WeekerOf(leagues, 2, 1));

        var lowerLeagues = leagues.Skip(4).ToList();
        var weekerPlayers = lowerLeagues.Select(l => l.Players.First()).ToList();
        if (weekerPlayers.Count < 16)
        {
            var remainingSeconds = lowerLeagues.Select(l => l.Players.Skip(1).First()).Take(16 - weekerPlayers.Count);
            weekerPlayers.AddRange(remainingSeconds);
        }
        weekerPlayers.Shuffle();

        var playersWithFreeWins = new List<PlayerInLeague>();
        var playersCount = weekerPlayers.Count + betterPlayers.Count;
        var remainingRounds = NormalRounds.Where(r => r < playersCount).ToList();
        for (int i = 0; i < 8; i++)
        {
            var dummyPlayer = PlayerInLeague.Create(ObjectId.Empty);
            playersWithFreeWins.Add(betterPlayers[i]);
            playersWithFreeWins.Add(dummyPlayer);
            var playerIndex = i * 2;
            playersWithFreeWins.Add(weekerPlayers[playerIndex]);
            playersWithFreeWins.Add(weekerPlayers[playerIndex + 1]);
        }

        var round = Round.Create(playersWithFreeWins);

        var rounds = new List<Round> { round };
        foreach (var remainingRound in remainingRounds)
        {
            var dummyPlayers = new List<PlayerInLeague>();
            for (int i = 0; i < remainingRound; i++)
            {
                var dummyPlayer1 = PlayerInLeague.Create(ObjectId.GenerateNewId());
                dummyPlayers.Add(dummyPlayer1);
            }
            
            var item = Round.Create(dummyPlayers);
            rounds.Add(item);
        }

        var playoff = new Playoff
        {
            Season = season,
            Rounds = rounds
        };
            
        var freeWins = round.Matchups.Where(m => m.Player2 == ObjectId.Empty);
        foreach (var freeWin in freeWins)
        {
            await playoff.ReportGame(mmrRepository, new MatchResultDto
            {
                MatchId = freeWin.Id,
                Player1 = new PlayerResultDto
                {
                    Id = freeWin.Player1,
                    VictoryPoints = 4500
                },
                Player2 = new PlayerResultDto
                {
                    Id = freeWin.Player2,
                    VictoryPoints = 0
                },
                SecondaryObjective = SecondaryObjectiveState.player1
            }, Mmr.Create(), Mmr.Create(), null, null);
        }
            
        return playoff;
    }

    private static PlayerInLeague BetterOf(List<League> leagues, int league, int place)
    {
        var offset = (league - 1) * 2;
        var playerA = leagues[offset].Players[place - 1];
        var playerB = leagues[offset + 1].Players[place - 1];
        if (playerA.BattlePoints == playerB.BattlePoints)
        {

            return playerA.VictoryPoints > playerB.VictoryPoints ? playerA : playerB;
        }

        return playerA.BattlePoints > playerB.BattlePoints ? playerA : playerB;
    }
        
    private static PlayerInLeague WeekerOf(List<League> leagues, int league, int place)
    {
        var offset = (league - 1) * 2;
        var playerA = leagues[offset].Players[place - 1];
        var playerB = leagues[offset + 1].Players[place - 1];;
        if (playerA.BattlePoints == playerB.BattlePoints)
        {

            return playerA.VictoryPoints < playerB.VictoryPoints ? playerA : playerB;
        }

        return playerA.BattlePoints < playerB.BattlePoints ? playerA : playerB;
    }

    public void SetDeadline(List<DateTime> dateTimes)
    {
        if (dateTimes.Count == Rounds.Count)
        {
            for (int i = 0; i < Rounds.Count; i++)
            {
                Rounds[i].DeadLine = dateTimes[i];
            }
        }
    }

    public async Task<MatchResult> ReportGame(
        IMmrRepository mmrRepository,
        MatchResultDto matchResultDto,
        Mmr player1Mmr,
        Mmr player2Mmr,
        GameList player1List,
        GameList player2List)
    {
        var roundIndex = Rounds.FindIndex(r => r.Matchups.Any(m => m.Id == matchResultDto.MatchId));
        var round = Rounds[roundIndex];
        var matchIndex = round.Matchups.FindIndex(m => m.Id == matchResultDto.MatchId);

        var match = round.Matchups[matchIndex];

        var result = await MatchResult.CreateKoResult(
            mmrRepository,
            matchResultDto.SecondaryObjective, 
            player1Mmr,
            player2Mmr,
            matchResultDto.Player1, 
            matchResultDto.Player2, 
            player1List, 
            player2List, 
            matchResultDto.WasDefLoss);
        match.RecordResult(result);

        var otherMatchIndex = matchIndex % 2 == 0 ? matchIndex + 1 : matchIndex - 1;

        // except finale
        if (round.Matchups.Count > 1)
        {
            var otherMatchuP = round.Matchups[otherMatchIndex];

            if (otherMatchIndex < matchIndex)
            {
                var playerInLeague1 = PlayerInLeague.Create(otherMatchuP.Result?.Winner ?? ObjectId.GenerateNewId());
                var playerInLeague2 = PlayerInLeague.Create(match.Result?.Winner ?? ObjectId.GenerateNewId());
                var matchup = Matchup.CreateForPlayoff(Rounds[roundIndex + 1].Matchups[otherMatchIndex / 2].Id, playerInLeague1, playerInLeague2);

                Rounds[roundIndex + 1].Matchups[otherMatchIndex / 2] = matchup;
            }
            else
            {
                var playerInLeague1 = PlayerInLeague.Create(otherMatchuP.Result?.Winner ?? ObjectId.GenerateNewId());
                var playerInLeague2 = PlayerInLeague.Create(match.Result?.Winner ?? ObjectId.GenerateNewId());
                var matchup = Matchup.CreateForPlayoff(Rounds[roundIndex + 1].Matchups[matchIndex / 2].Id, playerInLeague2, playerInLeague1);

                Rounds[roundIndex + 1].Matchups[matchIndex / 2] = matchup;
            }
        }
           
        return result;
    }

    public void DeleteGameReport(ObjectId matchupId)
    {
        var roundIndex = Rounds.FindIndex(r => r.Matchups.Any(m => m.Id == matchupId));
        var round = Rounds[roundIndex];
        var matchIndex = round.Matchups.FindIndex(m => m.Id == matchupId);
        if (Rounds.Count == roundIndex - 1) return;

        var nextRound = Rounds[roundIndex + 1];
        var nextMatch = nextRound.Matchups[matchIndex / 2];
        var match = round.Matchups[matchIndex];

        if (match.Result.Winner == nextMatch.Player1)
        {
            nextMatch.Player1 = ObjectId.GenerateNewId();
        }

        if (match.Result.Winner == nextMatch.Player2)
        {
            nextMatch.Player2 = ObjectId.GenerateNewId();
        }

        match.DeleteResult();
    }
}