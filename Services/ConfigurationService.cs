using System.Globalization;
using System.Reflection;

namespace EXE201_Backend.Services
{
    public class ConfigurationService : IConfigurationService
    {
        public string JWT_KEY { get; set; } = default!;
        public string JWT_ISSUER { get; set; } = default!;
        public string JWT_AUDIENCE { get; set; } = default!;
        public string ALLOWED_ORIGINS { get; set; } = default!;
        public string VNP_RETURN { get; set; } = default!;
        public string VNP_TMN { get; set; } = default!;
        public string VNP_SECRET { get; set; } = default!;
        public string SE_MERCHANT { get; set; } = default!;
        public string SE_SECRET { get; set; } = default!;
        public string DATABASE_CONNECTION { get; set; } = default!;

        public ConfigurationService()
        {
            InitFromEnv();
        }

        private void InitFromEnv()
        {
            var properties = typeof(ConfigurationService).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                if (!prop.CanWrite)
                {
                    continue;
                }

                var envValue = Environment.GetEnvironmentVariable(prop.Name);
                if (envValue == null)
                {
                    continue;
                }

                var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                try
                {
                    object? convertedValue;

                    if (targetType == typeof(string))
                    {
                        convertedValue = envValue;
                    }
                    else if (targetType == typeof(int))
                    {
                        convertedValue = int.Parse(envValue, CultureInfo.InvariantCulture);
                    }
                    else if (targetType == typeof(long))
                    {
                        convertedValue = long.Parse(envValue, CultureInfo.InvariantCulture);
                    }
                    else if (targetType == typeof(double))
                    {
                        convertedValue = double.Parse(envValue, CultureInfo.InvariantCulture);
                    }
                    else if (targetType == typeof(float))
                    {
                        convertedValue = float.Parse(envValue, CultureInfo.InvariantCulture);
                    }
                    else if (targetType == typeof(decimal))
                    {
                        convertedValue = decimal.Parse(envValue, CultureInfo.InvariantCulture);
                    }
                    else if (targetType == typeof(bool))
                    {
                        convertedValue = bool.Parse(envValue);
                    }
                    else
                    {
                        convertedValue = Convert.ChangeType(envValue, targetType, CultureInfo.InvariantCulture);
                    }

                    prop.SetValue(this, convertedValue);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to convert environment variable '{prop.Name}' to type {targetType.Name}", ex);
                }
            }
        }
    }
}
