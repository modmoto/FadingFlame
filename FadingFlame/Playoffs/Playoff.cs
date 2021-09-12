using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using FadingFlame.Leagues;
using FadingFlame.Repositories;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FadingFlame.Playoffs
{
    public class Playoff : IIdentifiable
    {
        private static readonly List<int> _normalRounds = new() { 256, 128, 64, 32, 16, 8, 4, 2 };
        
        [BsonId]
        public ObjectId Id { get; set; }

        public int Season { get; set; }

        public List<Round> Rounds { get; set; }

        public static Playoff Create(int season, List<PlayerInLeague> firstPlaces)
        {
            var playersWithFreeWins = new List<PlayerInLeague>();
            var roundIndex = _normalRounds.First(r => r < firstPlaces.Count);
            var gamesTooMuch = firstPlaces.Count - roundIndex;
            var remainingGames = gamesTooMuch * 2;
            var freeWinCounter = firstPlaces.Count - remainingGames;
            for (int i = 0; i < freeWinCounter; i++)
            {
                var dummyPlayer = PlayerInLeague.Create(ObjectId.Empty);
                playersWithFreeWins.Add(firstPlaces[i]);
                playersWithFreeWins.Add(dummyPlayer);
            }

            var lowerBracket = firstPlaces.TakeLast(remainingGames).ToList();
            playersWithFreeWins.AddRange(lowerBracket);

            var round = Round.Create(playersWithFreeWins);

            var playoff = new Playoff
            {
                Season = season,
                Rounds = new List<Round> { round }
            };
            
            var freeWins = round.Matchups.Where(m => m.Player2 == ObjectId.Empty);
            foreach (var freeWin in freeWins)
            {
                playoff.ReportGame(new MatchResultDto
                {
                    MatchId = freeWin.MatchId,
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
                });
            }
            
            return playoff;
        }

        public void ReportGame(MatchResultDto matchResultDto)
        {
            var match = Rounds.Last().Matchups.SingleOrDefault(m => m.MatchId == matchResultDto.MatchId);
            if (match == null
                || match.Player1 != matchResultDto.Player1.Id
                || match.Player2 != matchResultDto.Player2.Id)
            {
                throw new ValidationException("Match not in this playoffs in this config");
            }

            var result = MatchResult.Create(matchResultDto.SecondaryObjective, matchResultDto.Player1, matchResultDto.Player2);
            match.RecordResult(result);
        }

        public void AdvanceToNextStage()
        {
            var advanceToNextStage = Rounds.Last().AdvanceToNextStage();
            Rounds.Add(advanceToNextStage);
        }
    }

    public class Round
    {
        public List<Matchup> Matchups { get; set; }

        public static Round Create(List<PlayerInLeague> playerIds)
        {
            var matchups = new List<Matchup>();

            for (var index = 0; index < playerIds.Count; index += 2)
            {
                var matchup = Matchup.Create(playerIds[index], playerIds[index + 1]);
                matchups.Add(matchup);
            }

            return new Round
            {
                Matchups = matchups
            };
        }

        public Round AdvanceToNextStage()
        {
            if (Matchups.Any(m => !m.IsFinished))
            {
                throw new ValidationException("Can not advance to next round as games are still missing");
            }

            var winners = Matchups.Select(m => PlayerInLeague.Create(m.Result.Winner)).ToList();
            return Create(winners);
        }
    }
}