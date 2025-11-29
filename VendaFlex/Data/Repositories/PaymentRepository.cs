using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VendaFlex.Data.Entities;

namespace VendaFlex.Data.Repositories
{
    /// <summary>
    /// Repositório para operações relacionadas a pagamentos.
    /// </summary>
    public class PaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Basic CRUD
        public async Task<Payment?> GetByIdAsync(int id)
        {
            return await _context.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PaymentId == id);
        }

        public async Task<IEnumerable<Payment>> GetByInvoiceIdAsync(int invoiceId)
        {
            return await _context.Payments
                .Where(p => p.InvoiceId == invoiceId)
                .AsNoTracking()
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetByPaymentTypeIdAsync(int paymentTypeId)
        {
            return await _context.Payments
                .Where(p => p.PaymentTypeId == paymentTypeId)
                .AsNoTracking()
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime start, DateTime end)
        {
            return await _context.Payments
                .Where(p => p.PaymentDate >= start && p.PaymentDate <= end)
                .AsNoTracking()
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<Payment> AddAsync(Payment entity)
        {
            await _context.Payments.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Payment> UpdateAsync(Payment entity)
        {
            // Buscar a entidade existente do contexto
            var existingEntity = await _context.Payments.FindAsync(entity.PaymentId);
            
            if (existingEntity == null)
            {
                throw new InvalidOperationException($"Payment with ID {entity.PaymentId} not found.");
            }

            // Atualizar apenas as propriedades necessárias
            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
            
            await _context.SaveChangesAsync();
            return existingEntity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Payments.FindAsync(id);
            if (entity == null) return false;
            _context.Payments.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Payments.AnyAsync(p => p.PaymentId == id);
        }

        public async Task<decimal> GetTotalAmountByInvoiceIdAsync(int invoiceId)
        {
            return await _context.Payments
                .Where(p => p.InvoiceId == invoiceId && p.IsConfirmed)
                .SumAsync(p => (decimal?)p.Amount) ?? 0m;
        }
        #endregion
    }
}
