using System;
using System.Collections.Generic;
using MongoDB.Bson;

namespace FadingFlame.UserAccounts
{
    public class UserState
    {
        public event EventHandler UserLoggedIn;
        public event EventHandler UserLoggedOut;

        private List<string> Admins = new()
        {
            "AliceSmith@email.com",
            "simonheiss87@gmail.com"
        };

        public virtual void SetUserLoggedIn(ObjectId playerId, string userName, string email)
        {
            UserIsLoggedIn = true;
            LoggedInPlayerId = playerId;
            UserName = userName;
            UserIsAdmin = Admins.Contains(email);
            UserLoggedIn?.Invoke(this, EventArgs.Empty);
        }
        
        public virtual void SetUserLoggedOut()
        {
            UserIsLoggedIn = false;
            LoggedInPlayerId = null;
            UserName = null;
            UserIsAdmin = false;
            UserLoggedOut?.Invoke(this, EventArgs.Empty);
        }

        public bool UserIsLoggedIn { get; private set; }
        public bool UserIsAdmin { get; private set; }
        public ObjectId? LoggedInPlayerId { get; private set; }
        public string UserName { get; private set; }
    }
}