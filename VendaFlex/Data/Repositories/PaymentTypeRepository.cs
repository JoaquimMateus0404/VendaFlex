using Microsoft.EntityFrameworkCore;
using VendaFlex.Data.Entities;

namespace VendaFlex.Data.Repositories
{
    public class PaymentTypeRepository
    {
        private readonly ApplicationDbContext _context;
        public PaymentTypeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Basic CRUD
        public async Task<PaymentType?> GetByIdAsync(int id)
        {
            return await _context.PaymentTypes.AsNoTracking().FirstOrDefaultAsync(x => x.PaymentTypeId == id);
        }

        public async Task<IEnumerable<PaymentType>> GetAllAsync()
        {
            return await _context.PaymentTypes.AsNoTracking().OrderBy(x => x.Name).ToListAsync();
        }

        public async Task<IEnumerable<PaymentType>> GetActiveAsync()
        {
            return await _context.PaymentTypes.Where(x => x.IsActive).AsNoTracking().OrderBy(x => x.Name).ToListAsync();
        }

        public async Task<IEnumerable<PaymentType>> SearchAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term)) return Enumerable.Empty<PaymentType>();
            term = term.ToLower();
            return await _context.PaymentTypes
                .Where(x => x.Name.ToLower().Contains(term) || (x.Description != null && x.Description.ToLower().Contains(term)))
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<PaymentType> AddAsync(PaymentType entity)
        {
            await _context.PaymentTypes.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<PaymentType> UpdateAsync(PaymentType entity)
        {
            _context.PaymentTypes.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.PaymentTypes.FindAsync(id);
            if (entity == null) return false;
            _context.PaymentTypes.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        #endregion

        #region Checks & Pagination
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.PaymentTypes.AnyAsync(x => x.PaymentTypeId == id);
        }

        public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            name = name.Trim();
            if (excludeId.HasValue)
            {
                return await _context.PaymentTypes.AnyAsync(x => x.Name == name && x.PaymentTypeId != excludeId.Value);
            }
            return await _context.PaymentTypes.AnyAsync(x => x.Name == name);
        }

        public async Task<IEnumerable<PaymentType>> GetPagedAsync(int pageNumber, int pageSize)
        {
            return await _context.PaymentTypes
                .OrderBy(x => x.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.PaymentTypes.CountAsync();
        }
        #endregion
    }
}
