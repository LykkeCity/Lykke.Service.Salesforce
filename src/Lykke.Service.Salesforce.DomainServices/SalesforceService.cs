using System;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.Salesforce.Domain;
using Lykke.Service.Salesforce.Domain.Repositories;
using Lykke.Service.Salesforce.Domain.Services;
using Polly;
using Salesforce.Common;
using Salesforce.Common.Models.Json;
using Salesforce.Force;

namespace Lykke.Service.Salesforce.DomainServices
{
    public class SalesforceService : ISalesforceService
    {
        private readonly ApiSettings _settings;
        private readonly IAuthenticationClient _authenticationClient;
        private readonly ISalesforceContactsRepository _contactsRepository;
        private IForceClient _forceClient;
        private bool _authenticated;
        private readonly ILog _log;
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1,1);
        private const string ContactObjName = "Contact";

        public SalesforceService(
            ApiSettings settings,
            IAuthenticationClient authenticationClient,
            ISalesforceContactsRepository contactsRepository,
            ILogFactory logFactory)
        {
            _settings = settings;
            _authenticationClient = authenticationClient;
            _contactsRepository = contactsRepository;
            _log = logFactory.CreateLog(this);
        }
        
        public Task CreateContactAsync(string email, string partnerId)
        {
            return CallAsync(async () =>
            {
                var contactId = await _contactsRepository.GetContactIdByEmailAsync(email, partnerId);
                
                if (!string.IsNullOrEmpty(contactId))
                {
                    _log.Warning(nameof(CreateContactAsync), "Contact already exists", context: new { email = email.SanitizeEmail(), partnerId });
                    return;
                }
                
                var response = await _forceClient.CreateAsync(ContactObjName, new { LastName = "not set", Email = email });
                
                if (response.Success)
                {
                    await _contactsRepository.CreateContactAsync(response.Id, email, partnerId);
                }
                else
                {
                    _log.Warning(nameof(CreateContactAsync), "can't create contact in salesforce", context: response.Errors);
                }
            });
        }

        public async Task UpdateContactAsync(UpdateContactInfoRequest request)
        {
            await CallAsync(async () =>
            {
                var contactId = await _contactsRepository.GetContactIdByEmailAsync(request.Email, request.PartnerId);

                if (!string.IsNullOrEmpty(contactId))
                {
                    await _forceClient.UpdateAsync(ContactObjName, contactId, new
                    {
                        FirstName = request.FirstName,
                        LastName = request.LastName,
                        Phone = request.Phone,
                        MailingCountry = request.Country,
                        sub_Id__c = request.ClientId
                    });
                }
            });
        }

        private async Task CallAsync(Func<Task> action)
        {
            if (!_authenticated || _forceClient == null)
            {
                await AuthorizeAsync();
            }

            await Policy
                .Handle<ForceAuthException>()
                .Or<ForceException>(exception =>
                {
                    if (exception.Error == Error.InvalidSessionId)
                    {
                        _authenticated = false;
                        return true;
                    }
                    
                    _log.Warning(nameof(CallAsync), exception);
                    return false;
                })
                .RetryAsync(async (exception, timeSpan) =>
                {
                    await AuthorizeAsync();
                })
                .ExecuteAsync(action);
        }

        private async Task AuthorizeAsync()
        {
            await _semaphoreSlim.WaitAsync();

            try
            {
                if (_authenticated)
                    return;

                await _authenticationClient.UsernamePasswordAsync(_settings.ClientId, _settings.ClientSecret,
                    _settings.Username, $"{_settings.Password}{_settings.SecurityToken}");
                _forceClient = new ForceClient(_authenticationClient.InstanceUrl, _authenticationClient.AccessToken,
                    _authenticationClient.ApiVersion);
                _authenticated = true;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
    }
}
