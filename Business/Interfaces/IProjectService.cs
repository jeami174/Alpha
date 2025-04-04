using Business.Models;

public interface IProjectService
{
    Task<ServiceResult<bool>> CreateAsync(
        AddProjectForm form,
        int clientId,
        int statusId,
        List<int> memberIds,
        string? imageName = null);

    Task<ServiceResult<IEnumerable<ProjectModel>>> GetAllProjectsAsync(
        string? sortBy = null,
        string? statusFilter = null);

    Task<ServiceResult<ProjectModel>> GetProjectByIdAsync(string id);

    Task<ServiceResult<bool>> UpdateProjectAsync(
        string id,
        AddProjectForm form,
        int statusId,
        int clientId,
        List<int> memberIds);

    Task<ServiceResult<bool>> DeleteProjectAsync(string id);
}

