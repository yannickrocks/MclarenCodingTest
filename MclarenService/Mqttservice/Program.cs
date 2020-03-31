using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mqttservice.Helpers;
using Mqttservice.Helpers.Interfaces;

namespace Mqttservice
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices(
                    (hostContext, services) =>
                    {
                        services.AddSingleton<IPublishMetrics, PublishMetrics>();
                        services.AddSingleton<ICalculateSpeedAndDistance, CalculateSpeedAndDistance>();
                        services.AddSingleton<IPublishMessageToMqtt, PublishMessageToMqtt>();
                        services.AddTransient<ITransformTelemetryData, TransformTelemetryData>();
                        services.AddHostedService<StartupService>();
                    });
        }
    }
}
