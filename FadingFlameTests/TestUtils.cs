using System;
using FadingFlame.Leagues;
using FadingFlame.Players;
using MongoDB.Bson;

namespace FadingFlameTests
{
    public class TestUtils
    {
        public static League CreateLeagueWithPlayers(params ObjectId[] identities)
        {
            var league = League.Create(1, DateTime.Now, "1A", "bogota");
            foreach (var guidIdentity in identities)
            {
                league.AddPlayer(new Player()
                {
                    Id = guidIdentity
                });
            }

            return league;
        }
    }
}