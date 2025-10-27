

namespace VendaFlex.Data.Repositories
{
    public class ExpirationRepository
    {
        private readonly ApplicationDbContext _context;
        public ExpirationRepository(ApplicationDbContext context)
        {
            _context = context;
        }
    }
}
