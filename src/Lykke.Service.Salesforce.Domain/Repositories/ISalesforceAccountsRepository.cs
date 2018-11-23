using System.Threading.Tasks;

namespace Lykke.Service.Salesforce.Domain.Repositories
{
    public interface ISalesforceContactsRepository
    {
        Task CreateContactAsync(string contactId, string email, string partnerId);
        Task<string> GetContactIdByEmailAsync(string email, string partnerId);
    }
}
