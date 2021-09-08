namespace FadingFlame.Players
{
    public class GameList
    {
        public static GameList Create(string name, string list, Faction faction)
        {
            return new()
            {
                Name = name,
                List = list,
                Faction = faction
            };
        }


        public Faction Faction { get; set; }

        public string List { get; set; }

        public string Name { get; set; }
    }
}