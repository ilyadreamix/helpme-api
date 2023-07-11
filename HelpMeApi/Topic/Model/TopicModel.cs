namespace HelpMeApi.Topic.Model;

public class TopicModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public bool IsReadOnly { get; set; }
    public long CreatedAt { get; set; }
}
