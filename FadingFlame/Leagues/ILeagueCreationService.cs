using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FadingFlame.Admin;
using FadingFlame.Matchups;
using FadingFlame.Players;
using MongoDB.Bson;

namespace FadingFlame.Leagues
{
    public interface ILeagueCreationService
    {
        Task MakePromotionsAndDemotions();
        Task CreatePromotions();
        Task MakeSeasonOfficial();
        Task CreateEmptyLeagueInCurrentSeason();
    }

    public class LeagueCreationService : ILeagueCreationService
    {
        private readonly ISeasonRepository _seasonRepository;
        private readonly ILeagueRepository _leagueRepository;
        private readonly IPlayerRepository _playerRepository;
        private readonly IMatchupRepository _matchupRepository;

        public LeagueCreationService(
            ISeasonRepository seasonRepository, 
            ILeagueRepository leagueRepository,
            IPlayerRepository playerRepository,
            IMatchupRepository matchupRepository)
        {
            _seasonRepository = seasonRepository;
            _leagueRepository = leagueRepository;
            _playerRepository = playerRepository;
            _matchupRepository = matchupRepository;
        }
        
        public async Task MakePromotionsAndDemotions()
        {
            var seasons = await _seasonRepository.LoadSeasons();
            var nextSeason = seasons[0];
            var currentSeason = seasons[1];
            await _leagueRepository.DeleteForSeason(nextSeason.SeasonId);
            var enrolledPlayers = await _playerRepository.PlayersThatEnrolledInNextSeason();

            var currentLeagues = await _leagueRepository.LoadForSeason(currentSeason.SeasonId);

            var divisionsTemp = new List<List<Player>>();
            var divisionCount = (currentLeagues.Count + 1) / 2;
            for (int i = 0; i < divisionCount; i++)
            {
                divisionsTemp.Add(new List<Player>());
            }

            for (int division = 0; division < divisionCount; division ++)
            {
                var newPlayerRanks = divisionsTemp[division];
                var leagueIndex = division * 2;
                var currentLeagueA = currentLeagues[leagueIndex];
                var currentLeagueB = currentLeagues.Count > leagueIndex + 1 ? currentLeagues[leagueIndex + 1] : null;
                var oneLeagueDownA = currentLeagues.Count > leagueIndex + 2 ? currentLeagues[leagueIndex + 2] : null;
                var oneLeagueDownB = currentLeagues.Count > leagueIndex + 3 ? currentLeagues[leagueIndex + 3] : null;

                if (division == divisionsTemp.Count - 2)
                {
                    var isUneven = currentLeagues.Count % 2 != 0;
                    if (isUneven && oneLeagueDownA != null && currentLeagueB != null)
                    {
                        AddIfEnrolled(newPlayerRanks, enrolledPlayers, oneLeagueDownA.Players[1].Id);
                        AddIfEnrolled(newPlayerRanks, enrolledPlayers, currentLeagueB.Players[2].Id);
                    }
                    LeavePlayerInLeague(newPlayerRanks, enrolledPlayers, currentLeagueA, currentLeagueB, 3);
                }

                if (division == divisionsTemp.Count - 1)
                {
                    LeavePlayerInLeague(newPlayerRanks, enrolledPlayers, currentLeagueA, currentLeagueB, 3);
                    LeavePlayerInLeague(newPlayerRanks, enrolledPlayers, currentLeagueA, currentLeagueB, 4);
                    LeavePlayerInLeague(newPlayerRanks, enrolledPlayers, currentLeagueA, currentLeagueB, 5);
                }

                LeavePlayerInLeague(newPlayerRanks, enrolledPlayers, currentLeagueA, currentLeagueB, 2);

                if (oneLeagueDownB == null && oneLeagueDownA == null)
                {
                    break;
                }

                if (division == 0)
                {
                    var newPlayerRanksOneLeaguDown = divisionsTemp[1];

                    LeavePlayerInLeague(newPlayerRanks, enrolledPlayers, currentLeagueA, currentLeagueB, 0);
                    LeavePlayerInLeague(newPlayerRanks, enrolledPlayers, currentLeagueA, currentLeagueB, 1);
                    LeavePlayerInLeague(newPlayerRanks, enrolledPlayers, currentLeagueA, currentLeagueB, 3);

                    MoveFirstPlayerOfOneDownUp(newPlayerRanks, enrolledPlayers, oneLeagueDownA, oneLeagueDownB);
                    MoveLastPlayerDown(newPlayerRanksOneLeaguDown, enrolledPlayers, currentLeagueA, currentLeagueB);

                    SwitchPromotionsOneLeagueDown(newPlayerRanks, enrolledPlayers, newPlayerRanksOneLeaguDown, currentLeagueA, currentLeagueB);
                }

                if (division >= 1)
                {
                    var newPlayerRanksOneLeagueDown = divisionsTemp[division + 1];

                    if (division == 1)
                    {
                        MoveFirstPlayerOfOneDownUp(newPlayerRanks, enrolledPlayers, oneLeagueDownA, oneLeagueDownB);
                    }

                    SwitchPromotionsOneLeagueDown(newPlayerRanks, enrolledPlayers, newPlayerRanksOneLeagueDown, currentLeagueA, currentLeagueB);
                    if (newPlayerRanksOneLeagueDown != null)
                    {
                        SwitchPromotionsTwoLeagueasdDown(newPlayerRanks, enrolledPlayers, newPlayerRanksOneLeagueDown, currentLeagueA, currentLeagueB);
                    }

                    MoveLastPlayerDown(newPlayerRanksOneLeagueDown, enrolledPlayers, currentLeagueA, currentLeagueB);
                }
            }

            var newLeagues = new List<League>();

            var playerIdsFromLastSeason = divisionsTemp.SelectMany(p => p).Select(p => p.Id).ToList();

            var returningPlayersOrderedBySkill = enrolledPlayers
                .Where(p => !playerIdsFromLastSeason.Contains(p.Id))
                .OrderByDescending(RatePlayer).ToList();

            var playerCountPerDivision = League.MaxPlayerCount * 2;
            var newDivisionCountExceptFirstAndSecond = (int) Math.Ceiling((double) enrolledPlayers.Count / League.MaxPlayerCount / 2.0 - 2.0);
            var amountOfFreshPlayersEachDivision = (int) Math.Ceiling((decimal) returningPlayersOrderedBySkill.Count / newDivisionCountExceptFirstAndSecond);
            
            for (int division = 0; division < divisionsTemp.Count; division++)
            {
                var playersForThisDivision = divisionsTemp[division];
                if (playersForThisDivision.Count != playerCountPerDivision)
                {
                    var playerCountMissingFromDivision = playerCountPerDivision - playersForThisDivision.Count;
                    if ((division == 0 || division == 1) && playerCountMissingFromDivision > 0)
                    {
                        MovePlayersUp(division, divisionsTemp, playerCountMissingFromDivision);
                    }
                    else
                    {
                        var isLowestDivision = division == divisionsTemp.Count - 1;
                        if (isLowestDivision)
                        {
                            playersForThisDivision = returningPlayersOrderedBySkill;
                            divisionsTemp[division] = playersForThisDivision;
                        }
                        else
                        {
                            var playersToTakeFromNewPlayers = amountOfFreshPlayersEachDivision > playerCountPerDivision - playersForThisDivision.Count
                                ? playerCountPerDivision - playersForThisDivision.Count
                                : amountOfFreshPlayersEachDivision;
                        
                            var newPlayersThatShouldGoToThisLeague = returningPlayersOrderedBySkill.Take(playersToTakeFromNewPlayers);
                            returningPlayersOrderedBySkill = returningPlayersOrderedBySkill.Skip(playersToTakeFromNewPlayers).ToList();
                            playersForThisDivision.AddRange(newPlayersThatShouldGoToThisLeague);
                            if (division == 5)
                            {
                                var newPlayersThatShouldGoToThisLeague2 = returningPlayersOrderedBySkill.Take(2);
                                returningPlayersOrderedBySkill = returningPlayersOrderedBySkill.Skip(2).ToList();
                                playersForThisDivision.AddRange(newPlayersThatShouldGoToThisLeague2);
                            }
                            
                            // one day, mazbe
                            // var newPlayerCountMissingFromDivision = playerCountPerDivision - playersForThisDivision.Count;
                            // var remainingPlayers = divisionsTemp.Skip(division).Sum(d => d.Count);
                            // while (newPlayerCountMissingFromDivision > 0 
                            //        && remainingPlayers >= 0 
                            //        && newPlayerCountMissingFromDivision < remainingPlayers)
                            // {
                            //     MovePlayersUp(division, divisionsTemp, newPlayerCountMissingFromDivision);
                            //     newPlayerCountMissingFromDivision = playerCountPerDivision - playersForThisDivision.Count;
                            //     remainingPlayers = divisionsTemp.Skip(division).Sum(d => d.Count);
                            // }
                        }
                    }
                }

                if (playersForThisDivision.Count == 0)
                {
                    continue;
                }
                
                playersForThisDivision.Shuffle();
                var leagueIndex = division * 2;
                var leagueA = League.Create(seasons[0].SeasonId, seasons[0].StartDate, LeagueConstants.Ids[leagueIndex], LeagueConstants.Names[leagueIndex]);
                var first6 = playersForThisDivision.Take(League.MaxPlayerCount).ToList();
                foreach (var player in first6)
                {
                    leagueA.AddPlayer(player);
                }

                var leagueB = League.Create(seasons[0].SeasonId, seasons[0].StartDate, LeagueConstants.Ids[leagueIndex + 1], LeagueConstants.Names[leagueIndex + 1]);
                var last6 = playersForThisDivision.Skip(League.MaxPlayerCount).ToList();
                foreach (var player in last6)
                {
                    leagueB.AddPlayer(player);
                }

                newLeagues.Add(leagueA);
                if (last6.Count != 0)
                {
                    newLeagues.Add(leagueB);
                }
            }

            foreach (var newLeague in newLeagues)
            {
                newLeague.CreateGameDays();
            }

            await _leagueRepository.Insert(newLeagues);
            await SetDeploymentsForNextSeason();
        }

