namespace Core.Dto;

public record WarehouseDto(
    string Id,
    string Name,
    string? Sku = null // щоб не міняти поля в sample, використовується як location
);

