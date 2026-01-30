using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Data.Common;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

public sealed class RabbitMqListener
{
 

    public async Task StartAsync()
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest",
        };

        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync("server.heartbeat", ExchangeType.Fanout, durable: true);
        
        await channel.ExchangeDeclareAsync("machine.commands", ExchangeType.Topic, durable: true);

        var queue = await channel.QueueDeclareAsync(
            queue: "",
            durable: false,
            exclusive: true,
            autoDelete: true);

        await channel.QueueBindAsync(
            queue: queue.QueueName,
            exchange: "machine.commands",
            routingKey: Environment.MachineName);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += HandleMessageAsync;

        await channel.BasicConsumeAsync(queue.QueueName, true, consumer);
    }

    private async Task HandleMessageAsync(object sender, BasicDeliverEventArgs e)
    {
        var json = Encoding.UTF8.GetString(e.Body.ToArray());
        var command = JsonSerializer.Deserialize<CommandMessage>(json);

        if (command == null)
            return;

        if (command.Command == "reboot")
        {
            Execute("shutdown", "/r /f /t 0");
        }
        else if (command.Command == "logout")
        {
            Execute("shutdown", "/l /f");
        }

        await Task.CompletedTask;
    }

    private static void Execute(string file, string args)
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = file,
            Arguments = args,
            CreateNoWindow = true,
            UseShellExecute = false
        });
    }

    private record CommandMessage(string Command, string? User);
}
