using System;
using System.Collections.Generic;
using System.Linq;
using IdentityModel;
using Microsoft.AspNetCore.Http;

namespace FadingFlame.Players;

public class LoggedInUserState
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LoggedInUserState(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
        
    public event EventHandler UserLoggedIn;
    public event EventHandler UserTimeSet;

    private List<string> _admins = new()
    {
        "simonheiss87@gmail.com", // modmoto
        "tulmir@gmail.com", // tulmir
        "naughtylusbupport@protonmail.com", // nautilus
        "kara_te@yahoo.de", // just_flo
        "maik.henschen@web.de", // SouthsideDevil
    };

    public virtual void SetUserData(Player player)
    {
        LoggedInPlayer = player;
        LoadingPlayer = false;
        UserLoggedIn?.Invoke(this, EventArgs.Empty);
    }
        
    public virtual void ToggleAdmin()
    {
        UserIsAdmin = !UserIsAdmin;
        UserLoggedIn?.Invoke(this, EventArgs.Empty);
    }

    public bool UserIsLoggedIn => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
    public bool UserCanBeAdmin => _admins.Contains(AccountEmail?.ToLower());
    public bool UserIsAdmin { get; private set; }
    public Player LoggedInPlayer { get; private set; } = new();
    public DateTimeOffset? CurrentUserTime { get; private set; }
    public bool LoadingPlayer { get; private set; } = true;
    public string UserName => _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.GivenName)?.Value;
    public string AccountEmail => _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

    public void SetUserTime(DateTimeOffset currentUserTime)
    {
        CurrentUserTime = currentUserTime;
        UserTimeSet?.Invoke(this, EventArgs.Empty);
    }
}