using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FadingFlame.Leagues;
using FadingFlame.Matchups;
using FadingFlame.Players;
using FadingFlame.ReadModelBase;

namespace FadingFlame.GlobalLadder
{
    public class PlayerRankingModelReadHandler : IAsyncUpdatable
    {
        private readonly IMatchupRepository _matchupRepository;
        private readonly IPlayerRepository _playerRepository;
        private readonly IRankingReadmodelRepository _rankingReadmodelRepository;

        public PlayerRankingModelReadHandler(
            IMatchupRepository matchupRepository, 
            IPlayerRepository playerRepository, 
            IRankingReadmodelRepository rankingReadmodelRepository)
        {
            _matchupRepository = matchupRepository;
            _playerRepository = playerRepository;
            _rankingReadmodelRepository = rankingReadmodelRepository;
        }

        public async Task<HandlerVersion> Update(HandlerVersion currentVersion)
        {
            if (currentVersion.Version == null)
            {
                currentVersion.Version = DateTime.MinValue.ToString(CultureInfo.InvariantCulture);
            }
            var dateTime = DateTime.Parse(currentVersion.Version);
            var newMatches = await _matchupRepository.LoadFinishedSince(dateTime);
            var lastVersion = currentVersion.Version;
            foreach (var match in newMatches)
            {
                lastVersion = match.Result.RecordedAt.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
                if (match.Player2 == default) 
                    continue;
                
                var player1 = await _playerRepository.Load(match.Player1);
                var player2 = await _playerRepository.Load(match.Player2);

                var matchesPlayer1 = await _matchupRepository.LoadFinishedMatchesOfPlayer(player1);
                var matchesPlayer2 = await _matchupRepository.LoadFinishedMatchesOfPlayer(player2);

                var model1 = PlayerRankingReadModel.Create(player1, matchesPlayer1.Select(m => m.Result).ToList());
                var model2 = PlayerRankingReadModel.Create(player2, matchesPlayer2.Select(m => m.Result).ToList());

                await _rankingReadmodelRepository.Upsert(new List<PlayerRankingReadModel> {model1, model2});
            }

            return HandlerVersion.CreateFrom<PlayerRankingModelReadHandler>(lastVersion);
        }
    }
}