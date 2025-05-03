using Business.Models;
using Data.Entities;

namespace Business.Factories;
public class UserFactory
{
    public ApplicationUser Create(SignUpFormModel form)
    {
        return new ApplicationUser
        {
            UserName = form.Email,
            Email = form.Email,
            FirstName = form.FirstName,
            LastName = form.LastName
        };
    }
}
