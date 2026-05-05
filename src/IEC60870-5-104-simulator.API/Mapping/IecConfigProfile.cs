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
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => $"CA{src.Ca}_IOA{src.Oa}"))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Iec104DataType, opt => opt.MapFrom(src => src.TypeId ));

            CreateMap<DataPointConfig, Iec104DataPoint>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => new IecAddress(src.Ca, src.Oa)))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => $"CA{src.Ca}_IOA{src.Oa}"))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Iec104DataType, opt => opt.MapFrom(src => src.TypeId))
                .ForMember(dest => dest.InitString, opt => opt.MapFrom(src => src.InitValue))
                .ForMember(dest => dest.Mode, opt => opt.MapFrom(src => MapSimulationMode(src.Mode)));

            CreateMap<SimulationMode, SimulationModeConfig>();

            CreateMap<Iec104DataPoint, Iec104DataPointDto>()
                .ForMember(dest => dest.ObjectAddress, opt => opt.MapFrom(src => src.Address.ObjectAddress))
                .ForMember(dest => dest.StationaryAddress, opt => opt.MapFrom(src => src.Address.StationaryAddress))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value));

            CreateMap<Iec104DataPointDto, Iec104DataPoint>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => new IecAddress(src.StationaryAddress, src.ObjectAddress)))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => $"CA{src.StationaryAddress}_IOA{src.ObjectAddress}"))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            CreateMap<IecValueDto, IecValueObject>().ConvertUsing<DtoValueMapper>();
            CreateMap<IecValueObject, IecValueDto>().ConvertUsing<ValueDtoMapper>();
        }

        private static SimulationMode MapSimulationMode(SimulationModeConfig config) => config switch
        {
            SimulationModeConfig.GaussianNoise    => SimulationMode.GaussianNoise,
            SimulationModeConfig.PeriodicWave      => SimulationMode.PeriodicWave,
            SimulationModeConfig.EnergyCounter    => SimulationMode.EnergyCounter,
            SimulationModeConfig.CounterOnDemand  => SimulationMode.CounterOnDemand,
            SimulationModeConfig.Profile          => SimulationMode.Profile,
            SimulationModeConfig.Periodic         => SimulationMode.Periodic,
            SimulationModeConfig.RandomWalk       => SimulationMode.RandomWalk,
            SimulationModeConfig.CommandResponse  => SimulationMode.CommandResponse,
            _                                     => SimulationMode.Static
        };
    }
}
