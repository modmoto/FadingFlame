using System.Threading.Tasks;
using FadingFlame.Players;

namespace FadingFlame.Discord
{
    public interface IListAcceptAndRejectService
    {
        Task ApproveList1(Player player);
        Task ApproveList2(Player player);
        Task RejectList1(Player player);
        Task RejectList2(Player player);
        Task RequestList1(Player player, string list);
        Task RequestList2(Player player, string list);
    }

    public class ListAcceptAndRejectService : IListAcceptAndRejectService
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly IDiscordBot _discordBot;

        public ListAcceptAndRejectService(IPlayerRepository playerRepository, IDiscordBot discordBot)
        {
            _playerRepository = playerRepository;
            _discordBot = discordBot;
        }
        
        public async Task ApproveList1(Player player)
        {
            player.Army.List1.ApproveListChange();
            await UpdateAndSendMessage(player);
        }

        public async Task ApproveList2(Player player)
        {
            player.Army.List2.ApproveListChange();
            await UpdateAndSendMessage(player);
        }
    
        public async Task RejectList1(Player player)
        {
            player.Army.List1.RejectListChange();
            await UpdateAndSendMessage(player);
        }

        public async Task RejectList2(Player player)
        {
            player.Army.List2.RejectListChange();
            await UpdateAndSendMessage(player);
        }

        public async Task RequestList1(Player player, string list)
        {
            player.Army.List1.ProposeListChange(list);
            await UpdateAndSendMessage(player);
        }

        public async Task RequestList2(Player player, string list)
        {
            player.Army.List2.ProposeListChange(list);
            await UpdateAndSendMessage(player);
        }

        private async Task UpdateAndSendMessage(Player player)
        {
            await _playerRepository.Update(player);
            var count = (await _playerRepository.LoadWithPendingChanges()).Count;
            await _discordBot.SendRequestListChangedToBotsChannel(count);
        }
    }
}