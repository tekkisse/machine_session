using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQStuff
{
    public sealed class HeartbeatDto
    {
        public string MachineName { get; init; } = "";
        public DateTime TimestampUtc { get; init; }
        public IReadOnlyList<string> LoggedInUsers { get; init; } = [];
    }
}
