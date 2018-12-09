using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.Salesforce.Domain.Services
{
    public interface ISalesforceService
    {
        Task CreateContactAsync(string email, string partnerId, Dictionary<string, string> properties = null);
        Task UpdateContactAsync(UpdateContactInfoRequest request);
        Task<string> GetContactIdAsync(string email, string partnerId);
    }
}
