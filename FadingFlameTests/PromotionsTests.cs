using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FadingFlame.Admin;
using FadingFlame.Leagues;
using FadingFlame.Lists;
using FadingFlame.Matchups;
using FadingFlame.Players;
using MongoDB.Bson;
using NUnit.Framework;

namespace FadingFlameTests
{
    [TestFixture]
    public class PromotionsTests : IntegrationTestBase
    {
        private ListRepository _listRepository;
        private PlayerRepository _playerRepository;
        private LeagueRepository _leagueRepository;
        private SeasonRepository _seasonRepository;
        private int _nextSeason = 1;
        private int _currentSeason = 0;
        private MatchupRepository _matchupRepository;

        [SetUp]
        public void SetupInner()
        {
            _listRepository = new ListRepository(MongoClient);
            _playerRepository = new PlayerRepository(MongoClient, _listRepository);
            _matchupRepository = new MatchupRepository(MongoClient);
            _leagueRepository = new LeagueRepository(MongoClient, _matchupRepository);
            _seasonRepository = new SeasonRepository(MongoClient);
        }

        [Test]
        public async Task CreatePromotionWorks()
        {
            var leagueCreationService = CreateLeagueService();

            await CreateDefaultLeaguesAndPlayers(20);
            await leagueCreationService.CreatePromotions();
            var leaguesForSeason = await _leagueRepository.LoadForSeason(_currentSeason);

            foreach (var league in leaguesForSeason.Take(leaguesForSeason.Count - 2))
            {
                Assert.IsNotEmpty(league.PromotionMatches);
            }

            Assert.IsEmpty(leaguesForSeason[^1].PromotionMatches);
            Assert.IsEmpty(leaguesForSeason[^2].PromotionMatches);
        }

        [Test]
        [Ignore("make better")]
        public async Task CreateUpAndDown_UnevenLeagues()
        {
            var leagueCreationService = CreateLeagueService();

            await CreateDefaultLeaguesAndPlayers(19);
            await leagueCreationService.CreatePromotions();
            await FinishPromotions();

            await leagueCreationService.MakePromotionsAndDemotions();

            var leaguesInSeason = await _leagueRepository.LoadForSeason(_nextSeason);

            Assert.IsNotEmpty(leaguesInSeason);
            foreach (var league in leaguesInSeason)
            {
                Assert.AreEqual(6, league.Players.Count);
                Assert.AreEqual(5, league.GameDays.Count);
                foreach (var gameDay in league.GameDays)
                {
                    Assert.AreEqual(3, gameDay.Matchups.Count);
                }
            }
        }

        [Test]
        [Ignore("just bad rn")]
        public async Task CreateUpAndDown()
        {
            var leagueCreationService = CreateLeagueService();

            await CreateDefaultLeaguesAndPlayers(20);
            await leagueCreationService.CreatePromotions();
            await FinishPromotions();

            await leagueCreationService.MakePromotionsAndDemotions();

            var leaguesInSeason = await _leagueRepository.LoadForSeason(_nextSeason);

            Assert.IsNotEmpty(leaguesInSeason);
            foreach (var league in leaguesInSeason)
            {
                Assert.AreEqual(6, league.Players.Count);
                Assert.AreEqual(5, league.GameDays.Count);
                foreach (var gameDay in league.GameDays)
                {
                    Assert.AreEqual(3, gameDay.Matchups.Count);
                }
            }

            var seasons = await _seasonRepository.LoadSeasons();
            Assert.AreEqual(3, seasons.Count);
            Assert.AreEqual(2, seasons.First().SeasonId);

            var loadAll = await _playerRepository.LoadAll();
            foreach (var player in loadAll)
            {
                Assert.AreEqual(ObjectId.Empty, player.ArmyIdNextSeason);
                Assert.AreNotEqual(ObjectId.Empty, player.ArmyIdCurrentSeason);
            }
        }

        private async Task FinishPromotions()
        {
            var leaguesForSeason = await _leagueRepository.LoadForSeason(0);
            foreach (var league in leaguesForSeason)
            {
                foreach (var promotionMatch in league.PromotionMatches)
                {
                    var result = await MatchResult.CreateKoResult(TestUtils.MmrRepositoryMock(), SecondaryObjectiveState.draw, Mmr.Create(),
                        Mmr.Create(), new PlayerResultDto
                        {
                            Id = promotionMatch.Player1,
                            VictoryPoints = 3500
                        }, new PlayerResultDto
                        {
                            Id = promotionMatch.Player2,
                            VictoryPoints = 1000
                        }, 
                        GameList.DeffLoss(), 
                        GameList.DeffLoss(), 
                        false);
                    promotionMatch.RecordResult(result);
                    await _matchupRepository.UpdateMatch(promotionMatch);
                }
            }
        }

        private async Task CreateDefaultLeaguesAndPlayers(int until)
        {
            await _seasonRepository.Update(new Season { SeasonId = _currentSeason, StartDate = DateTime.Now });
            await _seasonRepository.Update(new Season { SeasonId = _nextSeason, StartDate = DateTime.Now });

            for (int i = 0; i < until; i++)
            {
                var league = League.Create(_currentSeason, DateTime.Now, LeagueConstants.Ids[i], LeagueConstants.Names[i]);
                for (int j = 0; j < 6; j++)
                {
                    var player = Player.Create($"layer{i}_{j}", $"test{i}_{j}@lol.de");
                    await _playerRepository.Insert(player);
                    player.SubmitListsNextSeason(GameList.DeffLoss(), GameList.DeffLoss(), _nextSeason, 4);
                    await _playerRepository.UpdateWithLists(player);
                    league.AddPlayer(player);
                }
                league.CreateGameDays();

                await _leagueRepository.Insert(new List<League> { league });
            }
        }

        private LeagueCreationService CreateLeagueService()
        {
            return new LeagueCreationService(_seasonRepository, _leagueRepository, _playerRepository, _matchupRepository);
        }
    }
}