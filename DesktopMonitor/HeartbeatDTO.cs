namespace DesktopMonitor
{
    public sealed class HeartbeatDto
    {
        public string MachineName { get; init; } = "";
        public DateTime TimestampUtc { get; init; }
        public IReadOnlyList<string> LoggedInUsers { get; init; } = [];
    }

}
