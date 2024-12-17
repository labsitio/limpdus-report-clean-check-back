
using ClosedXML.Excel;
using LimpidusMongoDB.Application.Contracts;
using LimpidusMongoDB.Application.Contracts.Requests;
using LimpidusMongoDB.Application.Enums.Errors;
using LimpidusMongoDB.Application.Helpers;
using LimpidusMongoDB.Application.Services.Interfaces;

namespace LimpidusMongoDB.Application.Services
{
    public class SpreadsheetService : ISpreadsheetService
    {
        public Result GenerateSpreadsheetAsync(SpreadsheetRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add(request.SheetName);

                for (int i = 0; i < request.Data.Length; i++)
                {
                    var row = request.Data[i];
                    for (int j = 0; j < row.Length; j++)
                    {
                        worksheet.Cell(_getLetterByIndex(j) + (i + 1)).Value = row[j];
                        worksheet.Cell(_getLetterByIndex(j) + (i + 1)).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                        worksheet.Cell(_getLetterByIndex(j) + (i + 1)).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);                        
                    }
                }

                worksheet.Columns().AdjustToContents();
                workbook.SaveAs(request.Name);

                return Result.Ok(data: request.Name);
            }
            catch (Exception)
            {
                return Result.Error(ApplicationErrors.Application_Error_General.Description());
            }
        }

        private string _getLetterByIndex(int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            var letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return letters[index].ToString();
        }

    }
}