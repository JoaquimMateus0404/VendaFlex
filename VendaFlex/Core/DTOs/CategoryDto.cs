namespace VendaFlex.Core.DTOs
{
    public class CategoryDto
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
        public int? ParentCategoryId { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        
        // Propriedade adicional para exibição
        public int ProductCount { get; set; }
    }
}
