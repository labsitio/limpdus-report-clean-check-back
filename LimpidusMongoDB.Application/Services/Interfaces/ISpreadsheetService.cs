
using LimpidusMongoDB.Application.Contracts;
using LimpidusMongoDB.Application.Contracts.Requests;

namespace LimpidusMongoDB.Application.Services.Interfaces
{
    public interface ISpreadsheetService
    {
        Result GenerateSpreadsheetAsync(SpreadsheetRequest request, CancellationToken cancellationToken = default);   
    }
}