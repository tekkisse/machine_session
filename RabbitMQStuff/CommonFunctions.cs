using RabbitMQ.Client;
using System.Text;

namespace RabbitMQStuff
{

    public class CommonFunctions
    {

        public const string server_heartbeat = "server.heartbeat";
        public const string machine_commands = "machine.commands";
        public const string heartbeats = "heartbeats";
            
        public string rabbit_hostname = "localhost";
        public string rabbit_username = "guest";
        public string rabbit_password = "guest";
 
        public async Task<(IChannel channel, string queue)> ConfigureRabbit()
        {
            var factory = new ConnectionFactory
            {
                HostName = rabbit_hostname,
                UserName = rabbit_username,
                Password = rabbit_password,
            };

            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(server_heartbeat, ExchangeType.Fanout, durable: true);

            var args = new Dictionary<string, object>
            {
                { "x-message-ttl", 300_000 }     // message TTL: 5 min
            };
            
            var queue2 = await channel.QueueDeclareAsync(
                queue: heartbeats,
                exclusive:false,
                autoDelete:false,
                durable: true,
                arguments:args);

            await channel.QueueBindAsync(
                queue: queue2.QueueName,
                exchange: server_heartbeat,
                routingKey: "#");

            await channel.ExchangeDeclareAsync(machine_commands, ExchangeType.Topic, durable: true);

            var queue = await channel.QueueDeclareAsync(
                queue: "",
                durable: false,
                exclusive: true,
                autoDelete: true);

            await channel.QueueBindAsync(
                queue: queue.QueueName,
                exchange: machine_commands,
                routingKey: Environment.MachineName);

            return (channel, queue.QueueName);
        }

        public async Task SendMessage(string exchangeName, string routingKey, string message)
        {
            var factory = new ConnectionFactory
            {
                HostName = rabbit_hostname,
                UserName = rabbit_username,
                Password = rabbit_password,
            };

            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();

            var body = Encoding.UTF8.GetBytes(message);

            await channel.BasicPublishAsync(exchangeName, routingKey, body);

        }
    }
}
