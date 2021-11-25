using System.Collections.Generic;
using System.Linq;
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
        Task<List<Player>> PlayersThatEnlistedInNextSeason();
        Task<List<Player>> LoadForLeague(List<ObjectId> playerIds, int season);
        Task<List<Player>> LoadAllWithoutList();
        Task<List<Player>> LoadAllWitList();
        Task<List<Player>> PlayersWithListInCurrentSeason();
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
            await LoadLists(player);
            return player;
        }

        private async Task LoadLists(Player player)
        {
            var currentList = await _listRepository.Load(player.ArmyIdCurrentSeason);
            var nextList = await _listRepository.Load(player.ArmyIdNextSeason);
            player.ArmyCurrentSeason = currentList;
            player.ArmyNextSeason = nextList;
        }

        public async Task UpdateWithLists(Player player, int season)
        {
            if (player.ArmyCurrentSeason != null)
            {
                if (player.ArmyIdCurrentSeason == default)
                {
                    await _listRepository.Insert(player.ArmyCurrentSeason);
                    player.ArmyIdCurrentSeason = player.ArmyCurrentSeason.Id;
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

        public async Task<Player> LoadByEmail(string accountEmail)
        {
            var player = await LoadFirst<Player>(p => p.AccountEmail == accountEmail);
            if (player != null)
            {
                await LoadLists(player);    
            }
            
            return player;
        }

        public Task<List<Player>> PlayersThatEnlistedInNextSeason()
        {
            return LoadAll<Player>(p => p.ArmyIdNextSeason != default);
        }

        public async Task<List<Player>> LoadForLeague(List<ObjectId> playerIds, int season)
        {
            var players = await LoadAll<Player>(p => playerIds.Contains(p.Id));
            await AddArmies(players);
            return players;
        }

        private async Task AddArmies(List<Player> players)
        {
            var armyIds = players.Select(p => p.ArmyIdCurrentSeason).ToList();
            var armies = await _listRepository.Load(armyIds);
            foreach (var player in players)
            {
                var armyOfPlayer = armies.SingleOrDefault(a => a.Id == player.ArmyIdCurrentSeason);
                player.ArmyCurrentSeason = armyOfPlayer;
            }
        }

        public Task<List<Player>> LoadAllWithoutList()
        {
            return LoadAll<Player>(p => p.ArmyIdCurrentSeason == default);
        }

        public Task<List<Player>> LoadAllWitList()
        {
            return LoadAll<Player>(p => p.ArmyIdCurrentSeason != default);
        }

        public async Task<List<Player>> PlayersWithListInCurrentSeason()
        {
            var players = await LoadAll<Player>(p => p.ArmyIdCurrentSeason != default);
            await AddArmies(players);
            return players;
        }
    }
}