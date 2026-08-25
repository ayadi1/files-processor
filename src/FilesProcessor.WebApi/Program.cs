using FilesProcessor.WebApi.Core.Configurations;
using FilesProcessor.WebApi.Core.Exceptions.Handlers;
using FilesProcessor.WebApi.Core.Options.Upload;
using FilesProcessor.WebApi.Infrastructure;
using FilesProcessor.WebApi.Infrastructure.Storage;
using FilesProcessor.WebApi.Storage;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
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

// register database
builder.Services
        .AddDbContext<AppDbContext>();

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
builder.Services.AddSingleton<IFileStorage, LocalDiskFileStorage>();

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
builder.Services.AddOpenApi();

var app = builder.Build();

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
