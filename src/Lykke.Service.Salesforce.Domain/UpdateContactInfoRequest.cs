using System.Collections.Generic;

namespace Lykke.Service.Salesforce.Domain
{
    public class UpdateContactInfoRequest
    {
        public string Email { get; set; }
        public string PartnerId { get; set; }
        public Dictionary<string, string> Properties { get; set; }
    }
}
