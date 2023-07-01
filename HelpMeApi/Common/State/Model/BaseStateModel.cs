using System.Text.Json.Serialization;

namespace HelpMeApi.Common.State.Model;

public abstract class BaseStateModel
{
    public int Code { get; set; }
    public string State { get; set; } = string.Empty;
    public bool HasError { get; set; } = true;
    public long Timestamp { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Extras { get; set; } = null;
}
