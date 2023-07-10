namespace HelpMeApi.User.Model;

public class UserPrivateModel : UserBaseModel
{
    public int? Age { get; set; }
    public long LastSignedInAt { get; set; }
}
