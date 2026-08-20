using Insert.Worker;
using Insert.Infrastructure;
using Insert.Infrastructure.Stories;
using Insert.Application.Stories;
using Microsoft.EntityFrameworkCore;
using Insert.Media;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<InsertDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddScoped<IIngestRepository, IngestRepository>();
builder.Services.AddScoped<IngestService>();

builder.Services.AddScoped<IMediaProcessor, FfmpegMediaProcessor>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();