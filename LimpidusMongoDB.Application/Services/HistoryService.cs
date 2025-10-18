using LimpidusMongoDB.Application.Contracts;
using LimpidusMongoDB.Application.Contracts.Requests;
using LimpidusMongoDB.Application.Contracts.Responses;
using LimpidusMongoDB.Application.Data.Entities;
using LimpidusMongoDB.Application.Data.Repositories.Interfaces;
using LimpidusMongoDB.Application.Enums.Errors;
using LimpidusMongoDB.Application.Helpers;
using LimpidusMongoDB.Application.Services.Interfaces;
using MongoDB.Driver;
using ZstdSharp.Unsafe;

namespace LimpidusMongoDB.Application.Services
{
    public class HistoryService : IHistoryService
    {
        private readonly IHistoryRepository _historyRepository;
        private readonly IEmployeeService _employeeService;
        private readonly ISpreadsheetService _spreadsheetService;

        public HistoryService(IHistoryRepository historyRepository, IEmployeeService employeeService, ISpreadsheetService spreadsheetService)
        {
            _historyRepository = historyRepository;
            _employeeService = employeeService;
            _spreadsheetService = spreadsheetService;
        }

        public async Task<Result> GetByProjectIdAndEmployeeIdAsync(int legacyProjectId, string employeeId, CancellationToken cancellationToken = default)
        {
            try
            {
                var mongoFilter = Builders<HistoryEntity>.Filter;
                var filter = mongoFilter.Eq(x => x.ProjectId, legacyProjectId) & mongoFilter.Eq(x => x.EmployeeId, employeeId);
                var histories = await _historyRepository.FindAsync(filter, cancellationToken);

                var responseList = histories.Select(x => new HistoryResponse(x));

                return Result.Ok(data: responseList);
            }
            catch (Exception)
            {
                return Result.Error(ApplicationErrors.Application_Error_General.Description());
            }
        }
        public async Task<Result> GetByProjectIdAsync(int legacyProjectId, HistoryQueryRequest query, CancellationToken cancellationToken = default)
        {
            if (query.DateStart == null || query.DateEnd == null)
            {
                var temp = DateTime.Now;
                query.DateStart = temp.AddDays(-30);
                query.DateEnd = temp;
            }

            try
            {
                var mongoFilter = Builders<HistoryEntity>.Filter;
                var filter =
                    mongoFilter.Eq(x => x.ProjectId, legacyProjectId) &
                    (mongoFilter.Lte(x => x.EndDate, query.DateEnd) & mongoFilter.Gte(x => x.CreatedDate, query.DateStart))
                    & (string.IsNullOrEmpty(query.Department) ? mongoFilter.Empty : mongoFilter.Regex(x => x.AreaTaskName, new MongoDB.Bson.BsonRegularExpression(query.Department)))
                    & (string.IsNullOrEmpty(query.Employee) ? mongoFilter.Empty : mongoFilter.Or(
                        mongoFilter.Regex(x => x.User.Name, new MongoDB.Bson.BsonRegularExpression(query.Employee)),
                        mongoFilter.Regex(x => x.User.LastName, new MongoDB.Bson.BsonRegularExpression(query.Employee))
                        ))
                    & (!query.Status.HasValue ? mongoFilter.Empty :
                        (query.Status.Value ? mongoFilter.Where(y => y.Justification.Information == null) : mongoFilter.Where(y => y.Justification.Information != null)));

                var histories = await _historyRepository.FindAsync(filter, cancellationToken);

                var responseList = histories.Select(x => new HistoryResponse(x)).ToList();

                var results = responseList.Select(x => new HistoryAuditResponse()
                {
                    Id = x.Id,
                    Department = x.AreaTaskName,
                    Employee = x.User.Name + " " + x.User.LastName,
                    DateStart = x.CreatedDate,
                    DateEnd = x.EndDate,
                    Justification = x.Justification != null ? new JustificationResponse(x.Justification?.Information, x.Justification?.Reason) : null,
                    Status = x.Justification?.Information == null
                }).ToList();

                return Result.Ok(data: new {
                    data = results,
                    departments = results.Select(x => x.Department).Distinct().ToList(),
                    employees = results.Select(x => x.Employee).Distinct().ToList()
                });
            }
            catch (Exception)
            {
                return Result.Error(ApplicationErrors.Application_Error_General.Description());
            }
        }

        public async Task<Result> GetHistoriesInSpreadsheet(int legacyId, HistoryQueryRequest query, CancellationToken cancellationToken = default)
        {
            var result = await GetByProjectIdAsync(legacyId, query, cancellationToken);

            var history = (IList<HistoryAuditResponse>)result.Data;
            var arrangedData = history.Select(x =>
            {
                string[] obj = new string[6];
                obj[0] = x.Department;
                obj[1] = x.Employee;
                obj[2] = x.DateStart.ToString();
                obj[3] = x.DateEnd.ToString();
                obj[4] = x.Duration.ToString();
                obj[5] = x.Status ? "Concluído" : "Pendente" ;

                return obj;
            }).ToArray();

            var data = new string[arrangedData.Length + 1][];
            data[0] = new [] { "Área", "Funcionário", "Início", "Conclusão", "Duração", "Status" };
            for (int i = 0; i < arrangedData.Length; i++)
            {
                data[i + 1] = arrangedData[i];
            }

            if(history.Count == 0)
            {
                return Result.Error(ApplicationErrors.Application_Error_General.Description());
            }

            var spreadsheet = _spreadsheetService.GenerateSpreadsheetAsync(new SpreadsheetRequest
            {
                Data = data,
                Name = $"Histories-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.xlsx",
                SheetName = $"Histories-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}"
            }, cancellationToken);

            return Result.Ok(data: spreadsheet.Data);
        }
        public async Task<Result> SaveAsync(IEnumerable<HistoryRequest> requests, CancellationToken cancellationToken = default)
        {
            try
            {
                foreach (var request in requests)
                {
                    var historyEntity = new HistoryEntity
                    {
                        ProjectId = request.ProjectId,
                        EmployeeId = request.EmployeeId,
                        AreaTaskId = request.AreaTaskId,
                        AreaTaskName = request.AreaTaskName,
                        EndDate = request.EndDate,
                        User = request.User != null ? new HistoryUserEntity
                        {
                            Name = request.User.Name,
                            LastName = request.User.LastName,
                        } : null,
                        Items = request.Items?.Select(x => new HistoryItemEntity
                        {
                            Id = x.Id,
                            Name = x.Name,
                            OrderBy = x.OrderBy,
                            EndDate = x.EndDate,
                            Performed = x.Performed
                        }),
                        Justification = request.Justification != null ? new HistoryJustificationEntity
                        {
                            Information = request.Justification.Information,
                            Reason = request.Justification.Reason,
                        } : null
                    };

                    await _historyRepository.InsertOneAsync(historyEntity, cancellationToken);
                }

                return Result.Ok();
            }
            catch (Exception)
            {
                return Result.Error(ApplicationErrors.Application_Error_General.Description());
            }
        }
    }
}
