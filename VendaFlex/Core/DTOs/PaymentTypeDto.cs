namespace VendaFlex.Core.DTOs
{
    public class PaymentTypeDto
    {
        public int PaymentTypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public bool RequiresReference { get; set; }
    }
}
