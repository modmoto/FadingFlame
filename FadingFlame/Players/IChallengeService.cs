using System.Collections.Generic;
using System.Threading.Tasks;
using FadingFlame.Matchups;
using MongoDB.Bson;

namespace FadingFlame.Players
{
    public interface IChallengeService
    {
        Task<ObjectId> ChallengePlayer(Player loggedInPlayer, Player player);
        Task RevokeChallenge(Matchup challengeId);
    }

    public class ChallengeService : IChallengeService
    {
        private readonly IMatchupRepository _matchupRepository;

        public ChallengeService(IMatchupRepository matchupRepository)
        {
            _matchupRepository = matchupRepository;
        }
        
        public async Task<ObjectId> ChallengePlayer(Player loggedInPlayer, Player player)
        {
            var matchesOfPlayer = await _matchupRepository.LoadChallengeOfPlayers(loggedInPlayer, player);
            if (matchesOfPlayer == null)
            {
                var challenge = Matchup.CreateChallengeGame(loggedInPlayer.Id, player.Id);
                await _matchupRepository.InsertMatch(challenge);
                return challenge.Id;
            }

            return matchesOfPlayer.Id;
        }

        public async Task RevokeChallenge(Matchup challengeId)
        {
            await _matchupRepository.DeleteMatches(new List<ObjectId> { challengeId.Id });
        }
    }
}