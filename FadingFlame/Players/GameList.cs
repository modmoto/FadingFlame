namespace FadingFlame.Players
{
    public class GameList
    {
        public static GameList Create(string name, string list)
        {
            return new()
            {
                Name = name,
                List = list
            };
        }

        public string List { get; set; }

        public string Name { get; set; }
    }
}