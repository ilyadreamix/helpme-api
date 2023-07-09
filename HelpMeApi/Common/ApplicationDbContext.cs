using HelpMeApi.Chat.Entity;
using HelpMeApi.Moderation;
using HelpMeApi.User.Entity;
using HelpMeApi.Topic.Entity;
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
        optionsBuilder.UseNpgsql(_configuration.GetConnectionString("Db"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>()
            .HasMany(a => a.Chats)
            .WithMany(c => c.JoinedUsers)
            .UsingEntity<UserChatRelationEntity>(
                j => j.HasOne(ac => ac.Chat).WithMany(),
                j => j.HasOne(ac => ac.User).WithMany())
            .ToTable("UserChatRelation");

        modelBuilder.Entity<ChatEntity>()
            .HasOne(c => c.Creator)
            .WithMany()
            .HasForeignKey(c => c.CreatorId);
        
        modelBuilder.Entity<TopicEntity>()
            .HasMany(t => t.Users)
            .WithMany(p => p.Topics)
            .UsingEntity(j => j.ToTable("TopicUserRelation"));

        modelBuilder.Entity<TopicEntity>()
            .HasMany(t => t.Chats)
            .WithMany(c => c.Topics)
            .UsingEntity(j => j.ToTable("TopicChatRelation"));

        base.OnModelCreating(modelBuilder);
    }

    public DbSet<UserEntity> Users { get; set; } = null!;
    public DbSet<ChatEntity> Chats { get; set; } = null!;
    public DbSet<ChatMessageEntity> ChatMessages { get; set; } = null!;
    public DbSet<TopicEntity> Topics { get; set; } = null!;
    public DbSet<ModerationEntity> Moderations { get; set; } = null!;
}
