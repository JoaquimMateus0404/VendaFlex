using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VendaFlex.Data.Entities;
using VendaFlex.Core.DTOs;

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

        /// <summary>
        /// Retorna os produtos mais vendidos agregando quantidade e receita.
        /// </summary>
        public async Task<IEnumerable<TopProductDto>> GetTopSellingProductsAsync(int top)
        {
            if (top <= 0) top = 5;

            var query = await _context.InvoiceProducts
                .AsNoTracking()
                .Include(ip => ip.Product)
                .GroupBy(ip => new { ip.ProductId, ip.Product.Name })
                .Select(g => new TopProductDto
                {
                    ProductName = g.Key.Name,
                    QuantitySold = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => (x.UnitPrice * x.Quantity) - ((x.UnitPrice * x.Quantity) * (x.DiscountPercentage / 100m)))
                })
                .OrderByDescending(x => x.QuantitySold)
                .Take(top)
                .ToListAsync();

            // Calcular ProgressPercentage relativo ao maior
            var maxQty = query.Count > 0 ? query.Max(x => x.QuantitySold) : 0;
            if (maxQty > 0)
            {
                foreach (var item in query)
                {
                    item.ProgressPercentage = Math.Round((item.QuantitySold / (double)maxQty) * 100.0, 2);
                }
            }

            return query;
        }
    }
}
