using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using Mqttservice.Helpers.Interfaces;
using Mqttservice.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mqttservice
{
    public class StartupService : BackgroundService
    {
        private const string ClientId = "DataProcessingApp";
        private const string MqttUri = "broker";
        private readonly Dictionary<int, PastAndCurrentData> carCoordinates;
        private readonly IMqttClient client;
        private readonly ILogger<StartupService> logger;
        private readonly IMqttClientOptions options;
        private readonly ITransformTelemetryData transform;

        public StartupService(ILogger<StartupService> logger, ITransformTelemetryData transform)
        {
            this.logger = logger;
            this.transform = transform;
            client = new MqttFactory().CreateMqttClient();
            carCoordinates = new Dictionary<int, PastAndCurrentData>();
            options = new MqttClientOptionsBuilder()
                .WithClientId(ClientId)
                .WithTcpServer(MqttUri, 1883)
                .Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                client.ConnectedHandler = new MqttClientConnectedHandlerDelegate(OnConnected);
                client.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(OnDisconnected);
                client.ApplicationMessageReceivedHandler =
                    new MqttApplicationMessageReceivedHandlerDelegate(OnReceivedAMessage);

                await client.ConnectAsync(options, CancellationToken.None);
            }
            catch (Exception)
            {
                if (!client.IsConnected)
                {
                    Console.WriteLine("### MQTT Server is not running ###");
                    Console.WriteLine("Please start the docker-compose now");
                    Console.WriteLine();
                }
            }
        }

        private void OnReceivedAMessage(MqttApplicationMessageReceivedEventArgs e)
        {
            Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
            Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
            Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
            Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
            Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
            Console.WriteLine();

            var car = JsonConvert.DeserializeObject<CarCoordinates>(
                Encoding.UTF8.GetString(e.ApplicationMessage.Payload));

            transform.ProcessNewMessage(
                client,
                car,
                carCoordinates);
        }

        private async void OnConnected(MqttClientConnectedEventArgs e)
        {
            Console.WriteLine("### CONNECTED WITH SERVER ###");

            await client.SubscribeAsync(new TopicFilterBuilder().WithTopic("carCoordinates").Build());

            Console.WriteLine("### SUBSCRIBED ###");
        }

        private async void OnDisconnected(MqttClientDisconnectedEventArgs e)
        {
            Console.WriteLine("### DISCONNECTED FROM SERVER ###");
            await Task.Delay(TimeSpan.FromSeconds(5));
            try
            {
                await client.ConnectAsync(options, CancellationToken.None);
            }
            catch
            {
                Console.WriteLine("### RECONNECTING FAILED ###");
            }
        }
    }
}
