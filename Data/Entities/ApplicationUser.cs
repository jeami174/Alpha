using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Data.Entities;
public class ApplicationUser : IdentityUser
{
    [ProtectedPersonalData]
    public string FirstName { get; set; } = null!;

    [ProtectedPersonalData]
    public string LastName { get; set; } = null!;

    [ProtectedPersonalData]
    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";

    [ProtectedPersonalData]
    [Column(TypeName = "varchar(10)")]

    public string PreferredTheme { get; set; } = "light";

    public MemberEntity? Member { get; set; }

    public ICollection<NotificationDismissedEntity> DismissedNotification { get; set; } = [];
    
    public DateTime LastLogin { get; set; } = DateTime.UtcNow;
}
