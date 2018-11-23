using JetBrains.Annotations;
using Lykke.Service.Salesforce.Domain;

namespace Lykke.Service.Salesforce.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class SalesforceSettings
    {
        public DbSettings Db { get; set; }
        public ApiSettings Api { get; set; }
        public RabbitMqSettings SagasRabbitMq { get; set; }
    }
}