        private int RatePlayer(Player player)
        {
            if (Math.Abs(player.Mmr.Rating - Mmr.Create().Rating) < 0.00001)
            {
                return player.SelfAssessment ?? 1;
            }

            return (int)((player.Mmr.Rating - 1000) / 100);
        }

        private static void MovePlayersUp(int division, List<List<Player>> divisionsTemp, int playerCountMissingFromDivision)
        {
            for (int i = division; i < divisionsTemp.Count - 1; i++)
            {
                var freePromotions = divisionsTemp[i + 1].Take(playerCountMissingFromDivision).ToList();
                divisionsTemp[i].AddRange(freePromotions);
                divisionsTemp[i + 1] = divisionsTemp[i + 1].Skip(playerCountMissingFromDivision).ToList();
            }
        }

        public async Task MakeSeasonOfficial()
        {
            var seasons = await _seasonRepository.LoadSeasons();
            var nextSeason = seasons[0];
            
            var newNextSeason = Season.Create(nextSeason.SeasonId + 1);
            await _seasonRepository.Update(newNextSeason);
            
            var enrolledPlayers = await _playerRepository.PlayersThatEnrolledInNextSeason();
            foreach (var player in enrolledPlayers)
            {
                player.Enroll();
            }

            await _playerRepository.Update(enrolledPlayers);
        }

