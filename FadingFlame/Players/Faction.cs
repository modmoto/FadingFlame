using System.Text.RegularExpressions;

namespace FadingFlame.Players
{
    public enum Faction
    {
        BeastHeards = 1,
        DaemonLegions = 2,
        DreadElves = 3,
        DwarvenHolds = 4,
        EmpireOfSonnstahl = 5,
        HighBornElves = 6,
        InfernalDwarfs = 7,
        KingdomOfEquitane = 8,
        OgreKhans = 9,
        OrcsAndGoblins = 10,
        SaurianAncients = 11,
        SylvanElves = 12,
        UndyingDynasties = 13,
        VampireConvenant = 14,
        VerminSwarm = 15,
        WarriorsOfTheDarkGods = 16
    }

    public static class FactionExtensions
    {
        public static string ToFactionString(this Faction faction)
        {
            return Regex.Replace(faction.ToString(), "(\\B[A-Z])", " $1");
        }
    }
}