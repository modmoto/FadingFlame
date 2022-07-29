using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FadingFlame.Matchups;
using FadingFlame.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FadingFlame.Leagues;

public interface ILeagueRepository
{
    Task<List<League>> LoadForSeason(int season);
    Task<League> Load(ObjectId id);
    Task Insert(List<League> newLeagues);
    Task DeleteForSeason(int season);
    Task<bool> Update(League league);
    Task<List<League>> LoadLeaguesForPlayer(ObjectId playerId);
    Task<League> LoadLeagueForPlayerInSeason(ObjectId playerId, int season);
    Task DeleteLeague(ObjectId leagueId);
}

public class LeagueRepository : MongoDbRepositoryBase, ILeagueRepository
{
    private readonly IMatchupRepository _matchupRepository;

    public LeagueRepository(MongoClient mongoClient, IMatchupRepository matchupRepository) : base(mongoClient)
    {
        _matchupRepository = matchupRepository;
    }

    public async Task<List<League>> LoadForSeason(int season)
    {
        var leagues = await LoadAll<League>(l => l.Season == season);
        var matchIds = leagues.SelectMany(l => l.GameDays.SelectMany(g => g.MatchupIds));
        var matches = await _matchupRepository.LoadMatches(matchIds.ToList());

        foreach (var league in leagues)
        {
            foreach (var gameDay in league.GameDays)
            {
                var matchesInGameDay = matches.Where(m => gameDay.MatchupIds.Contains(m.Id)).ToList();
                gameDay.Matchups = matchesInGameDay;
            }
        }
            
        return leagues;
    }

    public async Task<League> Load(ObjectId id)
    {
        var league = await LoadFirst<League>(id);
        await AddMatches(league);
        league.ReorderPlayers();

        return league;
    }

    private async Task AddMatches(League league)
    {
        if (league == null) return;
            
        var matchIds = league.GameDays.SelectMany(g => g.MatchupIds);
        var matches = await _matchupRepository.LoadMatches(matchIds.ToList());

        foreach (var gameDay in league.GameDays)
        {
            var matchesInGameDay = matches.Where(m => gameDay.MatchupIds.Contains(m.Id)).ToList();
            gameDay.Matchups = matchesInGameDay;
        }
    }

    public async Task Insert(List<League> newLeagues)
    {
        var matchups = newLeagues.SelectMany(l => l.GameDays.SelectMany(g => g.Matchups)).ToList();
        await _matchupRepository.InsertMatches(matchups);    
        await base.Insert(newLeagues);
    }

    public async Task DeleteForSeason(int season)
    {
        var leagues = await LoadForSeason(season);
        var matchups = leagues.SelectMany(l => l.GameDays.SelectMany(g => g.MatchupIds)).ToList();
        await _matchupRepository.DeleteMatches(matchups);
        await DeleteMultiple<League>(l => l.Season == season);
    }

    public async Task<bool> Update(League league)
    {
        var result = await UpdateVersionsave(league);
        if (result)
        {
            var matchups = league.GameDays.SelectMany(g => g.Matchups).ToList();
            await _matchupRepository.UpdateMatches(matchups);
        }

        return result;
    }

    public Task<List<League>> LoadLeaguesForPlayer(ObjectId playerId)
    {
        return LoadAll<League>(l => l.Players.Any(r => r.Id == playerId));
    }

    public async Task<League> LoadLeagueForPlayerInSeason(ObjectId playerId, int season)
    {
        var league = await LoadFirst<League>(l => l.Players.Any(r => r.Id == playerId) && l.Season == season);
        await AddMatches(league);
        return league;
    }

    public Task DeleteLeague(ObjectId leagueId)
    {
        return Delete<League>(leagueId);
    }
}