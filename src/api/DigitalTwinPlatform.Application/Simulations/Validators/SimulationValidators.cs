using DigitalTwinPlatform.Application.Simulations.Models;
using FluentValidation;

namespace DigitalTwinPlatform.Application.Simulations.Validators;

public class CreateSimulationValidator : AbstractValidator<CreateSimulationDto>
{
    public CreateSimulationValidator()
    {
        RuleFor(x => x.MachineId).NotEmpty();
        RuleFor(x => x.Parameters).NotNull();
    }
}
