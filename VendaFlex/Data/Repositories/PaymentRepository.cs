using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Data.Entities;

namespace CommerceHub.Data.Repositories
{
    /// <summary>
    /// Repositório para operações relacionadas a pagamentos.
    /// </summary>
    public class PaymentRepository : IRepository<Payment>
    {
        private readonly ApplicationDbContext _context;

        public PaymentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Payment> GetByIdAsync(int id)
        {
            return await _context.Payments.FindAsync(id);
        }

        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            return await _context.Payments.ToListAsync();
        }

        public async Task<IEnumerable<Payment>> FindAsync(System.Linq.Expressions.Expression<System.Func<Payment, bool>> predicate)
        {
            return await _context.Payments.Where(predicate).ToListAsync();
        }

        public async Task<Payment> AddAsync(Payment entity)
        {
            _context.Payments.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Payment> UpdateAsync(Payment entity)
        {
            _context.Payments.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return false;
            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Payment>> GetPagedAsync(int page, int pageSize)
        {
            return await _context.Payments
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Obtém todos os tipos de pagamento.
        /// </summary>
        public async Task<IEnumerable<PaymentType>> GetPaymentTypesAsync()
        {
            return await _context.PaymentTypes.ToListAsync();
        }
    }
}
