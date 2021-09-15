using System;
using System.Collections.Generic;
using System.Linq;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;

namespace FadingFlame.Players
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
            "simonheiss87@gmail.com", // modmoto
            "tulmir@gmail.com", // tulmir
            "herzog1602@googlemail.com", //almentro
        };

        public virtual void SetUserData(ObjectId playerId)
        {
            LoggedInPlayerId = playerId;
            UserLoggedIn?.Invoke(this, EventArgs.Empty);
        }

        public bool UserIsLoggedIn => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
        public bool UserIsAdmin => Admins.Contains(AccountEmail);
        public ObjectId? LoggedInPlayerId { get; private set; }
        public string UserName => _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.GivenName)?.Value;
        public string AccountEmail => _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
    }
}