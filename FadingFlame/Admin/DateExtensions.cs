using System;

namespace FadingFlame.Admin
{
    public static class DateExtensions
    {
        public static string ToMyDate(this DateTime time)
        {
            return time.ToString("dd.MM.");
        }
        
        public static string ToMyDateYear(this DateTime time)
        {
            return time.ToString("dd.MM.yyyy");
        }
        
        public static string ToMyDateTime(this DateTime time)
        {
            return time.ToString("dd.MM. hh:mm");
        }
    }
}