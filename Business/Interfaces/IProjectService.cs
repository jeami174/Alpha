using Business.Models;
using Domain.Models;

namespace Business.Interfaces
{
    /// <summary>
    /// Service-interface för hantering av projekt.
    /// </summary>
    public interface IProjectService
    {
        /// <summary>
        /// Skapar ett nytt projekt.
        /// </summary>
        /// <param name="form">Formulär med projektdata.</param>
        /// <param name="clientId">Kundens ID.</param>
        /// <param name="statusId">Statusens ID.</param>
        /// <param name="memberIds">Lista med medlems-ID:n.</param>
        /// <param name="imageName">Namn på bildfilen.</param>
        /// <returns>Ett ServiceResult som innehåller det skapade projektet.</returns>
        Task<ServiceResult<ProjectModel>> CreateAsync(
            AddProjectForm form,
            int clientId,
            int statusId,
            List<int> memberIds,
            string imageName);

        /// <summary>
        /// Hämtar alla projekt, med möjlighet att sortera och filtrera efter status.
        /// </summary>
        /// <param name="sortBy">
        /// Sorteringsparametern ("name", "created", "start" eller "end").
        /// Om null används standardordningen.
        /// </param>
        /// <param name="statusFilter">
        /// Filter för att begränsa projekten efter status (skiftlägesokänsligt).
        /// Om null returneras alla projekt.
        /// </param>
        /// <returns>Ett ServiceResult som innehåller en samling av projekt.</returns>
        Task<ServiceResult<IEnumerable<ProjectModel>>> GetAllProjectsAsync(
            string? sortBy = null,
            string? statusFilter = null);

        /// <summary>
        /// Hämtar ett enskilt projekt baserat på projektets ID.
        /// </summary>
        /// <param name="id">Projektets ID.</param>
        /// <returns>Ett ServiceResult som innehåller projektet om det hittas.</returns>
        Task<ServiceResult<ProjectModel>> GetProjectByIdAsync(string id);

        /// <summary>
        /// Uppdaterar ett befintligt projekt med de angivna värdena.
        /// </summary>
        /// <param name="form">Formulär med redigerade projektdata.</param>
        /// <param name="clientId">Kundens ID.</param>
        /// <param name="statusId">Statusens ID.</param>
        /// <param name="memberIds">Lista med medlems-ID:n.</param>
        /// <param name="newImageName">
        /// Det nya bildnamnet. Om null används befintligt värde från formuläret.
        /// </param>
        /// <returns>Ett ServiceResult som innehåller det uppdaterade projektet.</returns>
        Task<ServiceResult<ProjectModel>> UpdateProjectAsync(
            EditProjectForm form,
            int clientId,
            int statusId,
            List<int> memberIds,
            string? newImageName);

        /// <summary>
        /// Tar bort ett projekt baserat på dess ID.
        /// </summary>
        /// <param name="id">ID för projektet som ska tas bort.</param>
        /// <returns>Ett ServiceResult som innehåller true om radering lyckades.</returns>
        Task<ServiceResult<bool>> DeleteProjectAsync(string id);
    }
}
