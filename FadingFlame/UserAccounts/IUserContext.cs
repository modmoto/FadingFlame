using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;

namespace FadingFlame.UserAccounts
{
    public interface IUserContext
    {
        UserAccount GetUser();
        void SetUser(UserAccount account);
        void RemoveUser();
    }

    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContext;

        public UserContext(IHttpContextAccessor httpContext)
        {
            _httpContext = httpContext;
        }

        public UserAccount GetUser()
        {
            var claimsIdentity = FindUserClaim();
            if (claimsIdentity != null)
            {
                var userAccount = new UserAccount
                {
                    Email = claimsIdentity.Claims.FirstOrDefault(c => c.Type == "email")?.Value,
                    PlayerId = new ObjectId(claimsIdentity.Claims.FirstOrDefault(c => c.Type == "playerId")?.Value)
                };

                return userAccount;
            }

            return null;
        }

        public void SetUser(UserAccount account)
        {
            var identity = new ClaimsIdentity(new Claim[]
            {
                new("email", account.Email),
                new("playerId", account.PlayerId.ToString()),
            });

            var findUserClaim = FindUserClaim();
            if (findUserClaim == null)
            {
                _httpContext.HttpContext?.User.AddIdentity(identity);
            }
        }

        public void RemoveUser()
        {
            if (_httpContext.HttpContext != null) _httpContext.HttpContext.User = new ClaimsPrincipal();
        }

        private ClaimsIdentity FindUserClaim()
        {
            var claimsIdentity =
                _httpContext.HttpContext?.User.Identities.FirstOrDefault(u => u.HasClaim(c => c.Type == "playerId"));
            return claimsIdentity;
        }
    }
}