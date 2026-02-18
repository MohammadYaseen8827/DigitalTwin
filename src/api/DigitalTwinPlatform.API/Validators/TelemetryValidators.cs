using DigitalTwinPlatform.Application.Telemetry.Models;
using FluentValidation;

namespace DigitalTwinPlatform.API.Validators;

public class TelemetryIngestValidator : AbstractValidator<TelemetryIngestDto>
{
    public TelemetryIngestValidator()
    {
        RuleFor(x => x.MachineId).NotEmpty();
        RuleFor(x => x.DataType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Data).NotNull();
    }
}
