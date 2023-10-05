using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FadingFlame.Admin;
using FadingFlame.Leagues;
using FadingFlame.Matchups;
using FadingFlame.Players;
using FadingFlame.ReadModelBase;
using MongoDB.Bson;
using Moq;
using NUnit.Framework;

namespace FadingFlameTests;

public class SetMissingMatchesToDrawHandlerTests : IntegrationTestBase
{
    private readonly Mock<ISeasonRepository> _seasonRepo;

    public SetMissingMatchesToDrawHandlerTests()
    {
        var seasonRepoMock = new Mock<ISeasonRepository>();
        seasonRepoMock.Setup(m => m.LoadSeasons()).ReturnsAsync(new List<Season>() { new() { SeasonId = 1, IsPubliclyVisible = true } });
        _seasonRepo = seasonRepoMock;
    }
    
    [Test]
    public async Task SetMissingMatchesToDraw_FirstRunDoesNotThrowException()
    {
        var handler = new SetMissingMatchesToDrawHandler(_seasonRepo.Object, new LeagueRepository(MongoClient, new MatchupRepository(MongoClient)));

        var update = await handler.Update(HandlerVersion.CreateFrom<SetMissingMatchesToDrawHandler>(null));
        Assert.AreEqual(update.HandlerName, nameof(SetMissingMatchesToDrawHandler));
        Assert.AreEqual(DateTime.Parse(update.Version).Date, DateTime.UtcNow.Date);
    }

    [TestCase(1, true)]
    [TestCase(5, true)]
    [TestCase(13, true)]
    [TestCase(14, true)]
    [TestCase(15, true)]
    [TestCase(27, true)]
    [TestCase(28, false)]
    [TestCase(29, false)]
    public async Task SetMissingMatchesToDraw_UpdatesNothingAsGameTimeIsReallyNew(int timeSpan, bool  expected)
    {
        var leagueRepository = new LeagueRepository(MongoClient, new MatchupRepository(MongoClient));
        var handler = new SetMissingMatchesToDrawHandler(_seasonRepo.Object, leagueRepository);

        var league = CreateEmptyLeague(DateTime.Now.AddDays(-timeSpan));
        await leagueRepository.Insert(new List<League> { league });
        await handler.Update(HandlerVersion.CreateFrom<SetMissingMatchesToDrawHandler>(DateTime.UtcNow.ToString("O")));
        var loadForSeason = await leagueRepository.LoadForSeason(1);
        var single = loadForSeason.Single();
        var matchups = single.GameDays.SelectMany(g => g.Matchups);
        Assert.AreEqual(expected, matchups.All(m => !m.IsFinished));
    }
    
    [Test]
    public async Task SetMissingMatchesToDraw_DrawsFirstWeekAsNoOnePlayed()
    {
        var leagueRepository = new LeagueRepository(MongoClient, new MatchupRepository(MongoClient));
        var handler = new SetMissingMatchesToDrawHandler(_seasonRepo.Object, leagueRepository);

        var league = CreateEmptyLeague(DateTime.Now - TimeSpan.FromDays(29));
        await leagueRepository.Insert(new List<League> { league });
        await handler.Update(HandlerVersion.CreateFrom<SetMissingMatchesToDrawHandler>(DateTime.UtcNow.ToString("O")));
        var loadForSeason = await leagueRepository.LoadForSeason(1);
        var single = loadForSeason.Single();
        var allMatchesExceptFirstWeek = single.GameDays.Skip(1).SelectMany(g => g.Matchups).ToList();
        Assert.IsTrue(single.GameDays[0].Matchups.All(m => m.IsFinished));
        Assert.IsTrue(single.GameDays[0].Matchups.All(m => m.IsZeroToZero));
        Assert.IsTrue(allMatchesExceptFirstWeek.All(m => !m.IsFinished));
        Assert.IsTrue(allMatchesExceptFirstWeek.All(m => !m.IsZeroToZero));
    }
    
    [Test]
    public async Task SetMissingMatchesToDraw_DrawsSecondWeekAsNoOnePlayed()
    {
        var leagueRepository = new LeagueRepository(MongoClient, new MatchupRepository(MongoClient));
        var handler = new SetMissingMatchesToDrawHandler(_seasonRepo.Object, leagueRepository);

        var league = CreateEmptyLeague(DateTime.Now - TimeSpan.FromDays(28));
        await leagueRepository.Insert(new List<League> { league });
        await handler.Update(HandlerVersion.CreateFrom<SetMissingMatchesToDrawHandler>(DateTime.UtcNow.AddDays(14).ToString("O")));
        var loadForSeason = await leagueRepository.LoadForSeason(1);
        var single = loadForSeason.Single();
        var allMatchesExceptFirstTwoWeeks = single.GameDays.Skip(2).SelectMany(g => g.Matchups).ToList();
        Assert.IsTrue(single.GameDays[1].Matchups.All(m => m.IsFinished));
        Assert.IsTrue(single.GameDays[1].Matchups.All(m => m.IsZeroToZero));
        Assert.IsTrue(single.GameDays[0].Matchups.All(m => m.IsFinished));
        Assert.IsTrue(single.GameDays[0].Matchups.All(m => m.IsZeroToZero));
        Assert.IsTrue(allMatchesExceptFirstTwoWeeks.All(m => !m.IsFinished));
        Assert.IsTrue(allMatchesExceptFirstTwoWeeks.All(m => !m.IsZeroToZero));
    }
    
    private static League CreateEmptyLeague(DateTime startingTime)
    {
        var league = League.Create(1, startingTime, "1A", "egal");
        for (int i = 0; i < 6; i++)
        {
            league.AddPlayer(new Player {Id = ObjectId.GenerateNewId(), DisplayName = i.ToString()});
        }

        league.CreateGameDays();
        return league;
    }
}