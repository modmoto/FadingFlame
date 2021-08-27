using System.Threading.Tasks;
using FadingFlame.Players;

namespace FadingFlame.UserAccounts
{
    public interface IUserAccountCommandHandler
    {
        public Task Register(RegisterModel registerModel);
    }

    public class UserAccountCommandHandler : IUserAccountCommandHandler
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly UserState _userState;

        public UserAccountCommandHandler(
            IPlayerRepository playerRepository,
            UserState userState)
        {
            _playerRepository = playerRepository;
            _userState = userState;
        }

        public async Task Register(RegisterModel registerModel)
        {
            // todo from logged in user nehmen
            var player = Player.Create(registerModel.DisplayName);
            await _playerRepository.Insert(player);
            _userState.SetUserLoggedIn(player.Id, player.DisplayName);
        }
    }
}