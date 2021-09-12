using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using FadingFlame.Leagues;

namespace FadingFlame.Playoffs
{
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