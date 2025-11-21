using OpenRA.Converter.Core.Interfaces;
using OpenRA.Converter.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Services Registration ---

// 1. Reference Handling
builder.Services.AddSingleton<IReferenceRegistry, ReferenceRegistry>();
builder.Services.AddScoped<IReferenceIngestionService, ReferenceIngestionService>();

// 2. Decision Tree Handling
builder.Services.AddScoped<IDecisionTreeService, DecisionTreeService>();

// 3. C# Code Generation
builder.Services.AddScoped<ITraitSynthesisService, TraitSynthesisService>();
builder.Services.AddScoped<ICodeWriter, CSharpCodeWriter>();

// 4. YAML Code Generation (New)
builder.Services.AddScoped<IYamlSynthesisService, YamlSynthesisService>();
builder.Services.AddScoped<IYamlCodeWriter, YamlCodeWriter>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();