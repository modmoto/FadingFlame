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
        
        public async Task CreateNewLeagues()
        {
            var seasons = await _seasonRepository.LoadSeasons();
            var nextSeason = seasons[0];
            var currentSeason = seasons[1];
            await _leagueRepository.DeleteForSeason(nextSeason.SeasonId);

            var currentLeagues = await _leagueRepository.LoadForSeason(currentSeason.SeasonId);

            foreach (var currentLeague in currentLeagues)
            {
                var newLeague = League.Create(nextSeason.SeasonId, nextSeason.StartDate, currentLeague.DivisionId, currentLeague.Name);
                var firstPlace = await _playerRepository.Load(currentLeague.Players[0].Id);
                var secondPlace = await _playerRepository.Load(currentLeague.Players[1].Id);
                newLeague.AddPlayer(firstPlace);
                newLeague.AddPlayer(secondPlace);
            }

            await SetDeployments();
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