using System.Threading.Tasks;

namespace Lykke.Service.Salesforce.Domain.Services
{
    public interface ISalesforceService
    {
        Task CreateContactAsync(string email, string partnerId);
        Task UpdateContactAsync(UpdateContactInfoRequest request);
    }
}
