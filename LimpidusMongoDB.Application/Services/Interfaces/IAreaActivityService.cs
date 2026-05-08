using LimpidusMongoDB.Application.Contracts;
using LimpidusMongoDB.Application.Contracts.Requests;

namespace LimpidusMongoDB.Application.Services.Interfaces
{
    public interface IAreaActivityService
    {
        /// <param name="referenceDate">Se informado (só a data), filtra tarefas por weekDays (0=Dom…6=Sáb).</param>
        Task<Result> GetByProjectIdAsync(int legacyProjectId, DateTime? referenceDate = null);

        /// <param name="referenceDate">Se informado (só a data), filtra tarefas por weekDays (0=Dom…6=Sáb).</param>
        Task<Result> GetByProjectIdAndEmployeeIdAsync(int legacyProjectId, string employeeId, DateTime? referenceDate = null, CancellationToken cancellationToken = default);

        /// <param name="referenceDate">Se informado (só a data), filtra itens por weekDays.</param>
        Task<Result> GetItemsByAreaIsAsync(string areaId, DateTime? referenceDate = null, CancellationToken cancellationToken = default);

        Task<Result> SaveAsync(IEnumerable<AreaActivityRequest> request, CancellationToken cancellationToken = default);
    }
}
