using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VendaFlex.Data.Entities;

namespace VendaFlex.Data.Repositories
{
    /// <summary>
    /// Repositório para operações relacionadas aos tipos de despesas.
    /// </summary>
    public class ExpenseTypeRepository
    {
        private readonly ApplicationDbContext _context;

        public ExpenseTypeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

       
    }
}
