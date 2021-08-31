using System;
using System.Collections.Generic;
using System.Linq;
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

        public virtual void SetUserData(ObjectId playerId, string userName)
        {
            LoggedInPlayerId = playerId;
            UserName = userName;
            UserLoggedIn?.Invoke(this, EventArgs.Empty);
        }

        public bool UserIsLoggedIn => _httpContextAccessor.HttpContext?.User.HasClaim(c => c.Type == "email") ?? false;
        public bool UserIsAdmin => Admins.Contains(Email);
        public ObjectId? LoggedInPlayerId { get; private set; }
        public string UserName { get; private set; }
        public string Email => _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
        public string OriginalUserName => _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == "given_name")?.Value;
    }
}