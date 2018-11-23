using JetBrains.Annotations;
using Lykke.Sdk.Settings;

namespace Lykke.Service.Salesforce.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public SalesforceSettings SalesforceService { get; set; }
    }
}
