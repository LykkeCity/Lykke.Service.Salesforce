using System;

namespace Lykke.Service.Salesforce.Settings
{
    public class RabbitMqSettings
    {
        public string RabbitConnectionString { get; set; }
        public TimeSpan RetryDelay { get; set; }
    }
}
