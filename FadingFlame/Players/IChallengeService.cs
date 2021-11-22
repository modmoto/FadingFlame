using System.Collections.Generic;
using System.Threading.Tasks;
using FadingFlame.Matchups;
using MongoDB.Bson;

namespace FadingFlame.Players
{
    public interface IChallengeService
    {
        Task ChallengePlayer(Player loggedInPlayer, Player player);
        Task DeleteChalenge(Player loggedInPlayer);
    }

    public class ChallengeService : IChallengeService
    {
        private readonly IMatchupRepository _matchupRepository;
        private readonly IPlayerRepository _playerRepository;

        public ChallengeService(IMatchupRepository matchupRepository, IPlayerRepository playerRepository)
        {
            _matchupRepository = matchupRepository;
            _playerRepository = playerRepository;
        }
        
        public async Task ChallengePlayer(Player loggedInPlayer, Player player)
        {
            if (!loggedInPlayer.HasChallengedPlayer)
            {
                await Challenge(loggedInPlayer, player);
            }
            else
            {
                await DeleteChalenge(loggedInPlayer);
                await Challenge(loggedInPlayer, player);
            }
        }

        public async Task DeleteChalenge(Player loggedInPlayer)
        {
            await _matchupRepository.DeleteMatches(new List<ObjectId> { loggedInPlayer.CurrentChallengeId });
            loggedInPlayer.CancelChallenge();
            await _playerRepository.Update(loggedInPlayer);
        }

        private async Task Challenge(Player loggedInPlayer, Player player)
        {
            var challenge = Matchup.CreateChallengeGame(loggedInPlayer, player);
            await _matchupRepository.InsertMatches(new List<Matchup> {challenge});
            loggedInPlayer.Challenge(challenge);
            await _playerRepository.Update(loggedInPlayer);
        }
    }
}