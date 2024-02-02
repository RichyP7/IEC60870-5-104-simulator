using AutoMapper;
using IEC60870_5_104_simulator.Domain;
using static IEC60870_5_104_simulator.API.Iec104SimulationOptions;

namespace IEC60870_5_104_simulator.API.Mapping
{
    public class IecConfigProfile : Profile
    {
        public IecConfigProfile()
        {
            CreateMap<CommandPointConfig, Iec104CommandDataPointConfig>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => new IecAddress(src.Ca,src.Oa)))
                .ForMember(dest => dest.Iec104DataType, opt => opt.MapFrom(src => src.TypeId ));
            CreateMap<DataPointConfig, Iec104DataPointConfig>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => new IecAddress(src.Ca, src.Oa)))
                .ForMember(dest => dest.Iec104DataType, opt => opt.MapFrom(src => src.TypeId));

        }
    }
}
