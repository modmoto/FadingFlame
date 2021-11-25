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
        private Random rng = new();
        
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
            for (int i = 0; i < 100; i++)
            {
                leagues.Add(new List<Player>());
            }
            
            for (int i = 0; i < 98; i += 2)
            {
                var newPlayerRanks = new List<Player>();
                var currentLeagueA = currentLeagues[i];
                var currentLeagueB = currentLeagues[i + 1];
                var oneLeagueDownA = currentLeagues[i + 2];
                var oneLeagueDownB = currentLeagues[i + 3];

                LeavePlayerInLeague(newPlayerRanks, playersEnrolled, currentLeagueA, currentLeagueB, 2);

                if (i == 0)
                {
                    var newPlayerRanksOneLeaguDown = leagues[0];
                    
                    LeavePlayerInLeague(newPlayerRanks, playersEnrolled, currentLeagueA, currentLeagueB, 0);
                    LeavePlayerInLeague(newPlayerRanks, playersEnrolled, currentLeagueA, currentLeagueB, 1);
                    LeavePlayerInLeague(newPlayerRanks, playersEnrolled, currentLeagueA, currentLeagueB, 3);
                    
                    MoveFirstPlayerOfOneDownUp(newPlayerRanks, playersEnrolled, oneLeagueDownA, oneLeagueDownB);

                    MoveLastPlayerDown(newPlayerRanksOneLeaguDown, playersEnrolled, currentLeagueA, currentLeagueB);

                    SwitchRelegationsOneLeagueDown(newPlayerRanks, playersEnrolled, newPlayerRanksOneLeaguDown, currentLeagueA, currentLeagueB);
                }
                
                if (i >= 2)
                {
                    var newPlayerRanksOneLeaguDown = leagues[i / 2];
                    
                    SwitchRelegationsOneLeagueDown(newPlayerRanks, playersEnrolled, newPlayerRanksOneLeaguDown, currentLeagueA, currentLeagueB);
                    SwitchRelegationsTwoLeagueasdDown(newPlayerRanks, playersEnrolled, newPlayerRanksOneLeaguDown, currentLeagueA, currentLeagueB);
                    
                    MoveLastPlayerDown(newPlayerRanksOneLeaguDown, playersEnrolled, currentLeagueA, currentLeagueB);
                    
                    AddIfEnrolled(newPlayerRanks, playersEnrolled, currentLeagueA.RelegationMatchOverOneLeague.Result.Winner);
                    AddIfEnrolled(newPlayerRanksOneLeaguDown, playersEnrolled, currentLeagueA.RelegationMatchOverOneLeague.Result.Looser);
                    
                    AddIfEnrolled(newPlayerRanks, playersEnrolled, currentLeagueB.RelegationMatchOverOneLeague.Result.Winner);
                    AddIfEnrolled(newPlayerRanksOneLeaguDown, playersEnrolled, currentLeagueB.RelegationMatchOverOneLeague.Result.Looser);
                }
                
                leagues[i / 2] = newPlayerRanks.Any() ? newPlayerRanks : null;
            }

            await SetDeployments();
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

            AddIfEnrolled(newPlayerRanks, playersEnrolled, currentLeagueB.RelegationMatchOverOneLeague.Result.Winner);
            AddIfEnrolled(newPlayerRanksOneLeaguDown, playersEnrolled, currentLeagueB.RelegationMatchOverOneLeague.Result.Looser);
        }
        
        private void SwitchRelegationsTwoLeagueasdDown(
            List<Player> newPlayerRanks,
            List<Player> playersEnrolled,
            List<Player> newPlayerRanksOneLeaguDown,
            League currentLeagueA,
            League currentLeagueB)
        {
            AddIfEnrolled(newPlayerRanks, playersEnrolled, currentLeagueA.RelegationMatchOverOneLeague.Result.Winner);
            AddIfEnrolled(newPlayerRanksOneLeaguDown, playersEnrolled, currentLeagueA.RelegationMatchOverOneLeague.Result.Looser);
            
            AddIfEnrolled(newPlayerRanks, playersEnrolled, currentLeagueB.RelegationMatchOverOneLeague.Result.Winner);
            AddIfEnrolled(newPlayerRanksOneLeaguDown, playersEnrolled, currentLeagueB.RelegationMatchOverOneLeague.Result.Looser);
        }

        private static bool WinnerWasInCurrentLeague(League currentLeagueA)
        {
            return currentLeagueA.Players.Any(p => p.Id == currentLeagueA.RelegationMatchOverTwoLeagues.Result.Winner);
        }

        private void LeavePlayerInLeague(
            List<Player> newPlayerRanks, 
            List<Player> playersEnrolled,
            League currentLeagueA,
            League currentLeagueB,
            int index)
        {
            AddIfEnrolled(newPlayerRanks, playersEnrolled, currentLeagueA.Players[index].Id);
            AddIfEnrolled(newPlayerRanks, playersEnrolled, currentLeagueB.Players[index].Id);
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
                if (!newPlayerRanks.Any(p => p.Id == enrolledPlayer.Id))
                {
                    newPlayerRanks.Add(enrolledPlayer);
                }
            }
        }

        private async Task SetDeployments()
        {
            var seasons = await _seasonRepository.LoadSeasons();
            var currentSeason = seasons[0];

            var currentLeagues = await _leagueRepository.LoadForSeason(currentSeason.SeasonId);

            var secondaryObjectives = Enum.GetValues<SecondaryObjective>();
            Shuffle(secondaryObjectives);
            var deployments = Enum.GetValues<Deployment>();
            Shuffle(deployments);

            foreach (var currentLeague in currentLeagues)
            {
                var league = await _leagueRepository.Load(currentLeague.Id);
                league.SetScenarioAndDeployments(secondaryObjectives, deployments);
                await _leagueRepository.Update(league);
            }
        }

        private void Shuffle<T>(IList<T> list)
        {
            var n = list.Count;
            while (n > 1) {
                n--;
                var k = rng.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }

        public async Task CreateRelegations()
        {
            var seasons = await _seasonRepository.LoadSeasons();
            var currentSeason = seasons[1];

            var currentLeagues = await _leagueRepository.LoadForSeason(currentSeason.SeasonId);

            for (var index = 0; index < currentLeagues.Count; index++)
            {
                var oneLeagueBelow = index + 2 < currentLeagues.Count ? currentLeagues[index + 2] : null;
                var twoLeagueBelow = index + 4 < currentLeagues.Count ? currentLeagues[index + 4] : null;
                var currentLeague = currentLeagues[index];
                currentLeague.CreatRelegations(oneLeagueBelow, twoLeagueBelow);
                await _leagueRepository.Update(currentLeague);
            }
        }
    }
}