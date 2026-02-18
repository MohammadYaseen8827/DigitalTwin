using DigitalTwinPlatform.Application.Simulations.Models;
using FluentValidation;

namespace DigitalTwinPlatform.API.Validators;

public class SimulationRequestValidator : AbstractValidator<CreateSimulationDto>
{
    public SimulationRequestValidator()
    {
        RuleFor(x => x.MachineId).NotEmpty();
        RuleFor(x => x.Parameters).NotNull();
    }
}
