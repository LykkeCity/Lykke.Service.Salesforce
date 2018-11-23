using Autofac;
using AzureStorage.Tables;
using Lykke.Common.Log;
using Lykke.Sdk;
using Lykke.Service.Salesforce.AzureRepositories;
using Lykke.Service.Salesforce.Domain.Repositories;
using Lykke.Service.Salesforce.Domain.Services;
using Lykke.Service.Salesforce.DomainServices;
using Lykke.Service.Salesforce.Services;
using Lykke.Service.Salesforce.Settings;
using Lykke.SettingsReader;
using Salesforce.Common;

namespace Lykke.Service.Salesforce.Modules
{
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _appSettings;

        public ServiceModule(IReloadingManager<AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<StartupManager>()
                .As<IStartupManager>()
                .SingleInstance();
            
            builder.RegisterType<AuthenticationClient>()
                .As<IAuthenticationClient>()
                .SingleInstance();
            
            builder.RegisterType<SalesforceService>()
                .WithParameter(TypedParameter.From(_appSettings.CurrentValue.SalesforceService.Api))
                .As<ISalesforceService>()
                .SingleInstance();
            
            builder.Register(ctx => 
                    new SalesforceContactsRepository(
                        AzureTableStorage<SalesforceContactEntity>.Create(
                            _appSettings.ConnectionString(x => x.SalesforceService.Db.DataConnString),
                            "SalesforceContacts", ctx.Resolve<ILogFactory>()))
                )
                .As<ISalesforceContactsRepository>()
                .SingleInstance();
        }
    }
}
