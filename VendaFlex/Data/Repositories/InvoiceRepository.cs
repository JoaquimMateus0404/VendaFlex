using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VendaFlex.Data.Entities;

namespace VendaFlex.Data.Repositories
{
    /// <summary>
    /// Repositório para operações relacionadas a faturas/vendas.
    /// </summary>
    public class InvoiceRepository
    {
        private readonly ApplicationDbContext _context;

        public InvoiceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Basic CRUD
        public async Task<Invoice?> GetByIdAsync(int id)
        {
            return await _context.Invoices
                .Include(i => i.Person)
                .Include(i => i.User)
                .Include(i => i.InvoiceProducts)
                .ThenInclude(ip => ip.Product)
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);
        }

        public async Task<IEnumerable<Invoice>> GetAllAsync()
        {
            return await _context.Invoices
                .Include(i => i.Person)
                .Include(i => i.User)
                .AsNoTracking()
                .OrderByDescending(i => i.Date)
                .ToListAsync();
        }

        public async Task<Invoice> AddAsync(Invoice entity)
        {
            await _context.Invoices.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Invoice> UpdateAsync(Invoice entity)
        {
            _context.Invoices.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Invoices.FindAsync(id);
            if (entity == null) return false;
            _context.Invoices.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        #endregion

        #region Queries
        public async Task<Invoice?> GetByNumberAsync(string invoiceNumber)
        {
            return await _context.Invoices
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);
        }

        public async Task<IEnumerable<Invoice>> GetByStatusAsync(InvoiceStatus status)
        {
            return await _context.Invoices
                .Where(i => i.Status == status)
                .AsNoTracking()
                .OrderByDescending(i => i.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetByPersonIdAsync(int personId)
        {
            return await _context.Invoices
                .Where(i => i.PersonId == personId)
                .AsNoTracking()
                .OrderByDescending(i => i.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetByDateRangeAsync(DateTime start, DateTime end)
        {
            return await _context.Invoices
                .Where(i => i.Date >= start && i.Date <= end)
                .AsNoTracking()
                .OrderByDescending(i => i.Date)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Invoices.AnyAsync(i => i.InvoiceId == id);
        }

        public async Task<bool> NumberExistsAsync(string invoiceNumber, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(invoiceNumber)) return false;
            if (excludeId.HasValue)
            {
                return await _context.Invoices.AnyAsync(i => i.InvoiceNumber == invoiceNumber && i.InvoiceId != excludeId.Value);
            }
            return await _context.Invoices.AnyAsync(i => i.InvoiceNumber == invoiceNumber);
        }

        public async Task<IEnumerable<Invoice>> GetPagedAsync(int pageNumber, int pageSize)
        {
            return await _context.Invoices
                .OrderByDescending(i => i.Date)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Invoices.CountAsync();
        }
        #endregion
    }
}
