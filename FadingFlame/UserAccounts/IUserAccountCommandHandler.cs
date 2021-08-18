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
        private string _userKey = "user";

        public UserAccountCommandHandler(
            ILocalStorageService localStorageService,
            IUserAccountRepository accountRepository,
            IPlayerRepository playerRepository,
            IUserContext context)
        {
            _localStorageService = localStorageService;
            _accountRepository = accountRepository;
            _playerRepository = playerRepository;
            _context = context;
        }

        public async Task Login(LoginModel loginModel)
        {
            var user = await _accountRepository.Load(loginModel.Email);
            if (user?.Password == loginModel.Password)
            {
                await _localStorageService.SetItem(_userKey, user);
                _context.SetUser(user);
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

        public Task Logout()
        {
            _context.SetUser(null);
            return _localStorageService.RemoveItem(_userKey);
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
            }
        }
    }
}