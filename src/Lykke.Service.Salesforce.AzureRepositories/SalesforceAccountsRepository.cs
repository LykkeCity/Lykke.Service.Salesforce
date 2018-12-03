using System.Threading.Tasks;
using AzureStorage;
using Common;
using Lykke.Service.Salesforce.Domain.Repositories;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.Salesforce.AzureRepositories
{
    public class SalesforceContactEntity : TableEntity
    {
        public string ContactId { get; set; }

        public static SalesforceContactEntity Create(string contactId, string email, string partnerId)
        {
            return new SalesforceContactEntity
            {
                PartitionKey = GetPartitionKey(email, partnerId),
                RowKey = GetRowKey(email, partnerId),
                ContactId = contactId
            };
        }

        public static string GetPartitionKey(string email, string partnerId) => $"{GetKey(email, partnerId)}_PK";
        public static string GetRowKey(string email, string partnerId) => $"{GetKey(email, partnerId)}_RK";
        
        private static string GetKey(string email, string partnerId) => $"{email.SanitizeEmail()}{GetPartnerIdPrefix(partnerId)}";

        private static string GetPartnerIdPrefix(string partnerId) => string.IsNullOrEmpty(partnerId)
            ? string.Empty
            : $"_{partnerId}";
    }
    
    public class SalesforceContactsRepository : ISalesforceContactsRepository
    {
        private readonly INoSQLTableStorage<SalesforceContactEntity> _tableStorage;

        public SalesforceContactsRepository(INoSQLTableStorage<SalesforceContactEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }
        
        public Task CreateContactAsync(string contactId, string email, string partnerId)
        {
            var entity = SalesforceContactEntity.Create(contactId, email, partnerId);
            return _tableStorage.TryInsertAsync(entity);
        }

        public async Task<string> GetContactIdByEmailAsync(string email, string partnerId)
        {
            var entity = await _tableStorage.GetDataAsync(
                SalesforceContactEntity.GetPartitionKey(email, partnerId),
                SalesforceContactEntity.GetRowKey(email, partnerId));

            return entity?.ContactId;
        }
    }
}
