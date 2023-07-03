namespace HelpMeApi.Common.State.Model;

public abstract class BaseStateModel
{
    public int Code { get; set; }
    public string State { get; set; } = default!;
    public bool HasError { get; set; } = true;

    public long Timestamp => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}
