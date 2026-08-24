using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SmartEstimate.Infrastructure.Persistence;

/// <summary>
/// Creates the DbContext for EF Core design-time commands without starting the API host.
/// </summary>
public sealed class SmartEstimateDesignTimeDbContextFactory : IDesignTimeDbContextFactory<SmartEstimateDbContext>
{
    public SmartEstimateDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("SMARTESTIMATE_DESIGNTIME_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=smartestimate_designtime;Username=smartestimate";

        var options = new DbContextOptionsBuilder<SmartEstimateDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new SmartEstimateDbContext(options);
    }
}
