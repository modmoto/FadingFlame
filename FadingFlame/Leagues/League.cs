using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
        public bool IsFull => Players.Count == 6;
        public DateTimeOffset StartDate { get; set; }

        private Matchup GetMatchup(ObjectId matchId)
        {
            var match = GameDays.SelectMany(g => g.Matchups).FirstOrDefault(m => m.MatchId == matchId);
            if (match == null)
            {
                throw new ValidationException("Match not in this gameday");
            }

            return match;
        }

        public void ReportGame(MatchResultDto matchResultDto)
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

            Players = Players.OrderByDescending(p => p.Points).ThenByDescending(p => p.VictoryPoints).ToList();
        }

        public void AddPlayer(Player player)
        {
            if (IsFull) return;
            
            Players = Players.Where(p => p.Id != player.Id).ToList();
            var playerInLeague = PlayerInLeague.Create(player.Id);
            Players.Add(playerInLeague);

            if (IsFull)
            {
                CreateGameDays();
            }
        }

        public static League Create(int season, DateTimeOffset startDate, string divisionId, string name)
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
                var matchup = Matchup.Create(teamsTemp[playerIndex], teams.First());
                matchups.Add(matchup);

                for (var index = 1; index < numberOfMatchesInARound; index++)
                {
                    var firstPlayerIndex = (gameDayIndex + index) % numberOfPlayers;
                    var secondPlayerIndex = (gameDayIndex + numberOfPlayers - index) % numberOfPlayers;

                    var matchupInner = Matchup.Create(teamsTemp[firstPlayerIndex], teamsTemp[secondPlayerIndex]);
                    matchups.Add(matchupInner);
                }

                var round = GameDay.Create(offset, matchups);
                gameDays.Add(round);
            }

            GameDays = gameDays;
        }
    }
}