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

    public MemberEntity? Member { get; set; }
}
