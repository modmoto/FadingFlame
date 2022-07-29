using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace FadingFlame.Matchups;

[Route("match-data")]
public class MatchReportController : Controller
{
    private readonly IMatchupRepository _matchupRepository;
    string _apiSecret =  Environment.GetEnvironmentVariable("API_SECRET");

    public MatchReportController(IMatchupRepository matchupRepository)
    {
        _matchupRepository = matchupRepository;
    }
        
    [HttpGet]
    public async Task<ActionResult<List<Matchup>>> Get([FromQuery] string since, [FromQuery] string secret, [FromQuery] bool alsoRetrieveDefWins = false)
    {
        if (secret != _apiSecret) return Unauthorized();
        if (DateTime.TryParse(since, out var sinceDate))
        {
            if (alsoRetrieveDefWins)
            {
                return await _matchupRepository.LoadFinishedSince(sinceDate);
            }
            return await _matchupRepository.LoadRealFinishedSince(sinceDate);
        }

        return BadRequest($"could not parse \"since\" string: {since}. Has to be in format 2008-10-31T17:04:32.0000000Z");
    }
}