using EXE201_Backend.Models.Requests;
using EXE201_Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EXE201_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TimeController : ControllerBase
    {
        private readonly TimeMachine? _timeMachine;
        private readonly ILogger<TimeController> _logger;

        public TimeController(ITimeProvider timeProvider, ILogger<TimeController> logger)
        {
            if (timeProvider is TimeMachine)
            {
                _timeMachine = (TimeMachine)timeProvider;
            }

            _logger = logger;
        }

        [Authorize(Roles = "staff")]
        [HttpGet("now")]
        public IActionResult Now()
        {
            if (_timeMachine == null)
            {
                _logger.LogWarning("Now requested but TimeMachine is not available.");
                return BadRequest("Time manipulation is not enabled in this environment.");
            }

            var now = _timeMachine.Now;
            var offset = now - DateTime.Now;
            return Ok(new { now = now.ToString("o"), offset = offset.ToString() });
        }

        [Authorize(Roles = "staff")]
        [HttpPost("forward")]
        public IActionResult Forward([FromQuery] string? duration, [FromQuery] double? seconds, [FromBody] TimeShiftRequest? body = null)
        {
            if (!EnsureTimeMachine(out var err)) return err!;

            if (!TryGetTimeSpan(duration, seconds, body, out var ts, out var parseError))
            {
                return BadRequest(parseError);
            }

            _timeMachine!.Forward(ts);
            _logger.LogInformation("Time forwarded by {timespan}. New time: {now}", ts, _timeMachine.Now);
            return Ok(new { message = $"Time forwarded by {ts}.", now = _timeMachine.Now.ToString("o") });
        }

        [Authorize(Roles = "staff")]
        [HttpPost("backward")]
        public IActionResult Backward([FromQuery] string? duration, [FromQuery] double? seconds, [FromBody] TimeShiftRequest? body = null)
        {
            if (!EnsureTimeMachine(out var err)) return err!;

            if (!TryGetTimeSpan(duration, seconds, body, out var ts, out var parseError))
            {
                return BadRequest(parseError);
            }

            _timeMachine!.Backward(ts);
            _logger.LogInformation("Time moved backward by {timespan}. New time: {now}", ts, _timeMachine.Now);
            return Ok(new { message = $"Time moved backward by {ts}.", now = _timeMachine.Now.ToString("o") });
        }

        [Authorize(Roles = "staff")]
        [HttpPost("setoffset")]
        public IActionResult SetOffset([FromQuery] string? duration, [FromQuery] double? seconds, [FromBody] TimeShiftRequest? body = null)
        {
            if (!EnsureTimeMachine(out var err)) return err!;

            if (!TryGetTimeSpan(duration, seconds, body, out var ts, out var parseError))
            {
                return BadRequest(parseError);
            }

            _timeMachine!.SetOffset(ts);
            _logger.LogInformation("Time offset set to {offset}. New time: {now}", ts, _timeMachine.Now);
            return Ok(new { message = $"Time offset set to {ts}.", now = _timeMachine.Now.ToString("o") });
        }

        [Authorize(Roles = "staff")]
        [HttpPost("reset")]
        public IActionResult Reset()
        {
            if (!EnsureTimeMachine(out var err)) return err!;

            _timeMachine!.Reset();
            _logger.LogInformation("Time reset to system time. New time: {now}", _timeMachine.Now);
            return Ok(new { message = "Time reset to system time.", now = _timeMachine.Now.ToString("o") });
        }

        private bool EnsureTimeMachine(out IActionResult? errorResult)
        {
            if (_timeMachine == null)
            {
                _logger.LogWarning("Time manipulation request received but TimeMachine is not registered.");
                errorResult = BadRequest("Time manipulation is not enabled in this environment.");
                return false;
            }

            errorResult = null;
            return true;
        }

        private bool TryGetTimeSpan(string? durationQuery, double? secondsQuery, TimeShiftRequest? body, out TimeSpan timespan, out string? error)
        {
            timespan = TimeSpan.Zero;
            error = null;

            if (secondsQuery.HasValue)
            {
                try
                {
                    timespan = TimeSpan.FromSeconds(secondsQuery.Value);
                    return true;
                }
                catch (Exception ex)
                {
                    error = $"Invalid seconds value: {ex.Message}";
                    return false;
                }
            }

            var duration = durationQuery ?? body?.Duration;
            if (!string.IsNullOrWhiteSpace(duration))
            {
                if (TimeSpan.TryParse(duration, out timespan))
                {
                    return true;
                }
                else
                {
                    error = "Invalid duration format. Use TimeSpan formats like \"hh:mm:ss\" or provide numeric seconds.";
                    return false;
                }
            }

            if (body?.Seconds.HasValue == true)
            {
                try
                {
                    timespan = TimeSpan.FromSeconds(body.Seconds.Value);
                    return true;
                }
                catch (Exception ex)
                {
                    error = $"Invalid seconds value in body: {ex.Message}";
                    return false;
                }
            }

            error = "No duration provided. Supply either 'duration' (TimeSpan string) or 'seconds' (numeric) as query or JSON body.";
            return false;
        }
    }
}
