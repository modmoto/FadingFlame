using System.Collections.Generic;
using System.Threading.Tasks;
using FadingFlame.Matchups;
using MongoDB.Bson;

namespace FadingFlame.Players
{
    public interface IChallengeService
    {
        Task ChallengePlayer(Player loggedInPlayer, Player player);
        Task RevokeChallenge(Matchup challengeId);
    }

    public class ChallengeService : IChallengeService
    {
        private readonly IMatchupRepository _matchupRepository;

        public ChallengeService(IMatchupRepository matchupRepository)
        {
            _matchupRepository = matchupRepository;
        }
        
        public async Task ChallengePlayer(Player loggedInPlayer, Player player)
        {
            var matchesOfPlayer = await _matchupRepository.LoadChallengeOfPlayers(loggedInPlayer, player);
            if (matchesOfPlayer == null)
            {
                var challenge = Matchup.CreateChallengeGame(loggedInPlayer, player);
                await _matchupRepository.InsertMatch(challenge);
            }
        }

        public async Task RevokeChallenge(Matchup challengeId)
        {
            await _matchupRepository.DeleteMatches(new List<ObjectId> { challengeId.Id });
        }
    }
}