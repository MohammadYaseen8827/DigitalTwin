using DigitalTwinPlatform.Application.ProductionLines.Models;
using FluentValidation;

namespace DigitalTwinPlatform.Application.ProductionLines.Validators;

public class ProductionLineCreateValidator : AbstractValidator<ProductionLineCreateDto>
{
    public ProductionLineCreateValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Configuration).NotNull();
    }
}

public class ProductionLineUpdateValidator : AbstractValidator<ProductionLineUpdateDto>
{
    public ProductionLineUpdateValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Configuration).NotNull();
    }
}
