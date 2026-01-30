namespace DesktopMonitor
{
    public sealed class RedisOptions
    {
        public string Host { get; init; } = "";
        public int Port { get; init; }
        public string? Password { get; init; }
        public bool UseTls { get; init; }
        public int Database { get; init; }
    }

}
