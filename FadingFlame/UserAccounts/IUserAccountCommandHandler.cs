using System.Threading.Tasks;
using FadingFlame.Players;

namespace FadingFlame.UserAccounts
{
    public interface IUserAccountCommandHandler
    {
        public Task Login(LoginModel loginModel);
        public Task LoginFromCookie();
        public Task Logout();
        public Task Register(RegisterModel registerModel);
    }

    public class UserAccountCommandHandler : IUserAccountCommandHandler
    {
        private readonly ILocalStorageService _localStorageService;
        private readonly IUserAccountRepository _accountRepository;
        private readonly IPlayerRepository _playerRepository;
        private readonly IUserContext _context;
        private readonly UserState _userState;
        private string _userKey = "user";

        public UserAccountCommandHandler(
            ILocalStorageService localStorageService,
            IUserAccountRepository accountRepository,
            IPlayerRepository playerRepository,
            IUserContext context,
            UserState userState)
        {
            _localStorageService = localStorageService;
            _accountRepository = accountRepository;
            _playerRepository = playerRepository;
            _context = context;
            _userState = userState;
        }

        public async Task Login(LoginModel loginModel)
        {
            var user = await _accountRepository.Load(loginModel.Email);
            if (user?.Password == loginModel.Password)
            {
                await _localStorageService.SetItem(_userKey, user);
                _context.SetUser(user);
                var player = await _playerRepository.Load(user.PlayerId);
                _userState.SetUserLoggedIn(player.Id, player.DisplayName);
            }
        }

        public async Task LoginFromCookie()
        {
            var item = await _localStorageService.GetItem<UserAccount>(_userKey);
            if (item != null)
            {
                var loginModel = new LoginModel
                {
                    Email = item.Email,
                    Password = item.Password
                };

                await Login(loginModel);
            }
        }

        public async Task Logout()
        {
            _context.RemoveUser();
            await _localStorageService.RemoveItem(_userKey);
            _userState.SetUserLoggedOut();
        }

        public async Task Register(RegisterModel registerModel)
        {
            var user = await _accountRepository.Load(registerModel.Email);

            if (user == null)
            {
                var player = Player.Create(registerModel.DisplayName);
                await _playerRepository.Insert(player);
                var userAccount = UserAccount.Create(player.Id, registerModel.Email, registerModel.Password);

                _context.SetUser(userAccount);
                await _localStorageService.SetItem(_userKey, userAccount);
                await _accountRepository.Upsert(userAccount);
                _userState.SetUserLoggedIn(player.Id, player.DisplayName);
            }
        }
    }
}