using System;
using System.Collections.Generic;
using FadingFlame.Leagues;
using FadingFlame.Players;
using MongoDB.Bson;
using Moq;

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

        public static IMmrRepository MmrRepositoryMock()
        {
            var mock = new Mock<IMmrRepository>();
            mock.Setup(m => m.UpdateMmrs(It.IsAny<UpdateMmrRequest>())).ReturnsAsync(new List<Mmr>
            {
                Mmr.Create(),
                Mmr.Create()
            });
            return mock.Object;
        }
    }
}