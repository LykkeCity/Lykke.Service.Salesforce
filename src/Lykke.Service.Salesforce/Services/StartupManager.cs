using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Sdk;

namespace Lykke.Service.Salesforce.Services
{
    [UsedImplicitly]
    public class StartupManager : IStartupManager
    {
        private readonly ICqrsEngine _cqrsEngine;

        public StartupManager(ICqrsEngine cqrsEngine)
        {
            _cqrsEngine = cqrsEngine;
        }
        
        public Task StartAsync()
        {
            _cqrsEngine.Start();

            return Task.CompletedTask;
        }
    }
}
