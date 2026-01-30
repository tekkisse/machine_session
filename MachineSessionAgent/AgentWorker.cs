using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;

public sealed class AgentWorker : BackgroundService
{
    private readonly ILogger<AgentWorker> _logger;
    private readonly SessionDetector _sessionDetector;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly RabbitMqListener _rabbit;

    private readonly string _machineName = Environment.MachineName;

    public AgentWorker(
        ILogger<AgentWorker> logger,
        SessionDetector sessionDetector,
        IHttpClientFactory httpClientFactory,
        RabbitMqListener rabbit)
    {
        _logger = logger;
        _sessionDetector = sessionDetector;
        _httpClientFactory = httpClientFactory;
        _rabbit = rabbit;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _rabbit.StartAsync();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SendHeartbeatAsync(stoppingToken);
                _logger.LogInformation("Heartbeat sent");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Heartbeat failed");
            }
            
            await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
            //await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private async Task SendHeartbeatAsync(CancellationToken ct)
    {
        var users = _sessionDetector.GetLoggedInUsers();

        var payload = new
        {
            machineName = _machineName,
            timestampUtc = DateTime.UtcNow,
            loggedInUsers = users
        };

        var json = JsonSerializer.Serialize(payload);

        // Send by REST API
        var client = _httpClientFactory.CreateClient();
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        await client.PostAsync("https://localhost:32769/api/heartbeat", content, ct);
        
        // Send by RabbitMQ
        
    }
}
