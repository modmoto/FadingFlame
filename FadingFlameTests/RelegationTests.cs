using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FadingFlame.Admin;
using FadingFlame.Leagues;
using FadingFlame.Lists;
using FadingFlame.Matchups;
using FadingFlame.Players;
using FluentAssertions;
using MongoDB.Bson;
using NUnit.Framework;

namespace FadingFlameTests
{
    [TestFixture]
    public class RelegationTests : IntegrationTestBase
    {
        private ListRepository _listRepository;
        private PlayerRepository _playerRepository;
        private LeagueRepository _leagueRepository;
        private SeasonRepository _seasonRepository;
        private int _season = 1;

        [SetUp]
        public void Setup()
        {
            _listRepository = new ListRepository(MongoClient);
            _playerRepository = new PlayerRepository(MongoClient, _listRepository);
            _leagueRepository = new LeagueRepository(MongoClient, new MatchupRepository(MongoClient));
            _seasonRepository = new SeasonRepository(MongoClient);
        }

        [Test]
        public async Task CreateRelegationWorks()
        {
            var leagueCreationService = CreateLeagueService();


            await CreateDefaultLeaguesAndPlayers();
            await leagueCreationService.CreateRelegations();
            var leaguesForSeason = await _leagueRepository.LoadForSeason(0);

            foreach (var league in leaguesForSeason)
            {
                Assert.IsNotEmpty(league.RelegationMatches);
            }
        }

        private async Task CreateDefaultLeaguesAndPlayers()
        {
            await _seasonRepository.Update(new Season { SeasonId = 0});
            await _seasonRepository.Update(new Season { SeasonId = 1});

            for (int i = 0; i < 12; i++)
            {
                var league = League.Create(_season, DateTime.Now, LeagueConstants.Ids[i], LeagueConstants.Names[i]);
                for (int j = 0; j < 6; j++)
                {
                    var player = Player.Create($"layer{j}", $"test{j}@lol.de");
                    await _playerRepository.Insert(player);
                    league.AddPlayer(player);
                }

                await _leagueRepository.Insert(new List<League> { league });
            }
        }

        private LeagueCreationService CreateLeagueService()
        {
            return new LeagueCreationService(_seasonRepository, _leagueRepository, _playerRepository);
        }
    }
}