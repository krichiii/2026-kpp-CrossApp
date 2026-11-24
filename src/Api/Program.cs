using Api.Endpoints;
using Core.Abstractions;
using FluentValidation;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails(); // формат помилок RFC 9457
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
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapProducts();

app.Run();