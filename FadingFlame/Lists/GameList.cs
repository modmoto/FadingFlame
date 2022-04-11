
using System.Text.Json.Serialization;

namespace FadingFlame.Lists
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
        [JsonIgnore]
        public string ProposedListChange { get; set; }

        public string Name { get; set; }

        public static GameList DeffLoss()
        {
            return Create("None picked", "", Faction.None);
        }
    }
}