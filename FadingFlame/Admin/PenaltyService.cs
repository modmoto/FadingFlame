using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FadingFlame.Leagues;
using FadingFlame.Players;
using MongoDB.Bson;

namespace FadingFlame.Admin
{
    public class PenaltyService
    {
        private readonly ILeagueRepository _leagueRepository;
        private readonly IPlayerRepository _playerRepository;

        public PenaltyService(ILeagueRepository leagueRepository, IPlayerRepository playerRepository)
        {
            _leagueRepository = leagueRepository;
            _playerRepository = playerRepository;
        }
        
        public async Task<List<PenaltyUser>> GetPenalties(int season)
        {
            var leagues = await _leagueRepository.LoadForSeason(season);
            var playerIds = leagues.SelectMany(l => l.Players).Select(p => p.Id).ToList();
            var players = await _playerRepository.LoadForLeague(playerIds);
            var penalties = new List<PenaltyUser>();
            foreach (var league in leagues)
            {
                var matchups = league.GameDays.SelectMany(g => g.Matchups).ToList();
                foreach (var leaguePlayer in league.Players)
                {
                    var player = players.Single(p => p.Id == leaguePlayer.Id);
                    if (player.ArmyCurrentSeason.List1.List == player.ArmyCurrentSeason.List2.List) continue;
                    if (player.ArmyCurrentSeason.List2.List == "NA") continue;
                    if (player.ArmyCurrentSeason.List2.List == "none") continue;
                    
                    var matchesOfPlayer = matchups.Where(m => m.Player1 == leaguePlayer.Id || m.Player2 == leaguePlayer.Id);
                    var list1Count = 0;
                    var list2Count = 0;
                    foreach (var matchup in matchesOfPlayer)
                    {
                        if (!matchup.PlayersSelectedList || (matchup.Result?.Player1.BattlePoints == 0 && matchup.Result?.Player2.BattlePoints == 0) || matchup.Result?.WasDefLoss == true)
                        {
                            list1Count++;
                            list2Count++;
                            continue;
                        }

                        if (matchup.Player1 == player.Id)
                        {
                            if (matchup.Player1List == player.ArmyCurrentSeason.List1.Name)
                            {
                                list1Count++;
                            }
                            else
                            {
                                list2Count++;
                            }
                        }
                        
                        if (matchup.Player2 == player.Id)
                        {
                            if (matchup.Player2List == player.ArmyCurrentSeason.List1.Name)
                            {
                                list1Count++;
                            }
                            else
                            {
                                list2Count++;
                            }
                        }
                    }

                    if (list1Count < 2 || list2Count < 2)
                    {
                        penalties.Add(new PenaltyUser(player.Id, player.DisplayName, player.DiscordTag, league.DivisionId, league.Name, player.ArmyCurrentSeason.List1.List, player.ArmyCurrentSeason.List2.List));
                    }
                }
            }

            return penalties;
        }
    }

    public class PenaltyUser
    {
        public ObjectId PlayerId { get; }
        public string PlayerDisplayName { get; }
        public string PlayerDiscordTag { get; }
        public string LeagueDivisionId { get; }
        public string LeagueName { get; }
        public string List1 { get; }
        public string List2 { get; }

        public PenaltyUser(ObjectId playerId, string playerDisplayName, string playerDiscordTag, string leagueDivisionId, string leagueName, string list1, string list2)
        {
            PlayerId = playerId;
            PlayerDisplayName = playerDisplayName;
            PlayerDiscordTag = playerDiscordTag;
            LeagueDivisionId = leagueDivisionId;
            LeagueName = leagueName;
            List1 = list1;
            List2 = list2;
        }
    }
}