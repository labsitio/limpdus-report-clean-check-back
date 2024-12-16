using LimpidusMongoDB.Application.Data;
using LimpidusMongoDB.Application.Data.Repositories;
using LimpidusMongoDB.Application.Data.Repositories.Interfaces;
using LimpidusMongoDB.Application.Services;
using LimpidusMongoDB.Application.Services.Interfaces;

namespace LimpidusMongoDB.Api.Configurations
{
    public static class ServicesConfiguration
    {
        public static void AddServices(this IServiceCollection services)
        {
            services.AddSingleton<LimpidusContextDB>();
            services.AddScoped<IAreaActivityRepository, AreaActivityRepository>();
            services.AddScoped<IAreaActivityService, AreaActivityService>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IOperationalTaskRepository, OperationalTaskRepository>();
            services.AddScoped<IOperationalTaskService, OperationalTaskService>();
            services.AddScoped<IItemOperationalTaskRepository, ItemOperationalTaskRepository>();
            services.AddScoped<IHistoryRepository, HistoryRepository>();
            services.AddScoped<IHistoryService, HistoryService>();
            services.AddScoped<ISpreadsheetService, SpreadsheetService>();
            services.AddScoped<IItemHistoryRepository, ItemHistoryRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IJustificationRepository, JustificationRepository>();
            services.AddScoped<IReportService, ReportService>();
        }
    }
}