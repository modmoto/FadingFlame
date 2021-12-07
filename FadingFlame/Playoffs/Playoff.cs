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

namespace FadingFlame.Playoffs
{
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
            var weekerPlayers = new List<PlayerInLeague>();
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

            // lower leagues
            weekerPlayers.Add(BetterOf(leagues, 6, 1));
            weekerPlayers.Add(WeekerOf(leagues, 6, 1));
            
            weekerPlayers.Add(WeekerOf(leagues, 2, 2));
            weekerPlayers.Add(leagues[18].Players[0]);
            
            weekerPlayers.Add(WeekerOf(leagues, 4, 1));
            weekerPlayers.Add(BetterOf(leagues, 8, 1));
            
            weekerPlayers.Add(BetterOf(leagues, 4, 1));
            weekerPlayers.Add(WeekerOf(leagues, 8, 1));
            
            weekerPlayers.Add(WeekerOf(leagues, 5, 1));
            weekerPlayers.Add(BetterOf(leagues, 7, 1));
            
            weekerPlayers.Add(BetterOf(leagues, 3, 1));
            weekerPlayers.Add(WeekerOf(leagues, 9, 1));
            
            weekerPlayers.Add(BetterOf(leagues, 5, 1));
            weekerPlayers.Add(WeekerOf(leagues, 7, 1));
            
            weekerPlayers.Add(WeekerOf(leagues, 3, 1));
            weekerPlayers.Add(BetterOf(leagues, 9, 1));
            
            var playersWithFreeWins = new List<PlayerInLeague>();
            var playersCount = weekerPlayers.Count + betterPlayers.Count;
            var remainingRounds = NormalRounds.Where(r => r < playersCount).ToList();
            var roundIndex = remainingRounds.First();
            var gamesTooMuch = playersCount - roundIndex;
            var remainingGames = gamesTooMuch * 2;
            var freeWinCounter = playersCount - remainingGames;
            for (int i = 0; i < freeWinCounter; i++)
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
                player2List);
            match.RecordResult(result);

            var otherMatchIndex = matchIndex % 2 == 0 ? matchIndex + 1 : matchIndex - 1;

            var otherMatchuP = round.Matchups[otherMatchIndex];

            if (otherMatchIndex < matchIndex)
            {
                var playerInLeague1 = PlayerInLeague.Create(otherMatchuP.Result?.Winner ?? ObjectId.GenerateNewId());
                var playerInLeague2 = PlayerInLeague.Create(match.Result?.Winner ?? ObjectId.GenerateNewId());
                var matchup = Matchup.CreateForPlayoff(playerInLeague1, playerInLeague2);

                Rounds[roundIndex + 1].Matchups[otherMatchIndex / 2] = matchup;
            }
            else
            {
                var playerInLeague1 = PlayerInLeague.Create(otherMatchuP.Result?.Winner ?? ObjectId.GenerateNewId());
                var playerInLeague2 = PlayerInLeague.Create(match.Result?.Winner ?? ObjectId.GenerateNewId());
                var matchup = Matchup.CreateForPlayoff(playerInLeague2, playerInLeague1);

                Rounds[roundIndex + 1].Matchups[matchIndex / 2] = matchup;
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
}