using System;
using System.Collections.Generic;
using MongoDB.Bson;

namespace FadingFlame.UserAccounts
{
    public class UserState
    {
        public event EventHandler UserLoggedIn;

        private List<ObjectId> Admins = new()
        {
            new("611ce5c80c434f08ea507fa6")
        };

        public virtual void SetUserLoggedIn(ObjectId playerId, string userName)
        {
            UserIsLoggedIn = true;
            LoggedInPlayerId = playerId;
            UserName = userName;
            UserLoggedIn?.Invoke(this, EventArgs.Empty);
            UserIsAdmin = Admins.Contains(playerId);
        }

        public bool UserIsLoggedIn { get; private set; }
        public bool UserIsAdmin { get; private set; }
        public ObjectId LoggedInPlayerId { get; private set; }
        public string UserName { get; private set; }
    }
}