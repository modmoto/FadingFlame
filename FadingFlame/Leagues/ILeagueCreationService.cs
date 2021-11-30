using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FadingFlame.Admin;
using FadingFlame.Players;
using MongoDB.Bson;

namespace FadingFlame.Leagues
{
    public interface ILeagueCreationService
    {
        Task MakePromotionsAndDemotions();
        Task CreateRelegations();
    }

    public class LeagueCreationService : ILeagueCreationService
    {
        private readonly ISeasonRepository _seasonRepository;
        private readonly ILeagueRepository _leagueRepository;
        private readonly IPlayerRepository _playerRepository;
        
        public LeagueCreationService(
            ISeasonRepository seasonRepository, 
            ILeagueRepository leagueRepository,
            IPlayerRepository playerRepository)
        {
            _seasonRepository = seasonRepository;
            _leagueRepository = leagueRepository;
            _playerRepository = playerRepository;
        }
        
        public async Task MakePromotionsAndDemotions()
        {
            var seasons = await _seasonRepository.LoadSeasons();
            var nextSeason = seasons[0];
            var currentSeason = seasons[1];
            await _leagueRepository.DeleteForSeason(nextSeason.SeasonId);
            var playersEnrolled = await _playerRepository.LoadAll();
            // var playersEnrolled = await _playerRepository.PlayersThatEnlistedInNextSeason();

            var currentLeagues = await _leagueRepository.LoadForSeason(currentSeason.SeasonId);

            var leagues = new List<List<Player>>();
            for (int i = 0; i < (currentLeagues.Count + 1) / 2; i++)
            {
                leagues.Add(new List<Player>());
            }

            var isUneven = currentLeagues.Count % 2 != 0;

            for (int i = 0; i < currentLeagues.Count; i += 2)
            {
                var leagueIndex = i / 2;
                var newPlayerRanks = leagues[leagueIndex];
                var currentLeagueA = currentLeagues[i];
                var currentLeagueB = currentLeagues.Count > i + 1 ? currentLeagues[i + 1] : null;
                var oneLeagueDownA = currentLeagues.Count > i + 2 ? currentLeagues[i + 2] : null;
                var oneLeagueDownB = currentLeagues.Count > i + 3 ? currentLeagues[i + 3] : null;

                if (leagueIndex == leagues.Count - 2)
                {
                    if (isUneven && oneLeagueDownA != null && currentLeagueB != null)
                    {
                        AddIfEnrolled(newPlayerRanks, playersEnrolled, oneLeagueDownA.Players[1].Id);
                        AddIfEnrolled(newPlayerRanks, playersEnrolled, currentLeagueB.Players[2].Id);
                    }
                    LeavePlayerInLeague(newPlayerRanks, playersEnrolled, currentLeagueA, currentLeagueB, 3);
                }
                    
                if (leagueIndex == leagues.Count - 1)
                {
                    if (isUneven)
                    {
                        currentLeagueB = currentLeagueA;
                    }

                    LeavePlayerInLeague(newPlayerRanks, playersEnrolled, currentLeagueA, currentLeagueB, 3);
                    LeavePlayerInLeague(newPlayerRanks, playersEnrolled, currentLeagueA, currentLeagueB, 4);
                    LeavePlayerInLeague(newPlayerRanks, playersEnrolled, currentLeagueA, currentLeagueB, 5);
                }
                
                LeavePlayerInLeague(newPlayerRanks, playersEnrolled, currentLeagueA, currentLeagueB, 2);

                if (oneLeagueDownB == null && oneLeagueDownA == null)
                {
                    break;
                }
                
                if (oneLeagueDownB == null)
                {
                    oneLeagueDownB = oneLeagueDownA;
                }
                
                if (leagueIndex == 0)
                {
                    var newPlayerRanksOneLeaguDown = leagues[1];
                    
                    LeavePlayerInLeague(newPlayerRanks, playersEnrolled, currentLeagueA, currentLeagueB, 0);
                    LeavePlayerInLeague(newPlayerRanks, playersEnrolled, currentLeagueA, currentLeagueB, 1);
                    LeavePlayerInLeague(newPlayerRanks, playersEnrolled, currentLeagueA, currentLeagueB, 3);

                    MoveFirstPlayerOfOneDownUp(newPlayerRanks, playersEnrolled, oneLeagueDownA, oneLeagueDownB);
                    MoveLastPlayerDown(newPlayerRanksOneLeaguDown, playersEnrolled, currentLeagueA, currentLeagueB);

                    SwitchRelegationsOneLeagueDown(newPlayerRanks, playersEnrolled, newPlayerRanksOneLeaguDown, currentLeagueA, currentLeagueB);
                }
                
                if (leagueIndex >= 1)
                {
                    var oneLeagueDown = (i + 2) / 2;
                    var newPlayerRanksOneLeagueDown = leagues[oneLeagueDown];

                    if (leagueIndex == 1)
                    {
                        MoveFirstPlayerOfOneDownUp(newPlayerRanks, playersEnrolled, oneLeagueDownA, oneLeagueDownB);
                    }
                    
                    SwitchRelegationsOneLeagueDown(newPlayerRanks, playersEnrolled, newPlayerRanksOneLeagueDown, currentLeagueA, currentLeagueB);
                    if (newPlayerRanksOneLeagueDown != null)
                    {
                        SwitchRelegationsTwoLeagueasdDown(newPlayerRanks, playersEnrolled, newPlayerRanksOneLeagueDown, currentLeagueA, currentLeagueB);    
                    }
                    
                    MoveLastPlayerDown(newPlayerRanksOneLeagueDown, playersEnrolled, currentLeagueA, currentLeagueB);
                }
            }

            var newLeagues = new List<League>();
            for (int i = 0; i < leagues.Count; i++)
            {
                var players = leagues[i];
                players.Shuffle();
                var leagueA = League.Create(seasons[0].SeasonId, seasons[0].StartDate, LeagueConstants.Ids[i * 2], LeagueConstants.Names[i * 2]);
                var first6 = players.Take(6).ToList();
                foreach (var player in first6)
                {
                    leagueA.AddPlayer(player);
                }

                var leagueB = League.Create(seasons[0].SeasonId, seasons[0].StartDate, LeagueConstants.Ids[i * 2 + 1], LeagueConstants.Names[i * 2 + 1]);
                var last6 = players.Skip(6).ToList();
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

            await _leagueRepository.Insert(newLeagues);
            await SetDeploymentsForNextSeason();
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

            var secondaryObjectives = Enum.GetValues<SecondaryObjective>().ToList();
            var deployments = Enum.GetValues<Deployment>().ToList();
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
                currentLeague.CreateRelegations(oneLeagueBelow, twoLeagueBelow, leaguesCount % 2 != 0);
                await _leagueRepository.Update(currentLeague);
            }
        }
    }
}