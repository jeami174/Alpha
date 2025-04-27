using Business.Models;
using Domain.Models;

namespace WebApp.ViewModels
{
    /// <summary>
    /// ViewModel för AddMemberToProject-formuläret, med samma struktur som EditProject.
    /// </summary>
    public class AddMemberToProjectViewModel
    {
        public AddMemberToProjectForm FormData { get; set; } = new AddMemberToProjectForm();

        public IEnumerable<MemberModel> Members { get; set; } = new List<MemberModel>();
    }

    public class AddMemberToProjectForm
    {
        public string ProjectId { get; set; } = null!;

        public int? SelectedMemberId { get; set; }
    }
}
