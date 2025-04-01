using Business.Models;
using Data.Entities;

namespace Business.Factories;

public static class MemberFactory
{
    public static MemberEntity Create(AddMemberForm form, RoleEntity role)
    {
        return new MemberEntity
        {
            FirstName = form.FirstName,
            LastName = form.LastName,
            Email = form.MemberEmail,
            Phone = form.Phone,
            DateOfBirth = form.DateOfBirth,
            Role = role
        };
    }

    public static void Update(MemberEntity entity, EditMemberForm form, RoleEntity role)
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

    public static MemberModel ToModel(MemberEntity entity)
    {
        return new MemberModel
        {
            Id = entity.Id,
            ImageName = entity.ImageName,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            Email = entity.Email,
            Phone = entity.Phone,
            DateOfBirth = entity.DateOfBirth,
            Role = entity.Role,
            Address = entity.Address
        };
    }

}
