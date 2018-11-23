using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Service.Salesforce.Contract.Commands;
using Lykke.Service.Salesforce.Domain;
using Lykke.Service.Salesforce.Domain.Services;

namespace Lykke.Service.Salesforce.Workflow.CommandHandlers
{
    public class SalesforceCommandsHandler
    {
        private readonly ISalesforceService _salesforceService;
        private readonly TimeSpan _retryDelay;
        private readonly ILog _log;

        public SalesforceCommandsHandler(
            ISalesforceService salesforceService,
            TimeSpan retryDelay,
            ILogFactory logFactory)
        {
            _salesforceService = salesforceService;
            _retryDelay = retryDelay;
            _log = logFactory.CreateLog(this);
        }
        
        public async Task<CommandHandlingResult> Handle(CreateContactCommand command)
        {
            try
            {
                if (!command.Email.IsValidEmail())
                {
                    _log.Warning(nameof(CreateContactCommand), "Invalid email");
                    return CommandHandlingResult.Ok();
                }
                
                await _salesforceService.CreateContactAsync(command.Email, command.PartnerId);
            }
            catch (Exception e)
            {
                _log.Error(nameof(CreateContactCommand), e);

                return CommandHandlingResult.Fail(_retryDelay);
            }

            return CommandHandlingResult.Ok();
        }
        
        public async Task<CommandHandlingResult> Handle(UpdateContactCommand command)
        {
            try
            {
                if (!command.Email.IsValidEmail())
                {
                    _log.Warning(nameof(CreateContactCommand), "Invalid email");
                    return CommandHandlingResult.Ok();
                }
                
                if (string.IsNullOrEmpty(command.ClientId))
                {
                    _log.Warning(nameof(CreateContactCommand), "ClientId can't be empty");
                    return CommandHandlingResult.Ok();
                }
                
                await _salesforceService.UpdateContactAsync(new UpdateContactInfoRequest
                {
                    Email = command.Email, 
                    PartnerId = command.PartnerId, 
                    FirstName = command.FirstName, 
                    LastName = command.LastName,
                    Phone = command.Phone,
                    Country = command.Country,
                    ClientId = command.ClientId
                });
            }
            catch (Exception e)
            {
                _log.Error(nameof(CreateContactCommand), e);

                return CommandHandlingResult.Fail(_retryDelay);
            }

            return CommandHandlingResult.Ok();
        }
    }
}
