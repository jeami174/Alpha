using Business.Models;
using Domain.Models;

namespace Business.Interfaces;

public interface IProjectService
{

    Task<ServiceResult<ProjectModel>> CreateAsync(
        AddProjectForm form,
        int clientId,
        List<int> memberIds,
        string imageName);

    Task<ServiceResult<IEnumerable<ProjectModel>>> GetAllProjectsAsync(string? sortBy = null);

    Task<ServiceResult<ProjectModel>> GetProjectByIdAsync(string id);

    Task<ServiceResult<ProjectModel>> UpdateProjectAsync(
        EditProjectForm form,
        int clientId,
        List<int> memberIds,
        string? newImageName);

    Task<ServiceResult<bool>> DeleteProjectAsync(string id);

    Task<IEnumerable<ProjectModel>> GetNewProjectsAsync(DateTime since);
}
