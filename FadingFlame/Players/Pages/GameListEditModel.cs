using System.ComponentModel.DataAnnotations;
using FadingFlame.Lists;

namespace FadingFlame.Players.Pages
{
    public class GameListEditModel
    {
        [Range(1, 20)]
        public Faction Faction { get; set; }
        public string List { get; set; }
        public string ProposedListChange { get; set; }
        public string Name { get; set; }
    }
}