using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendaFlex.Data.Repositories
{
    public class InvoiceProductRepository
    {
        private readonly ApplicationDbContext _context;
        public InvoiceProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }
    }
}
