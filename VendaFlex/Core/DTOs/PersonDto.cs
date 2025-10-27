using VendaFlex.Data.Entities;

namespace VendaFlex.Core.DTOs
{
    public class PersonDto
    {
        public int PersonId { get; set; }
        public string Name { get; set; }
        public PersonType Type { get; set; }
        public string TaxId { get; set; }
        public string IdentificationNumber { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string MobileNumber { get; set; }
        public string Website { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal CurrentBalance { get; set; }
        public string ContactPerson { get; set; }
        public string Notes { get; set; }
        public string ProfileImageUrl { get; set; }
        public bool IsActive { get; set; }
        public int? Rating { get; set; }
    }
}
