using System.ComponentModel.DataAnnotations;

namespace HelpMeApi.Chat.Model.Request;

public class ChatCreateRequestModel
{
    [MinLength(4)]
    [MaxLength(36)]
    public string Title { get; set; } = null!;
    
    [MaxLength(256)]
    public string? Description { get; set; }

    public bool IsPublic { get; set; } = true;
    public List<Guid> Topics { get; set; } = new();
}
