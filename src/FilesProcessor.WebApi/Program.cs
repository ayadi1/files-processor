using FilesProcessor.WebApi.Core.Configurations;
using FilesProcessor.WebApi.Core.Exceptions.Handlers;
using FilesProcessor.WebApi.Core.Options.Upload;
using FilesProcessor.WebApi.Infrastructure;
using Hangfire;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var upload = builder.Configuration
    .GetSection(UploadOptions.SectionName).Get<UploadOptions>()
    ?? throw new InvalidOperationException("Upload section missing");

builder.WebHost.ConfigureKestrel(k => k.Limits.MaxRequestBodySize = upload.MaxFileBytes);

// register health check
builder.Services
        .AddHealthChecks();

// register options with there validations
builder.Services
        .AddOptions<UploadOptions>()
        .Bind(builder.Configuration.GetSection(UploadOptions.SectionName))
        .ValidateOnStart();

builder.Services
        .AddSingleton<IValidateOptions<UploadOptions>, ValidateUploadOptions>();
builder.Services
        .AddSingleton<IConfigureOptions<FormOptions>, ConfigureUploadFormLimits>();

// register service


// register logs service
builder.Host.UseSerilog((context, services, lc) => lc
    .ReadFrom.Configuration(context.Configuration)   // config-driven
    .ReadFrom.Services(services));                // lets sinks resolve DI

// register exceptions handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// register controller
builder.Services.AddControllers();

// register MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, _) =>
    {
        document.Info.Title = "Files Processor API";
        document.Info.Description = "Upload files, generate variants, and download processed results.";
        document.Info.Contact = new OpenApiContact { Name = "Oussama" };
        return Task.CompletedTask;
    });
});

// register Infrastructure
builder.Services.RegisterInfrastructure();

var app = builder.Build();

// Map Hangfire
app.UseHangfireDashboard();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseExceptionHandler();
app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
