using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FadingFlame.Players;
using FadingFlame.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FadingFlame.Matchups
{
    public interface IMatchupRepository
    {
        Task<List<Matchup>> LoadMatches(List<ObjectId> matchIds);
        Task InsertMatches(List<Matchup> matchups);
        Task DeleteMatches(List<ObjectId> matchups);
        Task UpdateMatches(List<Matchup> matchups);
        Task<List<Matchup>> LoadFinishedSince(DateTime currentVersionVersion);
        Task<List<Matchup>> LoadOpenMatchesOfPlayer(Player player);
        Task<List<Matchup>> LoadMatchesOfPlayer(Player player);
        Task<List<Matchup>> LoadChallengesOfPlayer(Player player);
        Task<Matchup> LoadMatch(ObjectId objectId);
        Task<Matchup> LoadChallengeOfPlayers(Player loggedInPlayer, Player player);
        Task UpdateMatch(Matchup matchup);
    }

    public class MatchupRepository : MongoDbRepositoryBase, IMatchupRepository
    {
        public MatchupRepository(MongoClient mongoClient) : base(mongoClient)
        {
        }
        public Task<List<Matchup>> LoadMatches(List<ObjectId> matchIds)
        {
            return LoadAll<Matchup>(m => matchIds.Contains(m.Id));
        }

        public Task InsertMatches(List<Matchup> matchups)
        {
            if (matchups.Count == 0) return Task.CompletedTask;
            return Insert(matchups);
        }

        public Task DeleteMatches(List<ObjectId> matchups)
        {
            return DeleteMultiple<Matchup>(matchups);
        }

        public Task UpdateMatches(List<Matchup> matchups)
        {
            return UpsertMany(matchups);
        }

        public Task<List<Matchup>> LoadFinishedSince(DateTime currentVersionVersion)
        {
            return LoadAll<Matchup>(m => m.Result.RecordedAt > currentVersionVersion);
        }

        public Task<List<Matchup>> LoadOpenMatchesOfPlayer(Player player)
        {
            return LoadAll<Matchup>(m => m.Result != null && (m.Player1 == player.Id || m.Player2 == player.Id));
        }

        public async Task<List<Matchup>> LoadMatchesOfPlayer(Player player)
        {
            var loadMatchesOfPlayer = await LoadAll<Matchup>(m => m.Player1 == player.Id || m.Player2 == player.Id);
            return loadMatchesOfPlayer.OrderByDescending(l => l.Id).ToList();
        }

        public Task<List<Matchup>> LoadChallengesOfPlayer(Player player)
        {
            return LoadAll<Matchup>(m => m.IsChallenge && m.Result == null && (m.Player1 == player.Id || m.Player2 == player.Id));
        }

        public Task<Matchup> LoadMatch(ObjectId objectId)
        {
            return LoadFirst<Matchup>(objectId);
        }

        public Task<Matchup> LoadChallengeOfPlayers(Player loggedInPlayer, Player player)
        {
            return LoadFirst<Matchup>(m => m.IsChallenge && m.Result == null && m.Player1 == loggedInPlayer.Id && m.Player2 == player.Id);
        }

        public Task UpdateMatch(Matchup matchup)
        {
            return Upsert(matchup);
        }
    }
}