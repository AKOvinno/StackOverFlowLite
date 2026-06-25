using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using StackOverflowLite.Infrastructure.Persistence.Context;

namespace StackOverflowLite.Infrastructure;

/// <summary>
/// Design-time factory so `dotnet ef migrations add` can instantiate ApplicationDbContext
/// when running with -p Persistence -s API.
/// The actual DbContext lives in Infrastructure; this factory bridges the design-time tooling.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../StackOverflowLite.API"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
