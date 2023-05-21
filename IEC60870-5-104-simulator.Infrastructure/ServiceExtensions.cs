using IEC60870_5_104_simulator.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace ServiceExtensionMethods
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddSingleton<IIec104Service, Iec104Service>();
            services.AddSingleton<lib60870.CS104.Server>(v =>
            {
                var server = new lib60870.CS104.Server();
                server.DebugOutput = false;
                server.MaxQueueSize = 100;
                server.SetLocalPort(2404);
                return server;
            });
            services.AddSingleton<IInformationObjectFactory, InformationObjectFactory>();
            return services;
        }

    }
}