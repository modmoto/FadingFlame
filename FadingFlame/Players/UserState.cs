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

        public virtual void SetUserData(Player player)
        {
            LoggedInPlayer = player;
            LoadingPlayer = false;
            UserLoggedIn?.Invoke(this, EventArgs.Empty);
        }

        public bool UserIsLoggedIn => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
        public bool UserIsAdmin => Admins.Contains(AccountEmail);
        public Player LoggedInPlayer { get; private set; } = new();
        public bool LoadingPlayer { get; private set; } = true;
        public string UserName => _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.GivenName)?.Value;
        public string AccountEmail => _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
    }
}