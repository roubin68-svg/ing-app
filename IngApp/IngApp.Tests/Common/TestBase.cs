using Microsoft.EntityFrameworkCore;
using IngApp.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;

namespace IngApp.Tests.Common;

/// <summary>
/// Base class برای تست‌های Integration که نیاز به Database دارند
/// </summary>
public abstract class TestBase : IDisposable
{
    protected AppDbContext DbContext { get; private set; }

    protected TestBase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        DbContext = new AppDbContext(options);
        
        // Seed داده‌های اولیه
        SeedDatabase();
    }

    /// <summary>
    /// Seed داده‌های اولیه برای تست (UserTypes, Roles, etc.)
    /// </summary>
    protected virtual void SeedDatabase()
    {
        // این متد در کلاس‌های فرزند override می‌شود
    }

    public void Dispose()
    {
        DbContext?.Dispose();
    }
}