        public async Task CreateEmptyLeagueInCurrentSeason()
        {
            var seasons = await _seasonRepository.LoadSeasons();
            var currentSeason = seasons[1];
            var currentLeagues = await _leagueRepository.LoadForSeason(currentSeason.SeasonId);
            var lowestLeague = currentLeagues.Last();
            var nameIndex = LeagueConstants.Names.IndexOf(lowestLeague.Name);
            var nextLeagueName = LeagueConstants.Names[nameIndex + 1];
            var idIndex = LeagueConstants.Ids.IndexOf(lowestLeague.DivisionId);
            var nextLeagueId = LeagueConstants.Ids[idIndex + 1];
            var league = League.Create(currentSeason.SeasonId, lowestLeague.StartDate, nextLeagueName, nextLeagueId);
            for (int i = 0; i < lowestLeague.Players.Count; i++)
            {
                var player = Player.Create($"DummyPlayer_{i}", $"DummyMail_{i}");
                player.Id = ObjectId.GenerateNewId();
                league.AddPlayer(player);
            }
            league.CreateGameDays();
            for (var i = 0; i < league.GameDays.Count; i++)
            {
                league.GameDays[i].Deployment = lowestLeague.GameDays[i].Deployment;
                league.GameDays[i].SecondaryObjective = lowestLeague.GameDays[i].SecondaryObjective;
            }
            
            await _leagueRepository.Insert(new List<League> { league });
        }

