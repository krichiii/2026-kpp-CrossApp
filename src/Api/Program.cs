using Api.Endpoints;
using Api.Handlers;
using Core.Abstractions;
using FluentValidation;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails(); // формат помилок RFC 9457
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddSingleton<ICatalogStore, InMemoryCatalogStore>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();
app.UseExceptionHandler(); 
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.MapGet("/throw", () => { throw new InvalidOperationException("Тестовий виняток для перевірки ProblemDetails."); });
    app.MapGet("/throw-domain", () => { throw new DomainException("Конфлікт бізнес-правила: товар заблоковано для змін."); });
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapProducts();

app.Run();