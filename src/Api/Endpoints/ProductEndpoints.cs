using System.Security.Cryptography;
using Api.Contracts;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;
using Microsoft.AspNetCore.Http.HttpResults;
namespace Api.Endpoints;

public static class ProductEndpoints
{
    public static void MapProducts(this WebApplication app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        group.MapGet("/", (ICatalogStore store) =>
            TypedResults.Ok(store.List().Select(p => p.ToDto()).ToList()));

        group.MapGet("/{id}", Results<Ok<ProductDto>, NotFound> 
            (string id, ICatalogStore store) =>
                store.GetById(id) is { } found
                    ? TypedResults.Ok(found.ToDto())
                    : TypedResults.NotFound());

        group.MapPost("/", Results<Created<ProductDto>, ValidationProblem>
            (SaveProductRequest request, ICatalogStore store) =>
            {
                if (!request.TryValidate(out var errors))
                    return TypedResults.ValidationProblem(errors);

                var id = Convert.ToHexString(RandomNumberGenerator.GetBytes(4)).ToLowerInvariant();
                var created = Product.Create(id, request.Sku, request.Name, request.Unit, request.Quantity);
                store.Add(created);
                return TypedResults.Created($"/api/products/{created.Id}", created.ToDto());
            });

        group.MapPut("/{id}", Results<Ok<ProductDto>, NotFound, ValidationProblem>
            (string id, SaveProductRequest request, ICatalogStore store) =>
            {
                if (!request.TryValidate(out var errors))
                    return TypedResults.ValidationProblem(errors);

                if (store.GetById(id) is null)
                    return TypedResults.NotFound();

                // PUT замінює стан: get-only поля не мутуємо, створюємо валідний Product з тим самим id.
                var replacement = Product.Create(id, request.Sku, request.Name, request.Unit, request.Quantity);
                store.Update(replacement);
                return TypedResults.Ok(replacement.ToDto());
            });

        group.MapDelete("/{id}", Results<NoContent, NotFound>
            (string id, ICatalogStore store) =>
                store.Remove(id)
                    ? TypedResults.NoContent()
                    : TypedResults.NotFound()); 
    } 
}