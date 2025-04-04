using Business.Models;
using Data.Entities;

namespace Business.Factories;

public class StatusFactory
{
    public StatusModel Create(StatusEntity entity)
    {
        return new StatusModel
        {
            Id = entity.Id,
            StatusName = entity.StatusName,
            ProjectCount = entity.Projects?.Count ?? 0
        };
    }

    public StatusEntity Create(StatusFormModel form)
    {
        return new StatusEntity
        {
            StatusName = form.StatusName
        };
    }

    public StatusEntity Update(StatusEntity entity, EditStatusForm form)
    {
        entity.StatusName = form.StatusName;
        return entity;
    }
}
