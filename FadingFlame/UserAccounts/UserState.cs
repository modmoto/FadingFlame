using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;

namespace FadingFlame.UserAccounts
{
    public class UserState
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserState(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        
        public event EventHandler UserLoggedIn;

        private List<string> Admins = new()
        {
            "simonheiss87@gmail.com"
        };

        public virtual void SetUserData(ObjectId playerId)
        {
            LoggedInPlayerId = playerId;
            if (Admins.Contains(AccountEmail))
            {
                var adminId = new ClaimsIdentity(new List<Claim> { new("role", "admin")});

                _httpContextAccessor.HttpContext.User.AddIdentity(adminId);
            }
            UserLoggedIn?.Invoke(this, EventArgs.Empty);
        }

        public bool UserIsLoggedIn => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
        public ObjectId? LoggedInPlayerId { get; private set; }
        public string UserName => _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.GivenName)?.Value;
        public string AccountEmail => _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
    }
}