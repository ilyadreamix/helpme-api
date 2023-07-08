using System.ComponentModel.DataAnnotations;
using HelpMeApi.Chat.Entity;
using HelpMeApi.Topic.Model;
using HelpMeApi.User.Entity;

namespace HelpMeApi.Topic.Entity;

public class TopicEntity
{
    [Key]
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public List<ChatEntity> Chats { get; set; } = new();
    public List<UserEntity> Users { get; set; } = new();
    
    public static explicit operator TopicModel(TopicEntity entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name
    };
}
