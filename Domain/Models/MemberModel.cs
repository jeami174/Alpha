
namespace Domain.Models;

public class MemberModel
{
    public int Id { get; set; }
    public string? ImageName { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public DateTime? DateOfBirth { get; set; }

    public RoleModel? Role { get; set; }
    public AddressModel? Address { get; set; }
}
