using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage.Tables;
using Lykke.Logs;
using Lykke.Service.Salesforce.AzureRepositories;
using Lykke.Service.Salesforce.Domain;
using Lykke.Service.Salesforce.DomainServices;
using Microsoft.Extensions.Configuration;
using Salesforce.Common;
using Xunit;

namespace Lykke.Service.Salesforce.Tests
{
    public class SalesforceServiceTests
    {
        [Fact(Skip = "manual test")]
        public async Task IsContactCreatedAndUpdated()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.tests.json")
                .Build();
            
            var settings = new ApiSettings();
            config.Bind(settings);
            
            var repository = new SalesforceContactsRepository(new NoSqlTableInMemory<SalesforceContactEntity>());
            
            var service = new SalesforceService(settings, new AuthenticationClient(), repository, EmptyLogFactory.Instance);

            await service.CreateContactAsync("test1@test.com", null);
            await service.UpdateContactAsync(new UpdateContactInfoRequest
            {
                Email = "test1@test.com",
                PartnerId = null,
                Properties = new Dictionary<string, string>
                {
                    { "FirstName", "FirstName" },
                    { "LastName", "Lastname" },
                    { "Phone", "7777" },
                    { "MailingCountry", "RUS" },
                }
            });
        }
    }
}
