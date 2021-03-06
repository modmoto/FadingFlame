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
        Task CreateRelegations();
        Task MakeSeasonOfficial();
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

                    SwitchRelegationsOneLeagueDown(newPlayerRanks, enrolledPlayers, newPlayerRanksOneLeaguDown, currentLeagueA, currentLeagueB);
                }

                if (division >= 1)
                {
                    var newPlayerRanksOneLeagueDown = divisionsTemp[division + 1];

                    if (division == 1)
                    {
                        MoveFirstPlayerOfOneDownUp(newPlayerRanks, enrolledPlayers, oneLeagueDownA, oneLeagueDownB);
                    }

                    SwitchRelegationsOneLeagueDown(newPlayerRanks, enrolledPlayers, newPlayerRanksOneLeagueDown, currentLeagueA, currentLeagueB);
                    if (newPlayerRanksOneLeagueDown != null)
                    {
                        SwitchRelegationsTwoLeagueasdDown(newPlayerRanks, enrolledPlayers, newPlayerRanksOneLeagueDown, currentLeagueA, currentLeagueB);
                    }

                    MoveLastPlayerDown(newPlayerRanksOneLeagueDown, enrolledPlayers, currentLeagueA, currentLeagueB);
                }
            }

            var newLeagues = new List<League>();
            var playerIdsFromLastSeason = divisionsTemp.SelectMany(p => p).Select(p => p.Id).ToList();
            
            var playersThatNotParticipatedLastSeason = enrolledPlayers.Where(p => !playerIdsFromLastSeason.Contains(p.Id))
                .OrderByDescending(p => p.SelfAssessment ?? (int) ((p.Mmr.Rating - 1000 ) / 100)).ToList();
            var playerCountPerDivision = League.MaxPlayerCount * 2;
            var newDivisionCountExceptFirstAndSecond = enrolledPlayers.Count / League.MaxPlayerCount / 2 - 2;
            var amountOfFreshPLayersEachLeague = (int) Math.Ceiling((decimal) playersThatNotParticipatedLastSeason.Count / newDivisionCountExceptFirstAndSecond);
            
            for (int division = 0; division < divisionsTemp.Count; division++)
            {
                var players = divisionsTemp[division];
                if (players.Count != playerCountPerDivision)
                {
                    var playerCountMissingFromDivision = playerCountPerDivision - players.Count;
                    if ((division == 0 || division == 1) && playerCountMissingFromDivision > 0)
                    {
                        MovePlayersUp(division, divisionsTemp, playerCountMissingFromDivision);
                    }
                    else
                    {
                        var playersToTakeFromNewPlayers = amountOfFreshPLayersEachLeague > playerCountPerDivision - players.Count
                                ? playerCountPerDivision - players.Count
                                : amountOfFreshPLayersEachLeague;
                        
                        var playersThatShouldGoToThisLeague = playersThatNotParticipatedLastSeason.Take(playersToTakeFromNewPlayers);
                        playersThatNotParticipatedLastSeason = playersThatNotParticipatedLastSeason.Skip(playersToTakeFromNewPlayers).ToList();
                        players.AddRange(playersThatShouldGoToThisLeague);
                        
                        var newPlayerCountMissingFromDivision = playerCountPerDivision - players.Count;
                        var remainingPlayers = divisionsTemp.Skip(division).Sum(d => d.Count);
                        while (newPlayerCountMissingFromDivision > 0 && remainingPlayers >= 0 && newPlayerCountMissingFromDivision < remainingPlayers)
                        {
                            MovePlayersUp(division, divisionsTemp, newPlayerCountMissingFromDivision);
                            newPlayerCountMissingFromDivision = playerCountPerDivision - players.Count;
                            remainingPlayers = divisionsTemp.Skip(division).Sum(d => d.Count);
                        }
                    }
                }

                if (players.Count == 0)
                {
                    continue;
                }
                
                players.Shuffle();
                var leagueIndex = division * 2;
                var leagueA = League.Create(seasons[0].SeasonId, seasons[0].StartDate, LeagueConstants.Ids[leagueIndex], LeagueConstants.Names[leagueIndex]);
                var first6 = players.Take(League.MaxPlayerCount).ToList();
                foreach (var player in first6)
                {
                    leagueA.AddPlayer(player);
                }

                var leagueB = League.Create(seasons[0].SeasonId, seasons[0].StartDate, LeagueConstants.Ids[leagueIndex + 1], LeagueConstants.Names[leagueIndex + 1]);
                var last6 = players.Skip(League.MaxPlayerCount).ToList();
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

        private void MoveFirstPlayerOfOneDownUp(List<Player> newPlayerRanks, List<Player> playersEnrolled, League oneLeagueDownA, League oneLeagueDownB)
        {
            AddIfEnrolled(newPlayerRanks, playersEnrolled, oneLeagueDownA.Players[0].Id);
            AddIfEnrolled(newPlayerRanks, playersEnrolled, oneLeagueDownB.Players[0].Id);
        }

        private void SwitchRelegationsOneLeagueDown(List<Player> newPlayerRanks,
            List<Player> playersEnrolled,
            List<Player> newPlayerRanksOneLeaguDown,
            League currentLeagueA,
            League currentLeagueB)
        {
            AddIfEnrolled(newPlayerRanks, playersEnrolled, currentLeagueA.RelegationMatchOverOneLeague.Result.Winner);
            AddIfEnrolled(newPlayerRanksOneLeaguDown, playersEnrolled, currentLeagueA.RelegationMatchOverOneLeague.Result.Looser);

            if (currentLeagueB != null)
            {
                if (currentLeagueB.RelegationMatchOverOneLeague == null)
                {
                    AddIfEnrolled(newPlayerRanks, playersEnrolled, currentLeagueB.Players[4].Id);
                    AddIfEnrolled(newPlayerRanks, playersEnrolled, currentLeagueB.Players[5].Id);
                }
                else
                {
                    AddIfEnrolled(newPlayerRanks, playersEnrolled, currentLeagueB.RelegationMatchOverOneLeague.Result.Winner);
                    AddIfEnrolled(newPlayerRanksOneLeaguDown, playersEnrolled, currentLeagueB.RelegationMatchOverOneLeague.Result.Looser);   
                } 
            }
        }
        
        private void SwitchRelegationsTwoLeagueasdDown(
            List<Player> newPlayerRanks,
            List<Player> playersEnrolled,
            List<Player> newPlayerRanksOneLeaguDown,
            League currentLeagueA,
            League currentLeagueB)
        {
            if (currentLeagueA != null)
            {
                AddIfEnrolled(newPlayerRanks, playersEnrolled, currentLeagueA.RelegationMatchOverTwoLeagues?.Result.Winner ?? ObjectId.Empty);
                AddIfEnrolled(newPlayerRanksOneLeaguDown, playersEnrolled, currentLeagueA.RelegationMatchOverTwoLeagues?.Result.Looser ?? ObjectId.Empty);    
            }

            if (currentLeagueB != null)
            {
                if (currentLeagueB.RelegationMatchOverTwoLeagues == null)
                {
                    AddIfEnrolled(newPlayerRanks, playersEnrolled, currentLeagueB.Players[3].Id);
                }
                else
                {
                    AddIfEnrolled(newPlayerRanks, playersEnrolled, currentLeagueB.RelegationMatchOverTwoLeagues.Result.Winner);
                    AddIfEnrolled(newPlayerRanksOneLeaguDown, playersEnrolled, currentLeagueB.RelegationMatchOverTwoLeagues.Result.Looser);    
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
            AddIfEnrolled(newPlayerRanks, playersEnrolled, currentLeagueA.Players[index].Id);
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

        public async Task CreateRelegations()
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
                var relegationMatches = currentLeague.RelegationMatches;
                if (relegationMatches.Any())
                {
                    await _matchupRepository.DeleteMatches(relegationMatches.Select(r => r.Id).ToList());
                }
                currentLeague.CreateRelegations(oneLeagueBelow, twoLeagueBelow, leaguesCount % 2 != 0);
                await _leagueRepository.Update(currentLeague);
            }
        }
    }
}