namespace HelpMeApi.Common.Object;

public class Extra
{
    public string Name { get; set; } = null!;
    public string Value { get; set; } = null!;
}

public static class ExtrasExtension
{
    public static Extra? Get(this List<Extra> extras, string name) =>
        extras.Find(extra => extra.Name == name);
}
