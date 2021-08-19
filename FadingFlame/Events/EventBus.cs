using System;

namespace FadingFlame.Events
{
    public class EventBus
    {
        public event EventHandler UserLoggedIn;

        public virtual void OnUserLoggedIn(UserLoggedInEvent e)
        {
            var handler = UserLoggedIn;
            handler?.Invoke(this, e);
        }
    }
}