using Microsoft.EntityFrameworkCore;

namespace DockerWorkshops;

public class SampleDbContext : DbContext
{
    protected readonly IConfiguration Configuration;

    public SampleDbContext(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        // connect to sql server with connection string from app settings
        options.UseSqlServer(Configuration.GetConnectionString(nameof(SampleDbContext)));
    }

    public DbSet<SomeData> SomeData => Set<SomeData>();
}