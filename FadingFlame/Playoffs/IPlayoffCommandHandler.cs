using System.Threading.Tasks;
using FadingFlame.Admin;
using FadingFlame.Leagues;
using FadingFlame.Players;

namespace FadingFlame.Playoffs;

public interface IPlayoffCommandHandler
{
    Task<Playoff> CreatePlayoffs();
}

public class PlayoffCommandHandler : IPlayoffCommandHandler
{
    private readonly ILeagueRepository _leagueRepository;
    private readonly IPlayoffRepository _playoffRepository;
    private readonly IMmrRepository _mmrRepository;
    private readonly SeasonState _seasonState;

    public PlayoffCommandHandler(
        ILeagueRepository leagueRepository,
        IPlayoffRepository playoffRepository,
        SeasonState seasonState, 
        IMmrRepository mmrRepository)
    {
        _leagueRepository = leagueRepository;
        _playoffRepository = playoffRepository;
        _seasonState = seasonState;
        _mmrRepository = mmrRepository;
    }

    public async Task<Playoff> CreatePlayoffs()
    {
        await _playoffRepository.Delete(_seasonState.CurrentSeason.SeasonId);
        var leagues = await _leagueRepository.LoadForSeason(_seasonState.CurrentSeason.SeasonId);

        var playoffs = await Playoff.Create(_mmrRepository, _seasonState.CurrentSeason.SeasonId, leagues);

        await _playoffRepository.Insert(playoffs);

        return playoffs;
    }
}