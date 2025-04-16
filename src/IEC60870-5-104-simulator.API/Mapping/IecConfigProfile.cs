using AutoMapper;
using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.ValueTypes;
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
            CreateMap<DataPointConfig, Iec104DataPoint>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => new IecAddress(src.Ca, src.Oa)))
                .ForMember(dest => dest.Iec104DataType, opt => opt.MapFrom(src => src.TypeId))
                .ForMember(dest => dest.InitString, opt => opt.MapFrom(src => src.InitValue));
            CreateMap<SimulationMode, SimulationModeConfig>();
            CreateMap<Iec104DataPoint, Iec104DataPointDto>()
                .ForMember(dest => dest.ObjectAddress, opt => opt.MapFrom(src => src.Address.ObjectAddress))
                .ForMember(dest => dest.StationaryAddress, opt => opt.MapFrom(src => src.Address.StationaryAddress))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value));
            CreateMap<Iec104DataPointDto, Iec104DataPoint>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => new IecAddress(src.StationaryAddress, src.ObjectAddress)));
            CreateMap<IecValueDto, IecValueObject>().ConvertUsing<DtoValueMapper>();
            CreateMap<IecValueObject, IecValueDto>().ConvertUsing<ValueDtoMapper>();

        }
    }
}
