using LimpidusMongoDB.Application.Contracts;
using LimpidusMongoDB.Application.Contracts.Responses;
using LimpidusMongoDB.Application.Data.Repositories.Interfaces;
using LimpidusMongoDB.Application.Enums.Errors;
using LimpidusMongoDB.Application.Helpers;
using LimpidusMongoDB.Application.Services.Interfaces;

namespace LimpidusMongoDB.Application.Services
{
    public class OperationalTaskService : IOperationalTaskService
    {
        private readonly IOperationalTaskRepository _operationalTaskRepository;
        private readonly IItemOperationalTaskRepository _itemRepository;

        public OperationalTaskService(IOperationalTaskRepository operationalTaskRepository, IItemOperationalTaskRepository itemRepository)
        {
            _operationalTaskRepository = operationalTaskRepository;
            _itemRepository = itemRepository;
        }

        public async Task<Result> GetAllOperationalTasks()
        {
            try
            {
                var operationalTasks = await _operationalTaskRepository.FindAllAsync();

                if (!operationalTasks?.Any() ?? true)
                    return Result.Error(OperationalTaskErrors.OperationalTask_Error_NotFound.Description());

                var operationalTaskResponseList = new List<OperationalTaskResponse>();

                foreach (var operationalTask in operationalTasks)
                {
                    var itemList = await _itemRepository.FindByOperationalTaskIdAsync(operationalTask.Id.ToString());
                    if (itemList?.Any() ?? false)
                        operationalTaskResponseList.Add(new OperationalTaskResponse(operationalTask, itemList));
                }

                return Result.Ok(data: operationalTaskResponseList);
            }
            catch (Exception ex)
            {
                return Result.Error(ApplicationErrors.Application_Error_General.Description());
            }
        }
    }
}