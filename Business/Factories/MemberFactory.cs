using Business.Models;
using Data.Entities;
using Domain.Models;

namespace Business.Factories;
public class MemberFactory
{
 public MemberEntity Create(AddMemberForm form, RoleEntity role, string imageName)
    {
        return new MemberEntity
        {
            FirstName = form.FirstName,
            LastName = form.LastName,
            Email = form.MemberEmail,
            Phone = form.Phone,
            DateOfBirth = form.DateOfBirth,
            Role = role,
            ImageName = imageName
        };
    }
    public void Update(MemberEntity entity, EditMemberForm form, RoleEntity role)
    {
        entity.FirstName = form.FirstName;
        entity.LastName = form.LastName;
        entity.Email = form.MemberEmail;
        entity.Phone = form.Phone;
        entity.DateOfBirth = form.DateOfBirth;
        entity.Role = role;

        if (!string.IsNullOrEmpty(form.ImageName))
        {
            entity.ImageName = form.ImageName;
        }
    }
    public MemberModel Create(MemberEntity entity)
    {
        return new MemberModel
        {
            Id = entity.Id,
            ImageName = string.IsNullOrWhiteSpace(entity.ImageName)
                ? "uploads/members/avatars/default.svg"
                : entity.ImageName.Replace("\\", "/"),
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            Email = entity.Email,
            Phone = entity.Phone,
            DateOfBirth = entity.DateOfBirth,

            Role = entity.Role != null
                ? new RoleModel
                {
                    Id = entity.Role.Id,
                    Name = entity.Role.Name
                }
                : null,

            Address = entity.Address != null
                ? new AddressModel
                {
                    Id = entity.Address.Id,
                    Street = entity.Address.Street,
                    PostalCode = entity.Address.PostalCode,
                    City = entity.Address.City
                }
                : null
        };
    }
}
