using Api.Contracts;
using FluentValidation;

namespace Api.Validation;

public sealed class SaveProductRequestValidator : AbstractValidator<SaveProductRequest>
{
    public SaveProductRequestValidator()
    {
        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("Артикул обов'язковий.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Назва обов'язкова, до 200 символів.")
            .MaximumLength(200).WithMessage("Назва обов'язкова, до 200 символів.");

        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage("Одиниця виміру обов'язкова, до 20 символів.")
            .MaximumLength(20).WithMessage("Одиниця виміру обов'язкова, до 20 символів.");

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("Кількість не може бути від'ємною.");
    }
}
