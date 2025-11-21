using OpenRA.Converter.Core.Interfaces;
using OpenRA.Converter.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Register the Reference Registry as a Singleton so data persists between API calls
builder.Services.AddSingleton<IReferenceRegistry, ReferenceRegistry>();

// Register the Ingestion Service
builder.Services.AddScoped<IReferenceIngestionService, ReferenceIngestionService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();