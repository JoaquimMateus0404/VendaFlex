

namespace VendaFlex.Data.Repositories
{
    /// <summary>
    /// Repositório para operações relacionadas a categorias de produtos.
    /// </summary>
    public class CategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

    }
}