        private void MoveFirstPlayerOfOneDownUp(List<Player> newPlayerRanks, List<Player> playersEnrolled, League oneLeagueDownA, League oneLeagueDownB)
        {
            AddIfEnrolled(newPlayerRanks, playersEnrolled, oneLeagueDownA.Players[0].Id);
            AddIfEnrolled(newPlayerRanks, playersEnrolled, oneLeagueDownB.Players[0].Id);
        }

        private void SwitchPromotionsOneLeagueDown(List<Player> newPlayerRanks,
            List<Player> playersEnrolled,
            List<Player> newPlayerRanksOneLeaguDown,
            League currentLeagueA,
            League currentLeagueB)
        {
            AddIfEnrolled(newPlayerRanks, playersEnrolled, currentLeagueA.PromotionMatchOverOneLeague.Result.Winner);
            AddIfEnrolled(newPlayerRanksOneLeaguDown, playersEnrolled, currentLeagueA.PromotionMatchOverOneLeague.Result.Looser);

            if (currentLeagueB != null)
            {
                if (currentLeagueB.PromotionMatchOverOneLeague == null)
                {
                    AddIfEnrolled(newPlayerRanks, playersEnrolled, currentLeagueB.Players[4].Id);
                    AddIfEnrolled(newPlayerRanks, playersEnrolled, currentLeagueB.Players[5].Id);
                }
                else
                {
                    AddIfEnrolled(newPlayerRanks, playersEnrolled, currentLeagueB.PromotionMatchOverOneLeague.Result.Winner);
                    AddIfEnrolled(newPlayerRanksOneLeaguDown, playersEnrolled, currentLeagueB.PromotionMatchOverOneLeague.Result.Looser);   
                } 
            }
        }
        
        private void SwitchPromotionsTwoLeagueasdDown(
            List<Player> newPlayerRanks,
            List<Player> playersEnrolled,
            List<Player> newPlayerRanksOneLeaguDown,
            League currentLeagueA,
            League currentLeagueB)
        {
            if (currentLeagueA != null)
            {
                AddIfEnrolled(newPlayerRanks, playersEnrolled, currentLeagueA.PromotionMatchOverTwoLeagues?.Result.Winner ?? ObjectId.Empty);
                AddIfEnrolled(newPlayerRanksOneLeaguDown, playersEnrolled, currentLeagueA.PromotionMatchOverTwoLeagues?.Result.Looser ?? ObjectId.Empty);    
            }

            if (currentLeagueB != null)
            {
                if (currentLeagueB.PromotionMatchOverTwoLeagues == null)
                {
                    AddIfEnrolled(newPlayerRanks, playersEnrolled, currentLeagueB.Players[3].Id);
                }
                else
                {
                    AddIfEnrolled(newPlayerRanks, playersEnrolled, currentLeagueB.PromotionMatchOverTwoLeagues.Result.Winner);
                    AddIfEnrolled(newPlayerRanksOneLeaguDown, playersEnrolled, currentLeagueB.PromotionMatchOverTwoLeagues.Result.Looser);    
                }
            }
        }

