using LimpidusMongoDB.Application.Contracts;
using LimpidusMongoDB.Application.Contracts.Requests;
using LimpidusMongoDB.Application.Contracts.Responses;
using LimpidusMongoDB.Application.Data.Entities;
using LimpidusMongoDB.Application.Data.Repositories.Interfaces;
using LimpidusMongoDB.Application.Enums.Errors;
using LimpidusMongoDB.Application.Helpers;
using LimpidusMongoDB.Application.Services.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace LimpidusMongoDB.Application.Services
{
    public class AreaActivityService : IAreaActivityService
    {
        private readonly IAreaActivityRepository _areaActivityRepository;

        public AreaActivityService(IAreaActivityRepository areaActivityRepository)
        {
            _areaActivityRepository = areaActivityRepository;
        }

        public async Task<Result> GetByProjectIdAsync(int legacyProjectId, DateTime? referenceDate = null)
        {
            try
            {
                var responseList = await FindByFilterAsync(Builders<AreaActivityEntity>.Filter.Eq(x => x.ProjectId, legacyProjectId));
                if (referenceDate.HasValue)
                    responseList = AreaActivityScheduleFilter.FilterAreasByReferenceDate(responseList, referenceDate.Value);

                responseList = DedupeAreasForList(responseList);

                return Result.Ok(data: responseList);
            }
            catch (Exception)
            {
                return Result.Error(ApplicationErrors.Application_Error_General.Description());
            }
        }

        public async Task<Result> GetByProjectIdAndEmployeeIdAsync(
            int legacyProjectId,
            string employeeId,
            DateTime? referenceDate = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var mongoFilter = Builders<AreaActivityEntity>.Filter;
                var filter = mongoFilter.Eq(x => x.ProjectId, legacyProjectId) & mongoFilter.Eq(x => x.EmployeeId, employeeId);
                var responseList = await FindByFilterAsync(filter, cancellationToken);
                if (referenceDate.HasValue)
                    responseList = AreaActivityScheduleFilter.FilterAreasByReferenceDate(responseList, referenceDate.Value);

                responseList = DedupeAreasForList(responseList);

                return Result.Ok(data: responseList);
            }
            catch (Exception)
            {
                return Result.Error(ApplicationErrors.Application_Error_General.Description());
            }
        }

        public async Task<Result> GetItemsByAreaIsAsync(string areaId, DateTime? referenceDate = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var area = await _areaActivityRepository.FindByIdAsync(areaId, cancellationToken);
                if (area == null)
                    return Result.Error(ProjectErrors.Project_Error_NotFound.Description());

                var itemsSource = await ResolveBestItemSourceAreaAsync(area, cancellationToken);

                if (!referenceDate.HasValue)
                    return Result.Ok(data: itemsSource.Items);

                var day = (short)referenceDate.Value.Date.DayOfWeek;
                var items = itemsSource.Items?
                    .Where(i => AreaActivityScheduleFilter.FrequencyAllowsDayForItem(i.Frequency, itemsSource.Frequency, day))
                    .ToList();

                return Result.Ok(data: items);
            }
            catch (Exception)
            {
                return Result.Error(ApplicationErrors.Application_Error_General.Description());
            }
        }

        public async Task<Result> SaveAsync(IEnumerable<AreaActivityRequest> requests, CancellationToken cancellationToken = default)
        {
            try
            {
                // Delete
                var allIds = requests?.Where(x => !string.IsNullOrWhiteSpace(x.Id)).Select(x => ObjectId.Parse(x.Id)).ToArray();
                var projectId = requests?.FirstOrDefault()?.ProjectId;
                var requestList = requests?.ToList() ?? [];
                if (projectId.HasValue && allIds.Any() && requestList.Count > 1)
                {
                    var mongoFilter = Builders<AreaActivityEntity>.Filter;
                    // Usar x => x.Id para gerar filtro em _id; "Id" como string não bate com o BSON do documento.
                    var filter = mongoFilter.Eq(x => x.ProjectId, projectId) & mongoFilter.Nin(x => x.Id, allIds);

                    var distinctEmployeeIds = requestList
                        .Where(x => !string.IsNullOrWhiteSpace(x.EmployeeId))
                        .Select(x => x.EmployeeId)
                        .Distinct(StringComparer.Ordinal)
                        .ToList();

                    // Um único funcionário no body: remove só documentos desse funcionário no projeto que não vieram no POST.
                    // Vários employeeId no mesmo array: mantém sincronização “todo o projeto” (distribuição entre funcionários).
                    if (distinctEmployeeIds.Count == 1)
                        filter &= mongoFilter.Eq(x => x.EmployeeId, distinctEmployeeIds[0]);

                    await _areaActivityRepository.DeleteManyAsync(filter, cancellationToken);
                }

                // Insert/Update
                foreach (var request in requests)
                {
                    var areaActivityEntity = new AreaActivityEntity
                    {
                        Name = request.Name,
                        Description = request.Description,
                        QuickTask = request.QuickTask,
                        TotalM2 = request.TotalM2,
                        EmployeeId = request.EmployeeId,
                        HeaderId = request.HeaderId,
                        OrderBy = request.OrderBy,
                        Frequency = request.Frequency != null ? new AreaActivityFrequencyEntity
                        {
                            Type = request.Frequency.Type,
                            WeekDays = request.Frequency.WeekDays,
                        } : null,
                        Items = request.Items?.Select(x => new AreaActivityItemEntity
                        {
                            ItemId = x.Id,
                            Name = x.Name,
                            OrderBy = x.OrderBy,
                            Frequency = x.Frequency != null ? new AreaActivityFrequencyEntity
                            {
                                Type = x.Frequency.Type,
                                WeekDays = x.Frequency.WeekDays,
                            } : null,
                        }),
                        ProjectId = request.ProjectId,
                    };

                    if (!string.IsNullOrWhiteSpace(request.Id))
                    {
                        areaActivityEntity.SetObjectId(request.Id);
                        
                        // Verifica se existe para atualizar
                        if (await _areaActivityRepository.Exists(BaseEntity.FindByIdDefinition<AreaActivityEntity>(request.Id), cancellationToken))
                        {
                            await _areaActivityRepository.UpdateOneAsync(request.Id, areaActivityEntity.GetUpdateDefinition(), cancellationToken);
                        }
                        else
                        {
                            // Id informado mas não existe, insere como novo
                            await _areaActivityRepository.InsertOneAsync(areaActivityEntity, cancellationToken);
                        }
                    }
                    else
                    {
                        // Id vazio = novo registro, apenas insere
                        await _areaActivityRepository.InsertOneAsync(areaActivityEntity, cancellationToken);
                    }
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                // Retorna a mensagem de erro real para facilitar debug
                var errorMessage = ex.InnerException != null 
                    ? $"{ApplicationErrors.Application_Error_General.Description()}: {ex.InnerException.Message}" 
                    : $"{ApplicationErrors.Application_Error_General.Description()}: {ex.Message}";
                return Result.Error(errorMessage);
            }
        }

        /// <summary>
        /// Vários POSTs com <c>id</c> vazio ou migrações repetidas criam o mesmo <c>headerId</c> (WORK_AREA_ID) em documentos distintos.
        /// Mantém um documento por chave (headerId ou nome se sem headerId), priorizando quem tem mais tarefas.
        /// </summary>
        private static List<AreaActivityResponse> DedupeAreasForList(IEnumerable<AreaActivityResponse> areas)
        {
            var list = areas?.ToList() ?? new List<AreaActivityResponse>();
            if (list.Count <= 1)
                return list;

            static string DedupeKey(AreaActivityResponse a) =>
                !string.IsNullOrWhiteSpace(a.HeaderId)
                    ? $"h:{a.HeaderId.Trim()}"
                    : $"n:{a.Name?.Trim() ?? string.Empty}";

            return list
                .GroupBy(DedupeKey)
                .Select(g => g
                    .OrderByDescending(x => x.Items?.Count() ?? 0)
                    .ThenByDescending(x => x.Id, StringComparer.Ordinal)
                    .First())
                .OrderBy(a => a.OrderBy)
                .ThenBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>
        /// Se o documento apontado pelo QR não tem <c>items</c>, tenta outro com o mesmo projeto, <c>headerId</c> e funcionário (duplicados no Mongo).
        /// </summary>
        private async Task<AreaActivityEntity> ResolveBestItemSourceAreaAsync(
            AreaActivityEntity primary,
            CancellationToken cancellationToken)
        {
            if (primary.Items?.Any() == true)
                return primary;

            if (string.IsNullOrWhiteSpace(primary.HeaderId))
                return primary;

            var filter = Builders<AreaActivityEntity>.Filter.Eq(x => x.ProjectId, primary.ProjectId)
                         & Builders<AreaActivityEntity>.Filter.Eq(x => x.HeaderId, primary.HeaderId);

            if (!string.IsNullOrWhiteSpace(primary.EmployeeId))
                filter &= Builders<AreaActivityEntity>.Filter.Eq(x => x.EmployeeId, primary.EmployeeId);

            var siblings = (await _areaActivityRepository.FindAsync(filter, cancellationToken))?.ToList()
                           ?? new List<AreaActivityEntity>();
            var best = siblings
                .OrderByDescending(x => x.Items?.Count() ?? 0)
                .FirstOrDefault();

            return best?.Items?.Any() == true ? best : primary;
        }

        private async Task<IEnumerable<AreaActivityResponse>> FindByFilterAsync(FilterDefinition<AreaActivityEntity> filter, CancellationToken cancellationToken = default)
        {
            var areasActivities = await _areaActivityRepository.FindAsync(filter, cancellationToken);
            if (areasActivities?.Any() == false) return Enumerable.Empty<AreaActivityResponse>();

            return areasActivities.Select(areaActivity => new AreaActivityResponse
            {
                Id = areaActivity.Id.ToString(),
                Name = areaActivity.Name,
                Description = areaActivity.Description,
                QuickTask = areaActivity.QuickTask,
                TotalM2 = areaActivity.TotalM2,
                EmployeeId = areaActivity.EmployeeId,
                HeaderId = areaActivity.HeaderId,
                OrderBy = areaActivity.OrderBy,
                Frequency = areaActivity.Frequency != null ? new AreaActivityFrequencyResponse
                {
                    Type = areaActivity.Frequency.Type,
                    WeekDays = areaActivity.Frequency.WeekDays,
                } : null,
                Items = areaActivity.Items?.Select(x => new AreaActivityItemResponse
                {
                    Id = x.ItemId,
                    Name = x.Name,
                    OrderBy = x.OrderBy,
                    Frequency = x.Frequency != null ? new AreaActivityFrequencyResponse
                    {
                        Type = x.Frequency.Type,
                        WeekDays = x.Frequency.WeekDays,
                    } : null
                }),
                ProjectId = areaActivity.ProjectId,
            });
        }
    }
}
