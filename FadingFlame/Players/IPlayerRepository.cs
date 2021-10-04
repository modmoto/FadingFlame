using System.Collections.Generic;
using System.Threading.Tasks;
using FadingFlame.Lists;
using FadingFlame.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FadingFlame.Players
{
    public interface IPlayerRepository
    {
        Task<List<Player>> LoadAll();
        Task<Player> Load(ObjectId id);
        Task UpdateWithLists(Player player, int season);
        Task Update(Player player);
        Task Insert(Player player);
        Task<Player> LoadByEmail(string accountEmail);
        Task<List<Player>> PlayersThatEnlistedInCurrentSeason(int season);
        Task<List<Player>> LoadForLeague(List<ObjectId> playerIds, int season);
        Task<List<Player>> LoadAllWithoutList();
        Task<List<Player>> LoadAllWitList();
    }

    public class PlayerRepository : MongoDbRepositoryBase, IPlayerRepository
    {
        private readonly IListRepository _listRepository;

        public PlayerRepository(MongoClient mongoClient, IListRepository listRepository) : base(mongoClient)
        {
            _listRepository = listRepository;
        }

        public Task<List<Player>> LoadAll()
        {
            return LoadAll<Player>();
        }

        public async Task<Player> Load(ObjectId id)
        {
            var player = await LoadFirst<Player>(id);
            var currentList = await _listRepository.Load(player.ArmyIdCurrentSeason);
            var nextList = await _listRepository.Load(player.ArmyIdNextSeason);
            player.ArmyCurrentSeason = currentList;
            player.ArmyNextSeason = nextList;
            return player;
        }

        public async Task UpdateWithLists(Player player, int season)
        {
            if (player.ArmyCurrentSeason != null)
            {
                if (player.ArmyCurrentSeason.Id == default)
                {
                    await _listRepository.Insert(player.ArmyCurrentSeason);
                }
                else
                {
                    await _listRepository.Update(player.ArmyCurrentSeason);
                }
            }

            await Upsert(player);
        }

        public Task Update(Player player)
        {
            return Upsert(player);
        }

        public Task Insert(Player player)
        {
            return base.Insert(player);
        }

        public Task<Player> LoadByEmail(string accountEmail)
        {
            return LoadFirst<Player>(p => p.AccountEmail == accountEmail);
        }

        public Task<List<Player>> PlayersThatEnlistedInCurrentSeason(int season)
        {
            return LoadAll<Player>(p => p.ArmyIdCurrentSeason != default);
        }

        public async Task<List<Player>> LoadForLeague(List<ObjectId> playerIds, int season)
        {
            var players = await LoadAll<Player>(p => playerIds.Contains(p.Id));
            foreach (var player in players)
            {
                var list = await _listRepository.Load(player.ArmyIdCurrentSeason);
                player.ArmyCurrentSeason = list;
            }

            return players;
        }

        public Task<List<Player>> LoadAllWithoutList()
        {
            return LoadAll<Player>(p => p.ArmyIdCurrentSeason == default);
        }

        public Task<List<Player>> LoadAllWitList()
        {
            return LoadAll<Player>(p => p.ArmyIdCurrentSeason != default);
        }
    }
}