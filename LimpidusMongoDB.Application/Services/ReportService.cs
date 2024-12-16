using LimpidusMongoDB.Application.Contracts;
using LimpidusMongoDB.Application.Contracts.Requests;
using LimpidusMongoDB.Application.Enums.Errors;
using LimpidusMongoDB.Application.Helpers;
using LimpidusMongoDB.Application.Services.Interfaces;
using LimpidusMongoDB.Application.Settings;
using Microsoft.Extensions.Configuration;

namespace LimpidusMongoDB.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly EmailSettings _emailSettings;

        public ReportService(IConfiguration configuration)
        {
            _emailSettings = configuration.GetSection(nameof(EmailSettings)).Get<EmailSettings>();
        }

        public async Task<Result> SendReportAsync(ReportRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var attachments = new List<(string, Stream)>();

                if (request.File != null)
                {
                    var memoryStream = new MemoryStream();
                    request.File.CopyTo(memoryStream);

                    attachments.Add((request.File.FileName, memoryStream));
                }

                await EmailSender.SendAsync(
                    _emailSettings,
                    new List<string> { "diego.morais@labsit.io" },
                    new List<string> { "dimas@labsit.io" },
                    "Notificação de Incidências/Problemas/Impedimentos",
                    request.Description,
                    attachments,
                    cancellationToken: cancellationToken);

                return Result.Ok();
            }
            catch (Exception)
            {
                return Result.Error(ApplicationErrors.Application_Error_General.Description());
            }
        }
    }
}
