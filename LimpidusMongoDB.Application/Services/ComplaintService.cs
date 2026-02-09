using System.Text;
using LimpidusMongoDB.Application.Contracts.Requests;
using LimpidusMongoDB.Application.Contracts.Responses;
using LimpidusMongoDB.Application.Helpers;
using LimpidusMongoDB.Application.Services.Interfaces;
using LimpidusMongoDB.Application.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LimpidusMongoDB.Application.Services
{
    public class ComplaintService : IComplaintService
    {
        private const int MaxRecipients = 20;
        private const string ReasonNoRecipients = "no_recipients";

        private readonly ISqlServerDataAccessFactory _sqlServerDataAccessFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ComplaintService> _logger;

        public ComplaintService(
            ISqlServerDataAccessFactory sqlServerDataAccessFactory,
            IConfiguration configuration,
            ILogger<ComplaintService> logger)
        {
            _sqlServerDataAccessFactory = sqlServerDataAccessFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ComplaintDataResponse?> GetDataAsync(int legacyProjectId, int legacyAreaId, CancellationToken cancellationToken = default)
        {
            var connectionString = _configuration.GetConnectionString("SqlServerDB");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                _logger.LogWarning("Connection string SqlServerDB not configured for complaint.");
                return null;
            }

            var dataAccess = _sqlServerDataAccessFactory.Create(connectionString);
            var projectName = await dataAccess.GetNomeProjetoAsync(legacyProjectId, cancellationToken);
            var areaName = await dataAccess.GetNomeAreaAsync(legacyAreaId, cancellationToken);

            if (string.IsNullOrWhiteSpace(projectName) && string.IsNullOrWhiteSpace(areaName))
                return null;

            return new ComplaintDataResponse
            {
                ProjectName = projectName ?? string.Empty,
                AreaName = areaName ?? string.Empty
            };
        }

        public async Task<ComplaintSendResponse> SendAsync(ComplaintSendRequest request, CancellationToken cancellationToken = default)
        {
            var connectionString = _configuration.GetConnectionString("SqlServerDB");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                _logger.LogError("Connection string SqlServerDB not configured for complaint.");
                throw new InvalidOperationException("Database configuration not available.");
            }

            var dataAccess = _sqlServerDataAccessFactory.Create(connectionString);

            var projectNameTask = dataAccess.GetNomeProjetoAsync(request.LegacyProjectId, cancellationToken);
            var areaNameTask = dataAccess.GetNomeAreaAsync(request.LegacyAreaId, cancellationToken);
            var mailTask = dataAccess.GetComplaintMailAsync(request.LegacyProjectId, cancellationToken);

            await Task.WhenAll(projectNameTask, areaNameTask, mailTask);

            var projectName = await projectNameTask ?? string.Empty;
            var areaName = await areaNameTask ?? string.Empty;
            var mailString = await mailTask;

            var recipients = NormalizeEmails(mailString);
            if (recipients.Count == 0)
            {
                _logger.LogInformation(
                    "Complaint with no recipients. LegacyProjectId={LegacyProjectId}, LegacyAreaId={LegacyAreaId}",
                    request.LegacyProjectId, request.LegacyAreaId);
                return new ComplaintSendResponse { Sent = false, Reason = ReasonNoRecipients };
            }

            var subject = $"{projectName} {areaName}".Trim();
            var bodyHtml = BuildBodyHtml(projectName, areaName, request.Problems ?? new List<string>(), request.Comments ?? string.Empty);

            var emailSettings = _configuration.GetSection(nameof(EmailSettings)).Get<EmailSettings>();
            if (emailSettings == null)
            {
                _logger.LogError("EmailSettings not configured for complaint.");
                throw new InvalidOperationException("Email configuration not available.");
            }

            foreach (var recipient in recipients)
            {
                await EmailSender.SendAsync(
                    emailSettings,
                    new[] { recipient },
                    Array.Empty<string>(),
                    subject,
                    bodyHtml,
                    cancellationToken: cancellationToken);
            }

            return new ComplaintSendResponse { Sent = true };
        }

        private static List<string> NormalizeEmails(string? mailString)
        {
            if (string.IsNullOrWhiteSpace(mailString))
                return new List<string>();

            return mailString
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(MaxRecipients)
                .ToList();
        }

        private static string BuildBodyHtml(string projectName, string areaName, List<string> problems, string comments)
        {
            var sb = new StringBuilder();
            sb.Append("<b>Projeto</b><br>").Append(projectName).Append("<br>\n<br><b>Área</b><br>").Append(areaName).Append("<br>\n<br><b>Escolha o problema encontrado</b><br>");
            foreach (var p in problems)
                sb.Append(p).Append("<br>");
            sb.Append("\n<br><b>Comentários</b><br>").Append(comments).Append("<br>\n");
            return sb.ToString();
        }
    }
}
