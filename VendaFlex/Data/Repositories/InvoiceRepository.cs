using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using VendaFlex.Data.Entities;

namespace VendaFlex.Data.Repositories
{
    /// <summary>
    /// Reposit�rio para opera��es relacionadas a faturas/vendas.
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
                .Where(i => !i.IsDeleted)
                .Include(i => i.Person)
                .Include(i => i.User)
                .AsNoTracking()
                .OrderByDescending(i => i.Date)
                .ToListAsync();
        }

        public async Task<Invoice> AddAsync(Invoice entity)
        {
            try
            {
                await _context.Invoices.AddAsync(entity);
                await _context.SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Erro no Repositorio Invoice: ", ex.Message);
                throw;
            }
        }

        public async Task<Invoice> UpdateAsync(Invoice entity)
        {
            // Buscar a entidade existente do contexto
            var existingEntity = await _context.Invoices.FindAsync(entity.InvoiceId);
            
            if (existingEntity == null)
            {
                throw new InvalidOperationException($"Invoice with ID {entity.InvoiceId} not found.");
            }

            // Atualizar apenas as propriedades necess�rias, excluindo navega��o
            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
            
            await _context.SaveChangesAsync();
            return existingEntity;
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
                .Where(i => !i.IsDeleted)
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);
        }

        public async Task<IEnumerable<Invoice>> GetByStatusAsync(InvoiceStatus status)
        {
            return await _context.Invoices
                .Where(i => !i.IsDeleted && i.Status == status)
                .AsNoTracking()
                .OrderByDescending(i => i.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetByPersonIdAsync(int personId)
        {
            return await _context.Invoices
                .Where(i => !i.IsDeleted && i.PersonId == personId)
                .AsNoTracking()
                .OrderByDescending(i => i.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetByDateRangeAsync(DateTime start, DateTime end)
        {
            // Normalizar datas: start às 00:00:00 e end às 23:59:59
            var startDate = start.Date; // 00:00:00
            var endDate = end.Date.AddDays(1).AddTicks(-1); // 23:59:59.9999999
            
            Debug.WriteLine($"[InvoiceRepository] GetByDateRangeAsync - Start: {startDate:yyyy-MM-dd HH:mm:ss}, End: {endDate:yyyy-MM-dd HH:mm:ss}");
            
            var result = await _context.Invoices
                .Where(i => !i.IsDeleted && i.Date >= startDate && i.Date <= endDate)
                .AsNoTracking()
                .OrderByDescending(i => i.Date)
                .ToListAsync();
            
            Debug.WriteLine($"[InvoiceRepository] GetByDateRangeAsync - Encontradas {result.Count} faturas");
            foreach (var inv in result)
            {
                Debug.WriteLine($"  - Fatura {inv.InvoiceNumber}, Data: {inv.Date:yyyy-MM-dd HH:mm:ss}, Status: {inv.Status}, Total: {inv.Total}");
            }
            
            return result;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Invoices.AnyAsync(i => i.InvoiceId == id && !i.IsDeleted);
        }

        public async Task<bool> NumberExistsAsync(string invoiceNumber, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(invoiceNumber)) return false;
            if (excludeId.HasValue)
            {
                return await _context.Invoices.AnyAsync(i => !i.IsDeleted && i.InvoiceNumber == invoiceNumber && i.InvoiceId != excludeId.Value);
            }
            return await _context.Invoices.AnyAsync(i => !i.IsDeleted && i.InvoiceNumber == invoiceNumber);
        }

        public async Task<IEnumerable<Invoice>> GetPagedAsync(int pageNumber, int pageSize)
        {
            return await _context.Invoices
                .Where(i => !i.IsDeleted)
                .OrderByDescending(i => i.Date)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Invoices.CountAsync(i => !i.IsDeleted);
        }
        #endregion
    }
}
