using System;
using System.Collections.Generic;
using DSharpPlus.Entities;

namespace FadingFlame.Leagues
{
    public class LeagueConstants
    {
        public static DateTimeOffset StartDate => new DateTimeOffset().AddDays(14).AddMonths(9).AddYears(2020);
        public static DateTimeOffset ListSubmissionDeadline => new DateTimeOffset().AddDays(24).AddMonths(8).AddYears(2020);
        public static bool ListSubmissionIsOver => DateTimeOffset.UtcNow > new DateTimeOffset().AddDays(25).AddMonths(8).AddYears(2020);
        public static int CurrentSeason => 1;
        public static List<string> Names => new()
        {
            "Sunna",
            "Sigmar",
            "Nagash",
            "Von Carstein",
            "Thorek",
            "Grungi",
            "Gork",
            "Mork",
            "Teclis",
            "Morathi",
            "Kroak",
            "Hellebron",
            "Settra",
            "Karl Franz",
            "Gotrek",
            "Felix",
            "Grimgor",
            "Thanquol",
            "Snikch",
            "Golgfag",
            "Kroq-gar",
            "Oxyotl",
            "Hellsnicht",
            "Green Knight",
            "Be'lakor",
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
            DiscordColor.Aquamarine,
            DiscordColor.Azure,
            DiscordColor.Blue,
            DiscordColor.Blurple,
            DiscordColor.Brown,
            DiscordColor.Chartreuse,
            DiscordColor.Cyan,
            DiscordColor.Gold,
            DiscordColor.Goldenrod,
            DiscordColor.Gray,
            DiscordColor.Grayple,
            DiscordColor.Green,
            DiscordColor.Lilac,
            DiscordColor.Magenta,
            DiscordColor.Orange,
            DiscordColor.Purple,
            DiscordColor.Red,
            DiscordColor.Rose,
            DiscordColor.Rose,
            DiscordColor.Sienna,
            DiscordColor.Teal,
            DiscordColor.Turquoise,
            DiscordColor.Violet,
            DiscordColor.Wheat
        };
    }
}