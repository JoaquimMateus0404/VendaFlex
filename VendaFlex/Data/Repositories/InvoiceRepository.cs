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
    public class InvoiceRepository : IRepository<Invoice>
    {
        private readonly ApplicationDbContext _context;

        public InvoiceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Invoice> GetByIdAsync(int id)
        {
            return await _context.Invoices.FindAsync(id);
        }

        public async Task<IEnumerable<Invoice>> GetAllAsync()
        {
            return await _context.Invoices.ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> FindAsync(System.Linq.Expressions.Expression<System.Func<Invoice, bool>> predicate)
        {
            return await _context.Invoices.Where(predicate).ToListAsync();
        }

        public async Task<Invoice> AddAsync(Invoice entity)
        {
            _context.Invoices.Add(entity);
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
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null) return false;
            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Invoice>> GetPagedAsync(int page, int pageSize)
        {
            return await _context.Invoices
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Obtém faturas de um cliente.
        /// </summary>
        public async Task<IEnumerable<Invoice>> GetByCustomerAsync(int personId)
        {
            return await _context.Invoices
                .Where(i => i.PersonId == personId)
                .ToListAsync();
        }

        /// <summary>
        /// Obtém produtos de uma fatura.
        /// </summary>
        public async Task<IEnumerable<InvoiceProduct>> GetProductsAsync(int invoiceId)
        {
            return await _context.InvoiceProducts
                .Where(ip => ip.InvoiceId == invoiceId)
                .ToListAsync();
        }

        /// <summary>
        /// Registra pagamento de uma fatura.
        /// </summary>
        public async Task<bool> RegisterPaymentAsync(Payment payment)
        {
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
