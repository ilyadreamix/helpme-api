using System.Text.Json.Serialization;

namespace HelpMeApi.Common.State.Model;

public class StateModel<T> : StateModelBase
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; set; }

    public static StateModel<T> ParseOk(T? data) => new()
    {
        Code = (int)StateCode.Ok,
        State = StateCode.Ok.ToString(),
        HasError = false,
        Data = data
    };

    public static StateModel<T> ParseFrom(StateCode code, T? data = default) => new()
    {
        Code = (int)code,
        State = code.ToString(),
        HasError = code != StateCode.Ok,
        Data = data
    };
}
