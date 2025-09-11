using FitnessTrackerApi.Services;
using Microsoft.EntityFrameworkCore;

namespace FitnessTrackerApi.Test;

public class TestDatabaseFixture
{
    private const string ConnectionString = @"Host=localhost;Port=5432;Database=fitness-tracker-api-test;Username=postgres;Password=postgres";

    public TestDatabaseFixture()
    {
        using (var context = CreateContext())
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }

        Cleanup();
    }

    public AppDbContext CreateContext()
        => new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(ConnectionString)
                .Options);

    public void Cleanup()
    {
        using var context = CreateContext();

        context.Workouts.RemoveRange(context.Workouts);
        context.RefreshTokens.RemoveRange(context.RefreshTokens);
        context.Users.RemoveRange(context.Users);
        context.SaveChanges();
    }
}

[CollectionDefinition("Tests")]
public class DatabaseTestsCollection : ICollectionFixture<TestDatabaseFixture>;