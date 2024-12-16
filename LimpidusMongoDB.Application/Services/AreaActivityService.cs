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

        public async Task<Result> GetByProjectIdAsync(int legacyProjectId)
        {
            try
            {
                var responseList = await FindByFilterAsync(Builders<AreaActivityEntity>.Filter.Eq(x => x.ProjectId, legacyProjectId));

                return Result.Ok(data: responseList);
            }
            catch (Exception)
            {
                return Result.Error(ApplicationErrors.Application_Error_General.Description());
            }
        }

        public async Task<Result> GetByProjectIdAndEmployeeIdAsync(int legacyProjectId, string employeeId, CancellationToken cancellationToken = default)
        {
            try
            {
                var mongoFilter = Builders<AreaActivityEntity>.Filter;
                var filter = mongoFilter.Eq(x => x.ProjectId, legacyProjectId) & mongoFilter.Eq(x => x.EmployeeId, employeeId);
                var responseList = await FindByFilterAsync(filter, cancellationToken);

                return Result.Ok(data: responseList);
            }
            catch (Exception)
            {
                return Result.Error(ApplicationErrors.Application_Error_General.Description());
            }
        }

        public async Task<Result> GetItemsByAreaIsAsync(string areaId, CancellationToken cancellationToken = default)
        {
            try
            {
                var area = await _areaActivityRepository.FindByIdAsync(areaId, cancellationToken);
                if (area == null)
                    return Result.Error(ProjectErrors.Project_Error_NotFound.Description());

                return Result.Ok(data: area.Items);
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
                if (projectId.HasValue && allIds.Any())
                {
                    var mongoFilter = Builders<AreaActivityEntity>.Filter;
                    var filter = mongoFilter.Eq(x => x.ProjectId, projectId) & mongoFilter.Nin("Id", allIds);

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
                    }

                    if (await _areaActivityRepository.Exists(BaseEntity.FindByIdDefinition<AreaActivityEntity>(request.Id), cancellationToken))
                    {
                        await _areaActivityRepository.UpdateOneAsync(request.Id, areaActivityEntity.GetUpdateDefinition(), cancellationToken);
                    }
                    else
                    {
                        await _areaActivityRepository.InsertOneAsync(areaActivityEntity, cancellationToken);
                    }
                }

                return Result.Ok();
            }
            catch (Exception)
            {
                return Result.Error(ApplicationErrors.Application_Error_General.Description());
            }
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
