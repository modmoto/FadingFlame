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

        public void ProposeListChange(string change)
        {
            ProposedListChange = change;
        }
        
        public void RejectListChange()
        {
            ProposedListChange = null;
        }
        
        public void ApproveListChange()
        {
            List = ProposedListChange;
            ProposedListChange = null;
        }

        public Faction Faction { get; set; }

        public string List { get; set; }
        public string ProposedListChange { get; set; }

        public string Name { get; set; }
    }
}