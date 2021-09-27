using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using FadingFlame.Matchups;
using FadingFlame.Players;
using FadingFlame.Repositories;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FadingFlame.Leagues
{
    public class League : IIdentifiable
    {
        public string Name { get; set; }
        public int Season { get; set; }
        [BsonId]
        public ObjectId Id { get; set; }
        public List<PlayerInLeague> Players { get; set; } = new();
        public string DivisionId { get; set; }
        public List<GameDay> GameDays { get; set; } = new();
        public DateTime StartDate { get; set; }

        private Matchup GetMatchup(ObjectId matchId)
        {
            var match = GameDays.SelectMany(g => g.Matchups).FirstOrDefault(m => m.Id == matchId);
            if (match == null)
            {
                throw new ValidationException("Match not in this gameday");
            }

            return match;
        }

        public MatchResult ReportGame(MatchResultDto matchResultDto)
        {
            var player1Result = matchResultDto.Player1;
            var player2Result = matchResultDto.Player2;

            var player1 = Players.FirstOrDefault(p => p.Id == player1Result.Id);
            var player2 = Players.FirstOrDefault(p => p.Id == player2Result.Id);

            if (player1 == null || player2 == null)
            {
                throw new ValidationException("Players are not in this league");
            }

            var match = GetMatchup(matchResultDto.MatchId);
            
            var result = MatchResult.Create(matchResultDto.SecondaryObjective, player1Result, player2Result);
            match.RecordResult(result);
            
            player1.RecordResult(result.Player1.BattlePoints, result.Player1.VictoryPoints);
            player2.RecordResult(result.Player2.BattlePoints, result.Player2.VictoryPoints);

            ReorderPlayers();

            return result;
        }

        public void AddPlayer(Player player)
        {
            if (Players.Count == 6) return;
            
            Players = Players.Where(p => p.Id != player.Id).ToList();
            var playerInLeague = PlayerInLeague.Create(player.Id);
            Players.Add(playerInLeague);

            if (Players.Count == 6)
            {
                CreateGameDays();
            }
        }

        public static League Create(int season, DateTime startDate, string divisionId, string name)
        {
            return new()
            {
                Season = season,
                DivisionId = divisionId,
                Name = name,
                StartDate = startDate
            };
        }

        public void CreateGameDays()
        {
            var teams = Players.ToList();
            if (teams.Count % 2 != 0)
            {
                teams.Add(PlayerInLeague.Create(ObjectId.Empty));
            }
            var numberOfRounds = teams.Count - 1;
            var numberOfMatchesInARound = teams.Count / 2;

            var teamsTemp = new List<PlayerInLeague>();

            teamsTemp.AddRange(teams.Skip(numberOfMatchesInARound).Take(numberOfMatchesInARound));
            teamsTemp.AddRange(teams.Skip(1).Take(numberOfMatchesInARound - 1).Reverse());

            var numberOfPlayers = teamsTemp.Count;

            var gameDays = new List<GameDay>();

            for (var gameDayIndex = 0; gameDayIndex < numberOfRounds; gameDayIndex++)
            {
                var matchups = new List<Matchup>();

                var playerIndex = gameDayIndex % numberOfPlayers;

                var offset = StartDate.AddDays(14 * gameDayIndex);
                var matchup = Matchup.CreateForLeague(teamsTemp[playerIndex], teams.First());
                matchups.Add(matchup);

                for (var index = 1; index < numberOfMatchesInARound; index++)
                {
                    var firstPlayerIndex = (gameDayIndex + index) % numberOfPlayers;
                    var secondPlayerIndex = (gameDayIndex + numberOfPlayers - index) % numberOfPlayers;

                    var playerInLeague = teamsTemp[firstPlayerIndex];
                    var playerAsGuest = teamsTemp[secondPlayerIndex];
                    if (playerInLeague.Id == ObjectId.Empty || playerAsGuest.Id == ObjectId.Empty)
                    {
                        continue;
                    }

                    var matchupInner = Matchup.CreateForLeague(playerInLeague, playerAsGuest);
                    matchups.Add(matchupInner);
                }

                var round = GameDay.Create(offset, matchups);
                gameDays.Add(round);
            }

            Players = Players.Where(l => l.Id != ObjectId.Empty).ToList();
            GameDays = gameDays;
        }

        public void PenaltyPointsForPlayer(ObjectId playerId, int penaltyPoints)
        {
            var player = Players.FirstOrDefault(p => p.Id == playerId);
            if (player == null)
            {
                throw new ValidationException("Players are not in this league");
            }
            
            player.Penalty(penaltyPoints);
            ReorderPlayers();
        }

        public void DeleteGameReport(ObjectId matchId)
        {
            var match = GetMatchup(matchId);
            
            var player1 = Players.FirstOrDefault(p => p.Id == match.Player1);
            var player2 = Players.FirstOrDefault(p => p.Id == match.Player2);

            if (player1 == null || player2 == null)
            {
                throw new ValidationException("Players are not in this league");
            }
            
            player1.DeleteResult(match.Result.Player1.BattlePoints, match.Result.Player1.VictoryPoints);
            player2.DeleteResult(match.Result.Player2.BattlePoints, match.Result.Player2.VictoryPoints);

            ReorderPlayers();
            
            match.DeleteResult();
        }

        private void ReorderPlayers()
        {
            Players = Players.OrderByDescending(p => p.BattlePoints + p.PenaltyPoints).ThenByDescending(p => p.VictoryPoints).ToList();
        }
    }
}