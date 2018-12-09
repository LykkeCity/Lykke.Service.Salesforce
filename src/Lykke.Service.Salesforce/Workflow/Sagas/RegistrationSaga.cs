using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Service.Registration.Contract.Events;
using Lykke.Service.Salesforce.Domain;
using Lykke.Service.Salesforce.Domain.Services;

namespace Lykke.Service.Salesforce.Workflow.Sagas
{
    public class RegistrationSaga
    {
        private readonly ISalesforceService _salesforceService;
        private readonly ILog _log;

        public RegistrationSaga(
            ISalesforceService salesforceService,
            ILogFactory logFactory)
        {
            _salesforceService = salesforceService;
            _log = logFactory.CreateLog(this);
        }

        [UsedImplicitly]
        public async Task Handle(ClientRegisteredEvent evt, ICommandSender commandSender)
        {
            try
            {
                if (!evt.Email.IsValidEmail())
                {
                    _log.Warning(nameof(ClientRegisteredEvent), "Invalid email");
                    return;
                }
                
                if (string.IsNullOrEmpty(evt.ClientId))
                {
                    _log.Warning(nameof(ClientRegisteredEvent), "ClientId can't be empty");
                    return;
                }

                var properties = new Dictionary<string, string>()
                {
                    {"FirstName", evt.FirstName},
                    {"LastName", evt.LastName},
                    {"Phone", evt.Phone},
                    {"MailingCountry", evt.CountryFromPOA},
                    {"sub_Id__c", evt.ClientId}
                };
                
                string contactId = await _salesforceService.GetContactIdAsync(evt.Email, null);

                if (string.IsNullOrEmpty(contactId))
                {
                    await _salesforceService.CreateContactAsync(evt.Email, null, properties);
                }
                else
                {
                    await _salesforceService.UpdateContactAsync(new UpdateContactInfoRequest
                    {
                        Email = evt.Email,
                        Properties = properties
                    });
                }
                
            }
            catch (Exception e)
            {
                _log.Error(nameof(ClientRegisteredEvent), e);
            }
        }
    }
}
