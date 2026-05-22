using System.Globalization;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EXE201_Backend.Services;
using EXE201_Backend.Models.Requests;

namespace EXE201_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "staff")]
    public class ConfigurationController : ControllerBase
    {
        private readonly IConfigurationService _configService;
        private readonly ILogger<ConfigurationController> _logger;

        public ConfigurationController(IConfigurationService configService, ILogger<ConfigurationController> logger)
        {
            _configService = configService;
            _logger = logger;
        }


        [HttpGet]
        public ActionResult<IEnumerable<object>> GetAll()
        {
            var props = typeof(IConfigurationService).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var result = props.Select(p =>
            {
                var val = p.GetValue(_configService);
                var valueStr = val is null ? null : ConvertToString(val, p.PropertyType);
                return new
                {
                    Name = p.Name,
                    Type = p.PropertyType.Name,
                    Value = valueStr
                };
            });

            return Ok(result);
        }

        [HttpGet("{name}")]
        public ActionResult<object> Get(string name)
        {
            var prop = GetPropertyByName(name);
            if (prop == null)
            {
                return NotFound(new { Message = $"Configuration key '{name}' not found." });
            }

            var val = prop.GetValue(_configService);
            var valueStr = val is null ? null : ConvertToString(val, prop.PropertyType);

            return Ok(new
            {
                Name = prop.Name,
                Type = prop.PropertyType.Name,
                Value = valueStr
            });
        }

        [HttpPut("{name}")]
        public ActionResult Set(string name, [FromBody] ConfigurationRequest request)
        {
            if (request is null)
            {
                return BadRequest(new { Message = "Request body is required." });
            }

            var prop = GetPropertyByName(name);
            if (prop == null || !prop.CanWrite)
            {
                return NotFound(new { Message = $"Configuration key '{name}' not found or is read-only." });
            }

            var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

            try
            {
                object? convertedValue = ConvertFromString(request.Value, targetType);
                var envString = ConvertToString(convertedValue!, targetType);
                Environment.SetEnvironmentVariable(prop.Name, envString);

                // Update current instance
                prop.SetValue(_configService, convertedValue);

                return NoContent();
            }
            catch (FormatException fe)
            {
                return BadRequest(new { Message = $"Value for '{name}' cannot be converted to type {targetType.Name}: {fe.Message}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set configuration value for {Name}", name);
                return StatusCode(500, new { Message = "Failed to update configuration.", Detail = ex.Message });
            }
        }

        private static PropertyInfo? GetPropertyByName(string name)
        {
            return typeof(IConfigurationService)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        private static string? ConvertToString(object? value, Type type)
        {
            if (value is null) return null;

            var targetType = Nullable.GetUnderlyingType(type) ?? type;

            if (targetType == typeof(string))
                return (string)value;

            if (targetType == typeof(int))
                return Convert.ToInt32(value).ToString(CultureInfo.InvariantCulture);

            if (targetType == typeof(long))
                return Convert.ToInt64(value).ToString(CultureInfo.InvariantCulture);

            if (targetType == typeof(double))
                return Convert.ToDouble(value).ToString(CultureInfo.InvariantCulture);

            if (targetType == typeof(float))
                return Convert.ToSingle(value).ToString(CultureInfo.InvariantCulture);

            if (targetType == typeof(decimal))
                return Convert.ToDecimal(value).ToString(CultureInfo.InvariantCulture);

            if (targetType == typeof(bool))
                return Convert.ToBoolean(value).ToString();

            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        private static object? ConvertFromString(string? str, Type targetType)
        {
            if (targetType == typeof(string))
                return str;

            if (str is null)
                throw new FormatException("Null value is not valid for non-string target type.");

            if (targetType == typeof(int))
            {
                if (int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i)) return i;
                throw new FormatException("Invalid integer.");
            }

            if (targetType == typeof(long))
            {
                if (long.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out var l)) return l;
                throw new FormatException("Invalid long.");
            }

            if (targetType == typeof(double))
            {
                if (double.TryParse(str, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var d)) return d;
                throw new FormatException("Invalid double.");
            }

            if (targetType == typeof(float))
            {
                if (float.TryParse(str, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var f)) return f;
                throw new FormatException("Invalid float.");
            }

            if (targetType == typeof(decimal))
            {
                if (decimal.TryParse(str, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var m)) return m;
                throw new FormatException("Invalid decimal.");
            }

            if (targetType == typeof(bool))
            {
                if (bool.TryParse(str, out var b)) return b;
                if (str == "0") return false;
                if (str == "1") return true;
                throw new FormatException("Invalid boolean.");
            }

            try
            {
                return Convert.ChangeType(str, targetType, CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                throw new FormatException($"Cannot convert to {targetType.Name}: {e.Message}", e);
            }
        }
    }
}
