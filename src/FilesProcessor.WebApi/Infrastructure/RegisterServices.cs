using FilesProcessor.WebApi.Application.Processing;
using FilesProcessor.WebApi.Infrastructure.Processing;
using FilesProcessor.WebApi.Infrastructure.Storage;
using FilesProcessor.WebApi.Storage;
using Hangfire;
using Hangfire.Storage.SQLite;

namespace FilesProcessor.WebApi.Infrastructure;

public static class RegisterServices
{
    public static IServiceCollection RegisterInfrastructure(this IServiceCollection services)
    {

        // register database
        services
            .AddDbContext<AppDbContext>();

        // add infra service
        services.AddSingleton<IFileProcessor, FileProcessor>();
        services.AddSingleton<IFileStorage, LocalDiskFileStorage>();
        services.AddScoped<IProcessingQueue, HangfireProcessingQueue>();

        // register Hangfire services
        services.AddHangfire(c => c.UseSQLiteStorage("hangfire.db"));
        services.AddHangfireServer();

        return services;
    }
}
