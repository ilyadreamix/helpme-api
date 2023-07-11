using System.ComponentModel.DataAnnotations;

namespace HelpMeApi.Chat.Model.Request;

public class ChatUpdateRequestModel
{
    [MinLength(4)]
    [MaxLength(36)]
    public string? Title { get; set; }
    
    [MaxLength(256)]
    public string? Description { get; set; }

    public bool IsPublic { get; set; } = true;
    public List<Guid>? TopicIds { get; set; } = null;
}
