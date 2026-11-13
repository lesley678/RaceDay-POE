
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;

namespace RaceDay.API.Tests;

public sealed class TestDatabase : IDisposable
{
    private readonly SqliteConnection _connection;

    public RaceDayContext Context { get; }

    public TestDatabase()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<RaceDayContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new RaceDayContext(options);
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
