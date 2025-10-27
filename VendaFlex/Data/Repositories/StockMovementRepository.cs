using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendaFlex.Data.Repositories
{
    public class StockMovementRepository
    {
        private readonly ApplicationDbContext _context;
        public StockMovementRepository(ApplicationDbContext context)
        {
            _context = context;
        }
    }
}
