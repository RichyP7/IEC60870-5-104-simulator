using IEC60870_5_104_simulator.Domain.Interfaces;
using IEC60870_5_104_simulator.Domain.Service;
using IEC60870_5_104_simulator.Infrastructure;
using IEC60870_5_104_simulator.Infrastructure.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using IEC60870_5_104_simulator.Infrastructure.DataPointsService;

namespace ServiceExtensionMethods
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddSingleton<IIec104Service, Iec104Service>();
            services.AddScoped<DataPointService, DataPointService>();
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
            services.AddSingleton<IInformationObjectFactory, RandomObjectFactory>();
            services.AddSingleton<ICommandResponseFactory, MirroredResponseFactory>();
            services.AddSingleton<IInformationObjectTemplate, InformationObjectTemplateMethod>();
            services.AddSingleton<IIecValueRepository, IecValueLocalStorageRepository>();
            return services;
        }

    }
}