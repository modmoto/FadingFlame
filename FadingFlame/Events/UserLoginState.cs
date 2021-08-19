using System;
using MongoDB.Bson;

namespace FadingFlame.Events
{
    public class UserLoginState
    {
        public event EventHandler UserLoggedIn;

        public virtual void SetUserLoggedIn(ObjectId playerId, string userName)
        {
            UserIsLoggedIn = true;
            LoggedInPlayerId = playerId;
            UserName = userName;
            UserLoggedIn?.Invoke(this, EventArgs.Empty);
        }

        public bool UserIsLoggedIn { get; private set; }
        public ObjectId LoggedInPlayerId { get; private set; }
        public string UserName { get; private set; }
    }
}