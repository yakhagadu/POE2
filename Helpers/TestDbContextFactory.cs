using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;

namespace RaceDay.Tests.Helpers;

public static class TestDbContextFactory
{
    /// <summary>
    /// Creates a fresh in-memory RaceDayDbContext. Each call = isolated database.
    /// </summary>
    public static RaceDayDbContext Create()
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new RaceDayDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}