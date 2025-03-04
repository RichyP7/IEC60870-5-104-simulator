using IEC60870_5_104_simulator.Domain.Interfaces;
using IEC60870_5_104_simulator.Domain.Service;
using IEC60870_5_104_simulator.Infrastructure;
using IEC60870_5_104_simulator.Infrastructure.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using IEC60870_5_104_simulator.Infrastructure.DataPointsService;
using ObjectFactory = IEC60870_5_104_simulator.Infrastructure.ObjectFactory;

namespace ServiceExtensionMethods
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddSingleton<IIec104Service, Iec104Service>();
            services.AddScoped<DataPointService, DataPointService>();
            services.AddScoped<ValueService, ValueService>();
            services.AddSingleton<IIec104ConfigurationService, Iec104ConfigurationService>();
            services.AddSingleton<lib60870.CS104.Server>(v =>
            {
                var server = new lib60870.CS104.Server();
                server.DebugOutput = false;
                server.EnqueueMode = lib60870.CS104.EnqueueMode.REMOVE_OLDEST;
                server.MaxQueueSize = 100;
                server.SetLocalPort(2404);
                return server;
            });
            services.AddSingleton<IInformationObjectFactory, ObjectFactory>();
            services.AddSingleton<ICommandResponseFactory, MirroredResponseFactory>();
            services.AddSingleton<IInformationObjectTemplate, InformationObjectTemplateMethod>();
            services.AddSingleton<IIecValueRepository, IecValueLocalStorageRepository>();
            return services;
        }

    }
}