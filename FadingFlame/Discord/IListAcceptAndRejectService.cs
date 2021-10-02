using System.Threading.Tasks;
using FadingFlame.Lists;
using FadingFlame.Players;

namespace FadingFlame.Discord
{
    public interface IListAcceptAndRejectService
    {
        Task ApproveList1(Army army);
        Task ApproveList2(Army army);
        Task RejectList1(Army army);
        Task RejectList2(Army army);
        Task RequestList1(Army army, string list);
        Task RequestList2(Army army, string list);
    }

    public class ListAcceptAndRejectService : IListAcceptAndRejectService
    {
        private readonly IDiscordBot _discordBot;
        private readonly IListRepository _listRepository;

        public ListAcceptAndRejectService(
            IDiscordBot discordBot,
            IListRepository listRepository)
        {
            _discordBot = discordBot;
            _listRepository = listRepository;
        }
        
        public async Task ApproveList1(Army army)
        {
            army.List1.ApproveListChange();
            await UpdateAndSendMessage(army);
        }

        public async Task ApproveList2(Army army)
        {
            army.List2.ApproveListChange();
            await UpdateAndSendMessage(army);
        }
    
        public async Task RejectList1(Army army)
        {
            army.List1.RejectListChange();
            await UpdateAndSendMessage(army);
        }

        public async Task RejectList2(Army army)
        {
            army.List2.RejectListChange();
            await UpdateAndSendMessage(army);
        }

        public async Task RequestList1(Army army, string list)
        {
            army.List1.ProposeListChange(list);
            await UpdateAndSendMessage(army);
        }

        public async Task RequestList2(Army army, string list)
        {
            army.List2.ProposeListChange(list);
            await UpdateAndSendMessage(army);
        }

        private async Task UpdateAndSendMessage(Army army)
        {
            await _listRepository.Update(army);
            var count = (await _listRepository.LoadWithPendingChanges()).Count;
            await _discordBot.SendRequestListChangedToBotsChannel(count);
        }
    }
}