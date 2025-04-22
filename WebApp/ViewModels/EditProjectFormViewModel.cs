using Business.Models;
using Domain.Models;
using System.Collections.Generic;

namespace WebApp.ViewModels
{
    /// <summary>
    /// ViewModel för EditProject-formuläret, inklusive alla listor för selects och checks.
    /// </summary>
    public class EditProjectFormViewModel
    {
        public EditProjectFormData FormData { get; set; } = new EditProjectFormData();

        public IEnumerable<ClientModel> Clients { get; set; } = new List<ClientModel>();

        public IEnumerable<StatusModel> Statuses { get; set; } = new List<StatusModel>();

        public IEnumerable<MemberModel> Members { get; set; } = new List<MemberModel>();
    }
}
