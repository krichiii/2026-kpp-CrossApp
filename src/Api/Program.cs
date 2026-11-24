using Api.Endpoints;
using Api.Handlers;
using Core.Abstractions;
using FluentValidation;
using Scalar.AspNetCore;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var raw = builder.Configuration.GetConnectionString("AppDb") ?? "Data Source=data/app.db";
var csb = new SqliteConnectionStringBuilder(raw);

if (!Path.IsPathRooted(csb.DataSource))
{ // відносний шлях - завжди від каталогу застосунку, не від cwd
    var full = Path.Combine(builder.Environment.ContentRootPath, csb.DataSource);
    Directory.CreateDirectory(Path.GetDirectoryName(full)!);
    csb.DataSource = full; 
}

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails(); // формат помилок RFC 9457
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite(csb.ConnectionString));
builder.Services.AddScoped<ICatalogStore, EfCatalogStore>();

var app = builder.Build();
app.UseExceptionHandler(); 
app.UseStatusCodePages();

using (var scope = app.Services.CreateScope()) // після builder.Build()   

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.MapGet("/throw", () => { throw new InvalidOperationException("Тестовий виняток для перевірки ProblemDetails."); });
    app.MapGet("/throw-domain", () => { throw new DomainException("Конфлікт бізнес-правила: товар заблоковано для змін."); });
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapProducts();

app.Run();

public partial class Program { }