using Business.Dtos;
using Data.Entities;

namespace Business.Factories;

public static class MemberFactory
{
    /// <summary>
    /// Skapar en MemberEntity från ett AddMemberForm-objekt.
    /// Bildhantering (MemberImage) behöver hanteras separat.
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
}
