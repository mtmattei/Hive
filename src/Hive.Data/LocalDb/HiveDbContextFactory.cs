using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Hive.Data.LocalDb;

/// <summary>
/// Design-time factory for EF Core migrations.
/// </summary>
public class HiveDbContextFactory : IDesignTimeDbContextFactory<HiveDbContext>
{
    public HiveDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<HiveDbContext>();
        optionsBuilder.UseSqlite("Data Source=hive.db");
        return new HiveDbContext(optionsBuilder.Options);
    }
}
