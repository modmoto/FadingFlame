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
    public class Playoff : IIdentifiable
    {
        private static readonly List<int> NormalRounds = new() { 256, 128, 64, 32, 16, 8, 4, 2 };
        
        [BsonId]
        public ObjectId Id { get; set; }

        public int Season { get; set; }

        public List<Round> Rounds { get; set; }

        public static async Task<Playoff> Create(
            IMmrRepository mmrRepository, 
            int season,
            List<PlayerInLeague> playoffPlayers)
        {
            var playersWithFreeWins = new List<PlayerInLeague>();
            playoffPlayers.Shuffle();
            var remainingRounds = NormalRounds.Where(r => r < playoffPlayers.Count).ToList();
            var roundIndex = remainingRounds.First();
            var gamesTooMuch = playoffPlayers.Count - roundIndex;
            var remainingGames = gamesTooMuch * 2;
            var freeWinCounter = playoffPlayers.Count - remainingGames;
            for (int i = 0; i < freeWinCounter; i++)
            {
                var dummyPlayer = PlayerInLeague.Create(ObjectId.Empty);
                playersWithFreeWins.Add(playoffPlayers[i]);
                playersWithFreeWins.Add(dummyPlayer);
            }

            var lowerBracket = playoffPlayers.TakeLast(remainingGames).ToList();
            playersWithFreeWins.AddRange(lowerBracket);

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
                match.Id);
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