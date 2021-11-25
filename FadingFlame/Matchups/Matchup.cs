using System.Collections.Generic;
using System.Linq;
using FadingFlame.Leagues;
using FadingFlame.Lists;
using FadingFlame.Players;
using FadingFlame.Repositories;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FadingFlame.Matchups
{
    public class Matchup : IIdentifiable
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public ObjectId Player1 { get; set; }
        public ObjectId Player2 { get; set; }
        [BsonIgnoreIfNull]
        public List<Player> OriginalPlayer1 { get; set; }
        [BsonIgnoreIfNull]
        public List<Player> OriginalPlayer2 { get; set; }

        [BsonIgnore] 
        public string Player1Name => OriginalPlayer1.First().DisplayName;
        [BsonIgnore]
        public string Player2Name => OriginalPlayer2.First().DisplayName;
        [BsonIgnore]
        public string Player1DiscordTag => OriginalPlayer1.First().DiscordTag;
        [BsonIgnore]
        public string Player2DiscordTag => OriginalPlayer2.First().DiscordTag;
        public MatchResult Result { get; set; }
        public GameList ChallengePlayer1List { get; set; }
        public GameList ChallengePlayer2List { get; set; }
        public bool PlayersSelectedList
        {
            get
            {
                if (IsChallengeOrRelegation)
                    return !string.IsNullOrEmpty(ChallengePlayer1List?.List) && !string.IsNullOrEmpty(ChallengePlayer2List?.List) 
                        || Result?.Player1List != null && Result?.Player2List != null;
                return !string.IsNullOrEmpty(Player1List) && !string.IsNullOrEmpty(Player2List);
            }
        }

        public string Player1List { get; set; }
        public string Player2List { get; set; }
        [BsonIgnore]
        public bool IsChallengeOrRelegation => IsChallenge || IsRelegation;
        public bool IsChallenge { get; set; }
        public bool IsRelegation { get; set; }
        public bool IsFinished => Result != null;

        public static Matchup CreateForLeague(PlayerInLeague playerAtHome, PlayerInLeague playerAsGuest)
        {
            return new()
            {
                Player1 = playerAtHome.Id,
                Player2 = playerAsGuest.Id,
                Id = ObjectId.GenerateNewId()
            };
        }

        public static Matchup CreateChallengeGame(ObjectId challenger, ObjectId playerThatGetsChallenged)
        {
            return new()
            {
                Player1 = challenger,
                Player2 = playerThatGetsChallenged,
                IsChallenge = true
            };
        }

        public static Matchup CreateRelegationGame(ObjectId challenger, ObjectId playerThatGetsChallenged)
        {
            return new()
            {
                Player1 = challenger,
                Player2 = playerThatGetsChallenged,
                Id = ObjectId.GenerateNewId(),
                IsRelegation = true
            };
        }
        
        public void SelectList(ObjectId playerId, string listName)
        {
            if (Player1 == playerId)
            {
                Player1List = listName;
            }
            
            if (Player2 == playerId)
            {
                Player2List = listName;
            }
        }

        public void RecordResult(MatchResult result)
        {
            if (result.WasDefLoss)
            {
                Player1List = GameList.DeffLoss().Name;
                Player2List = GameList.DeffLoss().Name;
            }
            Result = result;
        }

        public void DeleteResult()
        {
            Result = null;
        }

        public void SetZeroToZero()
        {
            Player1List = GameList.DeffLoss().Name;
            Player2List = GameList.DeffLoss().Name;
            Result = MatchResult.ZeroToZero(Id, Player1, Player2);
        }
    }
}