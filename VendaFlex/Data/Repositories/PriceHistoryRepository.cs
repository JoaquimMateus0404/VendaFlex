
namespace VendaFlex.Data.Repositories
{
    public class PriceHistoryRepository
    {
        private readonly ApplicationDbContext _context;

        public PriceHistoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }
    }
}
