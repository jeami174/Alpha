using Business.Models;
using Data.Entities;
using Domain.Models;

namespace Business.Factories;
public class ProjectFactory
{
    private readonly ClientFactory _clientFactory;
    private readonly MemberFactory _memberFactory;
    public ProjectFactory(ClientFactory clientFactory, MemberFactory memberFactory)
    {
        _clientFactory = clientFactory;
        _memberFactory = memberFactory;
    }
    public ProjectModel Create(ProjectEntity entity)
    {
        return new ProjectModel
        {
            Id = entity.Id,
            ImageName = string.IsNullOrWhiteSpace(entity.ImageName)
                ? "uploads/projects/avatars/default.svg"
                : entity.ImageName.Replace("\\", "/"),
            ProjectName = entity.ProjectName,
            Description = entity.Description,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            Created = entity.Created,
            Budget = entity.Budget,
            ClientModel = entity.Client != null
                ? _clientFactory.Create(entity.Client)
                : new ClientModel(),
            MemberModels = entity.Members != null
                ? entity.Members.Select(m => _memberFactory.Create(m)).ToList()
                : new List<MemberModel>()
        };
    }
    public ProjectEntity Create(
        AddProjectForm form,
        ClientEntity client,
        List<MemberEntity> members,
        string imageName)
    {
        return new ProjectEntity
        {
            ProjectName = form.ProjectName,
            Description = form.Description,
            StartDate = form.StartDate,
            EndDate = form.EndDate,
            Budget = form.Budget,
            Created = DateTime.Now,
            ImageName = imageName,
            Client = client,
            ClientId = client.ClientId,
            Members = members
        };
    }
    public void Update(
        ProjectEntity entity,
        EditProjectForm form,
        ClientEntity client,
        List<MemberEntity> members)
    {
        entity.ProjectName = form.ProjectName;
        entity.Description = form.Description;
        entity.StartDate = form.StartDate;
        entity.EndDate = form.EndDate;
        entity.Budget = form.Budget;
        entity.Client = client;

        entity.Members.Clear();
        foreach (var m in members)
        {
            entity.Members.Add(m);
        }
    }
    public ProjectEntity Create(ProjectModel model)
    {
        return new ProjectEntity
        {
            Id = model.Id,
            ProjectName = model.ProjectName,
            Description = model.Description,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            Budget = model.Budget,
            Created = model.Created,
            ClientId = model.ClientModel.ClientId,
        };
    }
}
