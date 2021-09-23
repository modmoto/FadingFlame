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
        HighbornElves = 6,
        InfernalDwarfs = 7,
        KingdomOfEquitaine = 8,
        OgreKhans = 9,
        OrcsAndGoblins = 10,
        SaurianAncients = 11,
        SylvanElves = 12,
        UndyingDynasties = 13,
        VampireConvenant = 14,
        VerminSwarm = 15,
        WarriorsOfTheDarkGods = 16,
        Asklanders = 17,
        Makhar = 18,
        Cultists = 19,
        Hobgoblins = 20,
        // look at the length of the allowed factions in EditListsModel, as this has to be hardcoded!!!!
    }

    public static class FactionExtensions
    {
        public static string ToFactionString(this Faction faction)
        {
            return Regex.Replace(faction.ToString(), "(\\B[A-Z])", " $1")
                .Replace("And", "and")
                .Replace("The", "the")
                .Replace("Asklanders", "Åsklanders")
                .Replace("Of", "of");
        }
    }
}