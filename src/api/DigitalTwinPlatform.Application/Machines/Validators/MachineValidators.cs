using DigitalTwinPlatform.Application.Machines.Models;
using FluentValidation;

namespace DigitalTwinPlatform.Application.Machines.Validators;

public class MachineCreateValidator : AbstractValidator<MachineCreateDto>
{
    public MachineCreateValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Type).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Properties).NotNull();
    }
}

public class MachineUpdateValidator : AbstractValidator<MachineUpdateDto>
{
    public MachineUpdateValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Type).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Properties).NotNull();
    }
}
