using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VendaFlex.Data.Entities;

namespace VendaFlex.Data.Repositories
{
    public class InvoiceProductRepository
    {
        private readonly ApplicationDbContext _context;
        public InvoiceProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Basic CRUD
        public async Task<InvoiceProduct?> GetByIdAsync(int id)
        {
            return await _context.InvoiceProducts
                .Include(ip => ip.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync(ip => ip.InvoiceProductId == id);
        }

        public async Task<IEnumerable<InvoiceProduct>> GetByInvoiceIdAsync(int invoiceId)
        {
            return await _context.InvoiceProducts
                .Include(ip => ip.Product)
                .Where(ip => ip.InvoiceId == invoiceId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<InvoiceProduct> AddAsync(InvoiceProduct entity)
        {
            await _context.InvoiceProducts.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<InvoiceProduct> UpdateAsync(InvoiceProduct entity)
        {
            _context.InvoiceProducts.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.InvoiceProducts.FindAsync(id);
            if (entity == null) return false;
            _context.InvoiceProducts.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        #endregion

        #region Checks
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.InvoiceProducts.AnyAsync(ip => ip.InvoiceProductId == id);
        }

        public async Task<bool> ExistsProductInInvoiceAsync(int invoiceId, int productId, int? excludeId = null)
        {
            if (excludeId.HasValue)
            {
                return await _context.InvoiceProducts.AnyAsync(ip => ip.InvoiceId == invoiceId && ip.ProductId == productId && ip.InvoiceProductId != excludeId.Value);
            }
            return await _context.InvoiceProducts.AnyAsync(ip => ip.InvoiceId == invoiceId && ip.ProductId == productId);
        }
        #endregion
    }
}
