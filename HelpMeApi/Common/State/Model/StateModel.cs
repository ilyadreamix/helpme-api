using System.Text.Json.Serialization;

namespace HelpMeApi.Common.State.Model;

public class StateModel<T> : BaseStateModel
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; set; } = default;
}