        private void LeavePlayerInLeague(
            List<Player> newPlayerRanks, 
            List<Player> playersEnrolled,
            League currentLeagueA,
            League currentLeagueB,
            int index)
        {
            if (index >= currentLeagueA.Players.Count) 
                return;
            
            AddIfEnrolled(newPlayerRanks, playersEnrolled, currentLeagueA.Players[index].Id);
            
            if (index >= currentLeagueB?.Players.Count) 
                return;
            
            AddIfEnrolled(newPlayerRanks, playersEnrolled, currentLeagueB?.Players[index].Id ?? ObjectId.Empty);
        }

        private void MoveLastPlayerDown(List<Player> newPlayerRanksOneLeaguDown, List<Player> playersEnrolled, League currentLeagueA,
            League currentLeagueB)
        {
            AddIfEnrolled(newPlayerRanksOneLeaguDown, playersEnrolled, currentLeagueA.Players[5].Id);
            AddIfEnrolled(newPlayerRanksOneLeaguDown, playersEnrolled, currentLeagueB.Players[5].Id);
        }

        private void AddIfEnrolled(List<Player> newPlayerRanks, List<Player> playersEnrolled, ObjectId playerId)
        {
            var enrolledPlayer = playersEnrolled.FirstOrDefault(p => p.Id == playerId);
            if (enrolledPlayer != null)
            {
                if (newPlayerRanks.All(p => p.Id != enrolledPlayer.Id))
                {
                    newPlayerRanks.Add(enrolledPlayer);
                }
            }
        }

        private async Task SetDeploymentsForNextSeason()
        {
            var seasons = await _seasonRepository.LoadSeasons();
            var currentSeason = seasons[0];

            var currentLeagues = await _leagueRepository.LoadForSeason(currentSeason.SeasonId);

            var secondaryObjectives = Enum.GetValues<SecondaryObjective>().Where(r => r != SecondaryObjective.RandomObjective).ToList();
            var deployments = Enum.GetValues<Deployment>().Where(r => r != Deployment.RandomDeployment).ToList();
            secondaryObjectives.Shuffle();
            deployments.Shuffle();

            foreach (var currentLeague in currentLeagues)
            {
                var league = await _leagueRepository.Load(currentLeague.Id);
                league.SetScenarioAndDeployments(secondaryObjectives, deployments);
                await _leagueRepository.Update(league);
            }
        }

        public async Task CreatePromotions()
        {
            var seasons = await _seasonRepository.LoadSeasons();
            var currentSeason = seasons[1];

            var currentLeagues = await _leagueRepository.LoadForSeason(currentSeason.SeasonId);

            var leaguesCount = currentLeagues.Count;
            for (var index = 0; index < leaguesCount; index++)
            {
                var oneLeagueBelow = index + 2 < leaguesCount ? currentLeagues[index + 2] : null;
                var twoLeagueBelow = index + 4 < leaguesCount ? currentLeagues[index + 4] : null;
                var currentLeague = currentLeagues[index];
                var promotionMatches = currentLeague.PromotionMatches;
                if (promotionMatches.Any())
                {
                    await _matchupRepository.DeleteMatches(promotionMatches.Select(r => r.Id).ToList());
                }
                currentLeague.CreatePromotions(oneLeagueBelow, twoLeagueBelow, leaguesCount % 2 != 0);
                await _leagueRepository.Update(currentLeague);
            }
        }
    }
}