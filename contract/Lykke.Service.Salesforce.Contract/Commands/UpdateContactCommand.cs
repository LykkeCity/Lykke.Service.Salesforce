namespace Lykke.Service.Salesforce.Contract.Commands
{
    public class UpdateContactCommand
    {
        public string Email { get; set; }
        public string PartnerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Country { get; set; }
        public string ClientId { get; set; }
    }
}
