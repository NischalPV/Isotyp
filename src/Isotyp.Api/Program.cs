using Microsoft.EntityFrameworkCore;
using Isotyp.Application.Interfaces;
using Isotyp.Application.Services;
using Isotyp.Core.Interfaces;
using Isotyp.Infrastructure;
using Isotyp.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Isotyp - Enterprise Data Architecture Platform",
        Version = "v1",
        Description = @"Enterprise data architecture platform where:
- All data processing runs locally via agents; cloud sees metadata only
- System forms a versioned, canonical understanding of data and re-validates it over time
- AI may suggest schema/model evolution but never auto-applies changes
- All changes require explicit multi-layer human approval
- Respects schema locks, updates DB + ORM together
- Fully auditable and rollback-safe"
    });
});

// Configure database (SQLite for development)
builder.Services.AddDbContext<IsotypDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=isotyp.db"));

// Register Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register Application Services
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IDataSourceService, DataSourceService>();
builder.Services.AddScoped<ISchemaVersionService, SchemaVersionService>();
builder.Services.AddScoped<ISchemaChangeRequestService, SchemaChangeRequestService>();
builder.Services.AddScoped<IAiSuggestionService, AiSuggestionService>();
builder.Services.AddScoped<IAgentService, AgentService>();
builder.Services.AddScoped<IDataValidationService, DataValidationService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.MapControllers();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<IsotypDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();
