using System.Collections.Generic;
using System.Linq;

namespace FadingFlame.Leagues;

public class PlayerPlacementComparer : IComparer<PlayerInLeague>
{
    private readonly League _league;

    public PlayerPlacementComparer(League league)
    {
        _league = league;
    }

    public int Compare(PlayerInLeague a, PlayerInLeague b)
    {
        if (a != null && b != null)
        {
            if (a.BattlePoints != b.BattlePoints)
            {
                return b.BattlePoints - a.BattlePoints;
            }

            var gameBetweenPlayers = _league.GameDays
                .SelectMany(g => g.Matchups)
                .FirstOrDefault(m => m.Player1 == a.Id && m.Player2 == b.Id || m.Player2 == a.Id && m.Player1 == b.Id);
            if (gameBetweenPlayers?.IsFinished == true && gameBetweenPlayers.Result.IsDraw)
            {
                return gameBetweenPlayers.Result.Winner == a.Id ? -1 : 1;
            }

            return b.VictoryPoints - a.VictoryPoints;
        }

        return 0;
    }
}