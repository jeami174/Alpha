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
            ImageName = entity.ImageName,
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

            MemberModels = entity.Members
                .Select(m => _memberFactory.Create(m))
                .ToList()
        };
    }
}
