
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Configuration;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;
    using System.Text;
    using System.Text.Json;
    using System.Xml;
    using RabbitMQStuff;

    namespace DesktopMonitor
    {

        public class RabbitMqConsumerService : BackgroundService
        {
            private readonly ILogger<RabbitMqConsumerService> _logger;
            private readonly IConfiguration _config;
            private readonly SessionStateService _service;

        public RabbitMqConsumerService(SessionStateService service,
                ILogger<RabbitMqConsumerService> logger,
                IConfiguration config)
            {
                _logger = logger;
                _config = config;
                _service = service;
            }

            protected override async Task ExecuteAsync(CancellationToken stoppingToken)
            {

                var rabbitStuff = new RabbitMQStuff.CommonFunctions();
                var (channel, queue) = await rabbitStuff.ConfigureRabbit();

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += HandleMessageAsync;

                await channel.BasicConsumeAsync(RabbitMQStuff.CommonFunctions.heartbeats, true, consumer);

            }

            private async Task HandleMessageAsync(object sender, BasicDeliverEventArgs e)
            {
                var json = Encoding.UTF8.GetString(e.Body.ToArray());
                var dto = JsonSerializer.Deserialize<HeartbeatDto>(json);
                if (dto != null)
                    await _service.ProcessHeartbeatAsync(dto);

                await Task.CompletedTask;
            }
            
        }
    }
    
