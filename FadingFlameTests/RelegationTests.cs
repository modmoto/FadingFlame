using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FadingFlame.Admin;
using FadingFlame.Leagues;
using FadingFlame.Lists;
using FadingFlame.Matchups;
using FadingFlame.Players;
using Moq;
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
        private int _nextSeason = 1;
        private int _currentSeason = 0;
        private MatchupRepository _matchupRepository;

        [SetUp]
        public void Setup()
        {
            _listRepository = new ListRepository(MongoClient);
            _playerRepository = new PlayerRepository(MongoClient, _listRepository);
            _matchupRepository = new MatchupRepository(MongoClient);
            _leagueRepository = new LeagueRepository(MongoClient, _matchupRepository);
            _seasonRepository = new SeasonRepository(MongoClient);
        }

        [Test]
        public async Task CreateRelegationWorks()
        {
            var leagueCreationService = CreateLeagueService();

            await CreateDefaultLeaguesAndPlayers();
            await leagueCreationService.CreateRelegations();
            var leaguesForSeason = await _leagueRepository.LoadForSeason(_currentSeason);

            foreach (var league in leaguesForSeason.Take(leaguesForSeason.Count - 2))
            {
                Assert.IsNotEmpty(league.RelegationMatches);
            }

            Assert.IsEmpty(leaguesForSeason[leaguesForSeason.Count - 2].RelegationMatches);
            Assert.IsEmpty(leaguesForSeason[leaguesForSeason.Count - 1].RelegationMatches);
        }

        [Test]
        public async Task CreateUpAndDown()
        {
            var leagueCreationService = CreateLeagueService();

            await CreateDefaultLeaguesAndPlayers();
            await leagueCreationService.CreateRelegations();
            await FinishRelegations();

            await leagueCreationService.MakePromotionsAndDemotions();

            var leaguesInSeason = await _leagueRepository.LoadForSeason(_nextSeason);

            Assert.IsNotEmpty(leaguesInSeason);
        }

        private async Task FinishRelegations()
        {
            var leaguesForSeason = await _leagueRepository.LoadForSeason(0);
            foreach (var league in leaguesForSeason)
            {
                var leagueRelegationMatches = league.RelegationMatches;
                foreach (var relegationMatch in leagueRelegationMatches)
                {
                    var result = await MatchResult.CreateKoResult(TestUtils.MmrRepositoryMock(), SecondaryObjectiveState.draw, Mmr.Create(),
                        Mmr.Create(), new PlayerResultDto
                        {
                            Id = relegationMatch.Player1,
                            VictoryPoints = 3500
                        }, new PlayerResultDto
                        {
                            Id = relegationMatch.Player2,
                            VictoryPoints = 1000
                        }, GameList.DeffLoss(), GameList.DeffLoss());
                    relegationMatch.RecordResult(result);
                    await _matchupRepository.UpdateMatch(relegationMatch);
                }
            }
        }

        private async Task CreateDefaultLeaguesAndPlayers()
        {
            await _seasonRepository.Update(new Season { SeasonId = _currentSeason, StartDate = DateTime.Now });
            await _seasonRepository.Update(new Season { SeasonId = _nextSeason, StartDate = DateTime.Now });

            for (int i = 0; i < 20; i++)
            {
                var league = League.Create(_currentSeason, DateTime.Now, LeagueConstants.Ids[i], LeagueConstants.Names[i]);
                for (int j = 0; j < 6; j++)
                {
                    var player = Player.Create($"layer{i}_{j}", $"test{i}_{j}@lol.de");
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