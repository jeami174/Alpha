using Business.Models;
using Data.Entities;

namespace Business.Factories;

public static class MemberFactory
{
    /// <summary>
    /// Skapar en MemberEntity från ett AddMemberForm-objekt.
    /// </summary>

    public static MemberEntity Create(AddMemberForm form)
    {
        return new MemberEntity
        {
            FirstName = form.FirstName,
            LastName = form.LastName,
            Email = form.MemberEmail,
            Address = form.Address,
            Phone = form.Phone,
            JobTitle = form.JobTitle,
            DateOfBirth = form.DateOfBirth,
        };
    }

    /// <summary>
    /// Uppdaterar en befintlig MemberEntity med data från EditMemberForm (vid redigering).
    /// </summary>
    public static void Update(MemberEntity entity, EditMemberForm form)
    {
        entity.FirstName = form.FirstName;
        entity.LastName = form.LastName;
        entity.Email = form.MemberEmail;
        entity.Address = form.Address;
        entity.Phone = form.Phone;
        entity.JobTitle = form.JobTitle;
        entity.DateOfBirth = form.DateOfBirth;

        if (!string.IsNullOrEmpty(form.ImageName))
        {
            entity.ImageName = form.ImageName;
        }

    }
}
