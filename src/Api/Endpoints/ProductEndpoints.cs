using System.Security.Cryptography;
using Api.Contracts;
using Api.Filters;
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

        group.MapGet("/", (ICatalogStore store, int page = 1, int pageSize = 20, string? search = null) =>
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var filtered = string.IsNullOrWhiteSpace(search)
                ? store.List()
                : store.Find(p =>
                    p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    p.Sku.Contains(search, StringComparison.OrdinalIgnoreCase));

            var total = filtered.Count;
            var items = filtered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => p.ToDto())
                .ToList();

            return TypedResults.Ok(new PagedResult<ProductDto>(items, total));
        });

        group.MapGet("/{id}", Results<Ok<ProductDto>, NotFound> 
            (string id, ICatalogStore store) =>
                store.GetById(id) is { } found
                    ? TypedResults.Ok(found.ToDto())
                    : TypedResults.NotFound());

        group.MapPost("/", (SaveProductRequest request, ICatalogStore store) =>
            {
                if (store.Find(p => p.Sku.Equals(request.Sku, StringComparison.OrdinalIgnoreCase)).Count > 0)
                {
                    throw new DomainException($"Товар з артикулом '{request.Sku}' вже існує.");
                }

                var id = Convert.ToHexString(RandomNumberGenerator.GetBytes(4)).ToLowerInvariant();
                var created = Product.Create(id, request.Sku, request.Name, request.Unit, request.Quantity);
                store.Add(created);
                return TypedResults.Created($"/api/products/{created.Id}", created.ToDto());
            })
            .AddEndpointFilter<ValidationFilter<SaveProductRequest>>();

        group.MapPut("/{id}", Results<Ok<ProductDto>, NotFound>
            (string id, SaveProductRequest request, ICatalogStore store) =>
            {
                if (store.GetById(id) is null)
                    return TypedResults.NotFound();

                var duplicate = store.Find(p => p.Sku.Equals(request.Sku, StringComparison.OrdinalIgnoreCase) && !p.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
                if (duplicate.Count > 0)
                {
                    throw new DomainException($"Товар з артикулом '{request.Sku}' вже існує.");
                }

                // PUT замінює стан: get-only поля не мутуємо, створюємо валідний Product з тим самим id.
                var replacement = Product.Create(id, request.Sku, request.Name, request.Unit, request.Quantity);
                store.Update(replacement);
                return TypedResults.Ok(replacement.ToDto());
            })
            .AddEndpointFilter<ValidationFilter<SaveProductRequest>>();

        group.MapDelete("/{id}", Results<NoContent, NotFound>
            (string id, ICatalogStore store) =>
                store.Remove(id)
                    ? TypedResults.NoContent()
                    : TypedResults.NotFound()); 
    } 
}