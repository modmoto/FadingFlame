using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using NUnit.Framework;

namespace FadingFlameTests;

public class IntegrationTestBase
{
    protected readonly MongoClient MongoClient = new(Environment.GetEnvironmentVariable("TESTING_MONGO_CONNECTION_STRING") ?? "mongodb://157.90.1.251:3512/");

    [SetUp]
    public async Task Setup()
    {
        await MongoClient.DropDatabaseAsync("FadingFlame");
    }
}