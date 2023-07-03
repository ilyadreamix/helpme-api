namespace HelpMeApi.Profile.Model;

public class ProfileModel
{
    public string Nickname { get; set; } = null!;
    public int? Age { get; set; }
    public bool IsHidden { get; set; }
}
