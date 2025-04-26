using Business.Models;
using Data.Entities;
using Domain.Models;

namespace Business.Factories;

public class ProjectFactory
{
    private readonly ClientFactory _clientFactory;
    private readonly StatusFactory _statusFactory;
    private readonly MemberFactory _memberFactory;

    public ProjectFactory(ClientFactory clientFactory, StatusFactory statusFactory, MemberFactory memberFactory)
    {
        _clientFactory = clientFactory;
        _statusFactory = statusFactory;
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
            StatusModel = entity.Status != null
                ? _statusFactory.Create(entity.Status)
                : new StatusModel(),
            MemberModels = entity.Members != null
                ? entity.Members.Select(m => _memberFactory.Create(m)).ToList()
                : new List<MemberModel>()
        };
    }

    // 2) Lägg till den här overloaden utan status
    public ProjectEntity Create(
        AddProjectForm form,
        ClientEntity client,
        List<MemberEntity> members,
        string imageName)
    {
        var project = new ProjectEntity
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

        return project;
    }

    public ProjectEntity Create(AddProjectForm form, ClientEntity client, StatusEntity status, List<MemberEntity> members, string imageName)
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
            Status = status,
            Members = members
        };
    }

    public void Update(
        ProjectEntity entity,
        EditProjectForm form,
        ClientEntity client,
        StatusEntity status,
        List<MemberEntity> members)
    {
        // Övriga fält
        entity.ProjectName = form.ProjectName;
        entity.Description = form.Description;
        entity.StartDate = form.StartDate;
        entity.EndDate = form.EndDate;
        entity.Budget = form.Budget;
        entity.Client = client;
        entity.Status = status;

        // Här: rensa gamla och addera nya
        entity.Members.Clear();
        foreach (var m in members)
        {
            // EF Core är redan tracking för dessa MemberEntity, så det här sätter
            // deras FK (ProjectId) till detta projects Id, och tar bort FK på
            // de som blivit bortplockade.
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
            StatusId = model.StatusModel.Id,
        };
    }
}
