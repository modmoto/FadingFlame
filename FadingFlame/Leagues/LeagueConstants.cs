using System.Collections.Generic;
using DSharpPlus.Entities;
using MongoDB.Bson;

namespace FadingFlame.Leagues
{
    public class LeagueConstants
    {
        public static ObjectId FreeWinPlayer => new("507f1f77bcf86cd799439011");
        public static List<string> Names => new()
        {
            "Sunna",
            "Avras",
            "Moritaur",
            "Amhair",
            "Cibaresh",
            "Sugulag",
            "The Lady",
            "Amryl",
            "Teput",
            "Quintus Augustus",
            "Qenghet Khan",
            "Bragh",
            "Yema",
            "Lugar",
            "Nezibkesh",
            "Savar",
            "Tsanas",
            "Phateb",
            "Shamut",
            // those are old Fantasy names
            "Archaon",
            "Ariel",
            "Araloth",
            "Imrik",
            "Tzarina Katarin",
            "Tzar Boris",
            "Settra",
            "The Red Duke",
            "Abhorash",
            "Greasus Goldtooth",
            "Mazdamundi"
        };

        public static List<string> Ids => new()
        {
            "1A",
            "1B",
            "2A",
            "2B",
            "3A",
            "3B",
            "4A",
            "4B",
            "5A",
            "5B",
            "6A",
            "6B",
            "7A",
            "7B",
            "8A",
            "8B",
            "9A",
            "9B",
            "10A",
            "10B",
            "11A",
            "11B",
            "12A",
            "12B",
            "13A",
            "13B",
            "14A",
            "14B",
            "15A",
            "15B",
            "16A",
            "16B",
            "17A",
            "17B",
            "18A",
            "18B"
        };

        public static List<DiscordColor> DiscordColors => new()
        {
            DiscordColor.Gold,
            DiscordColor.Gold,
            DiscordColor.Goldenrod,
            DiscordColor.Goldenrod,
            DiscordColor.Green,
            DiscordColor.Green,
            DiscordColor.DarkGreen,
            DiscordColor.DarkGreen,
            DiscordColor.Aquamarine,
            DiscordColor.Aquamarine,
            DiscordColor.Azure,
            DiscordColor.Azure,
            DiscordColor.Blue,
            DiscordColor.Blue,
            DiscordColor.Blurple,
            DiscordColor.Blurple,
            DiscordColor.Purple,
            DiscordColor.Purple,
            DiscordColor.Magenta,
            DiscordColor.Magenta,
            DiscordColor.IndianRed,
            DiscordColor.IndianRed,
            DiscordColor.DarkRed,
            DiscordColor.DarkRed,
            DiscordColor.Brown,
            DiscordColor.Brown,
            DiscordColor.Wheat,
            DiscordColor.Wheat,
            DiscordColor.Chartreuse,
            DiscordColor.Chartreuse,
            DiscordColor.Gray,
            DiscordColor.Gray,
            DiscordColor.Grayple,
            DiscordColor.Grayple,
            DiscordColor.DarkBlue,
            DiscordColor.DarkBlue,
            DiscordColor.NotQuiteBlack,
            DiscordColor.NotQuiteBlack,
            DiscordColor.SapGreen,
            DiscordColor.SapGreen,
        };
    }
}