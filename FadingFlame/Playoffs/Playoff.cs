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

        [BsonId]
        public ObjectId Id { get; set; }

        public int Season { get; set; }

        public List<Round> Rounds { get; set; }

        public int RoundCounter { get; set; }

        public static Playoff Create(int season, List<PlayerInLeague> firstPlaces)
        {
            var roundCounter = GetRounds(firstPlaces);
            var rounds = new List<Round>();
            
            rounds.Add(Round.Create(firstPlaces));

            return new Playoff
            {
                Season = season,
                Rounds = rounds,
                RoundCounter = roundCounter
            };
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

        private static int GetRounds(List<PlayerInLeague> firstPlaces)
        {
            switch (firstPlaces.Count)
            {
                case 2: return 1;
                case 4: return 2;
                case 8: return 3;
                case 16: return 4;
                case 32: return 5;
                case 64: return 6;
                case 128: return 7;
                default: throw new ValidationException("To dumb for math");
            }
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