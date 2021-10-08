using System.Threading.Tasks;
using FadingFlame.Lists;

namespace FadingFlame.Discord
{
    public interface IListAcceptAndRejectService
    {
        Task ApproveList1(Army army, string discordTag);
        Task ApproveList2(Army army, string discordTag);
        Task RejectList1(Army army, string discordTag, bool sendPlayerMessage);
        Task RejectList2(Army army, string discordTag, bool sendPlayerMessage);
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
        
        public async Task ApproveList1(Army army, string discordTag)
        {
            army.List1.ApproveListChange();
            await UpdateAndSendMessage(army, discordTag, true, true);
        }

        public async Task ApproveList2(Army army, string discordTag)
        {
            army.List2.ApproveListChange();
            await UpdateAndSendMessage(army, discordTag, true, true);
        }
    
        public async Task RejectList1(Army army, string discordTag, bool sendPlayerMessage)
        {
            army.List1.RejectListChange();
            await UpdateAndSendMessage(army, discordTag, false, sendPlayerMessage);
        }

        public async Task RejectList2(Army army, string discordTag, bool sendPlayerMessage)
        {
            army.List2.RejectListChange();
            await UpdateAndSendMessage(army, discordTag, false, sendPlayerMessage);
        }

        public async Task RequestList1(Army army, string list)
        {
            army.List1.ProposeListChange(list);
            await _listRepository.Update(army);
            var count = (await _listRepository.LoadWithPendingChanges()).Count;
            await _discordBot.SendRequestListChangedToBotsChannel(count);
        }

        public async Task RequestList2(Army army, string list)
        {
            army.List2.ProposeListChange(list);
            await _listRepository.Update(army);
            var count = (await _listRepository.LoadWithPendingChanges()).Count;
            await _discordBot.SendRequestListChangedToBotsChannel(count);
        }

        private async Task UpdateAndSendMessage(Army army, string discordTag, bool wasApproved, bool sendPlayerMessage)
        {
            await _listRepository.Update(army);
            var count = (await _listRepository.LoadWithPendingChanges()).Count;
            await _discordBot.SendRequestListChangedToBotsChannel(count);
            if (sendPlayerMessage)
            {
                await _discordBot.ConfirmationMessageToUser(discordTag, wasApproved);    
            }
        }
    }
}