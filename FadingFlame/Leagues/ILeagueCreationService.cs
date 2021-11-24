using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FadingFlame.Admin;
using FadingFlame.Players;

namespace FadingFlame.Leagues
{
    public interface ILeagueCreationService
    {
        Task CreateNewLeagues();
        Task SetDeployments();
    }

    public class LeagueCreationService : ILeagueCreationService
    {
        private readonly ISeasonRepository _seasonRepository;
        private readonly ILeagueRepository _leagueRepository;
        private Random rng = new();
        
        public LeagueCreationService(
            ISeasonRepository seasonRepository, 
            ILeagueRepository leagueRepository)
        {
            _seasonRepository = seasonRepository;
            _leagueRepository = leagueRepository;
        }
        
        public async Task CreateNewLeagues()
        {
            var seasons = await _seasonRepository.LoadSeasons();
            var nextSeason = seasons[0];
            var currentSeason = seasons[1];

            var currentLeagues = await _leagueRepository.LoadForSeason(currentSeason.SeasonId);
            
            var secondaryObjectives = Enum.GetValues<SecondaryObjective>();
            Shuffle(secondaryObjectives);
            var deployments = Enum.GetValues<Deployment>();
            Shuffle(deployments);
        }

        public async Task SetDeployments()
        {
            var seasons = await _seasonRepository.LoadSeasons();
            var currentSeason = seasons[1];
            
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
    }
}