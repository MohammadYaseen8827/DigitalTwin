using AutoMapper;
using ProductionLineDto = DigitalTwinPlatform.Application.ProductionLines.Models.ProductionLineDto;

namespace DigitalTwinPlatform.API.Profiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ProductionLine, ProductionLineDto>()
            .ForMember(dest => dest.MachineIds, opt => opt.MapFrom(src => src.Machines.Select(m => m.Id)));
    }
}

