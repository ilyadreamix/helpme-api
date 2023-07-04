using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HelpMeApi.Account;

namespace HelpMeApi.Profile;

public class ProfileEntity
{
    [Key]
    [ForeignKey("Account")]
    public Guid Id { get; set; }

    public virtual AccountEntity Account { get; set; } = null!;
    
    public string Nickname { get; set; } = null!;
    public int? Age { get; set; }
    public bool IsHidden { get; set; }
}
