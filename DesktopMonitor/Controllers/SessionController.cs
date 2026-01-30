using Microsoft.AspNetCore.Mvc;
using RabbitMQStuff;
using StackExchange.Redis;

namespace DesktopMonitor.Controllers
{
    [ApiController]
    [Route("api")]
    public sealed class SessionController : ControllerBase
    {
        private readonly SessionStateService _service;
        private readonly IDatabase _db;

        public SessionController(SessionStateService service, IConnectionMultiplexer redis)
        {
            _service = service;
            _db = redis.GetDatabase();
        }

        //[HttpPost("heartbeat")]
        //public async Task<IActionResult> Heartbeat([FromBody] HeartbeatDto dto)
        //{
        //    await _service.ProcessHeartbeatAsync(dto);
        //    return Ok();
        //}

 

        [HttpGet("sessions/{username}")]
        public async Task<IActionResult> GetUserSessions(string username)
        {
            var userKey = $"user:{username}";
            var machines = await _db.SetMembersAsync(userKey);

            var activeMachines = new List<string>();

            foreach (var m in machines.Select(x => x.ToString()))
            {
                var machineKey = $"machine:{m}";

                if (!await _db.KeyExistsAsync(machineKey))
                {
                    // stale reference → cleanup
                    await _db.SetRemoveAsync(userKey, m);
                    continue;
                }

                // optional: verify user still listed on machine
                if (await _db.SetContainsAsync(machineKey, username))
                    activeMachines.Add(m);
                else
                    await _db.SetRemoveAsync(userKey, m);
            }

            // optional: delete empty user key
            if (await _db.SetLengthAsync(userKey) == 0)
                await _db.KeyDeleteAsync(userKey);

            return Ok(activeMachines);
        }

    }

}
