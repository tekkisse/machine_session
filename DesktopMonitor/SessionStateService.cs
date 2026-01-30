using RabbitMQStuff;
using StackExchange.Redis;

namespace DesktopMonitor
{
    public sealed class SessionStateService
    {
        private readonly IDatabase _db;

        public SessionStateService(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }


        public async Task ProcessHeartbeatAsync(HeartbeatDto hb)
        {
            var machineKey = $"machine:{hb.MachineName}";

            var previousUsers = (await _db.SetMembersAsync(machineKey))
                .Select(v => v.ToString())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var currentUsers = hb.LoggedInUsers
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var added = currentUsers.Except(previousUsers);
            var removed = previousUsers.Except(currentUsers);

            foreach (var user in added)
            {
                await _db.SetAddAsync(machineKey, user);
                await _db.SetAddAsync($"user:{user}", hb.MachineName);
            }

            foreach (var user in removed)
            {
                await _db.SetRemoveAsync(machineKey, user);
                await _db.SetRemoveAsync($"user:{user}", hb.MachineName);
            }

            // 🔑 TTL refreshed on every heartbeat
            await _db.KeyExpireAsync(machineKey, TimeSpan.FromMinutes(2));

            // Optional: clean empty user keys
            foreach (var user in removed)
            {
                var userKey = $"user:{user}";
                if (await _db.SetLengthAsync(userKey) == 0)
                {
                    await _db.KeyDeleteAsync(userKey);
                }
            }
        }

    }

}
