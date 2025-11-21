using OpenRA.Converter.Core.Interfaces;
using OpenRA.Converter.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Service Registration ---

// 1. References (Truth Source)
builder.Services.AddSingleton<IReferenceRegistry, ReferenceRegistry>();
builder.Services.AddScoped<IReferenceIngestionService, ReferenceIngestionService>();

// 2. Decision Tree (Parsing & Logic)
builder.Services.AddScoped<IDecisionTreeService, DecisionTreeService>();

// 3. Synthesis (Code Generation)
builder.Services.AddScoped<ITraitSynthesisService, TraitSynthesisService>();
builder.Services.AddScoped<ICodeWriter, CSharpCodeWriter>();

// 4. YAML Generation
builder.Services.AddScoped<IYamlSynthesisService, YamlSynthesisService>();
builder.Services.AddScoped<IYamlCodeWriter, YamlCodeWriter>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// -- Middleware --
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();