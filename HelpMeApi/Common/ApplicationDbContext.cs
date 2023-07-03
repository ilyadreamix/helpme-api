using HelpMeApi.Account;
using HelpMeApi.Profile.Entity;
using Microsoft.EntityFrameworkCore;

namespace HelpMeApi.Common;

public class ApplicationDbContext : DbContext
{
    private readonly IConfiguration _configuration;
    
    public ApplicationDbContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseLazyLoadingProxies();
        optionsBuilder.UseNpgsql(_configuration.GetConnectionString("Db"));
    }

    public DbSet<AccountEntity> Accounts { get; set; } = null!;
    public DbSet<ProfileEntity> Profiles { get; set; } = null!;
}
