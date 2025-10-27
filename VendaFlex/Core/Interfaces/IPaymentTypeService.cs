using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Utils;

namespace VendaFlex.Core.Interfaces
{
    public interface IPaymentTypeService
    {
        // Consultas
        Task<OperationResult<PaymentTypeDto>> GetByIdAsync(int id);
        Task<OperationResult<IEnumerable<PaymentTypeDto>>> GetAllAsync();
        Task<OperationResult<IEnumerable<PaymentTypeDto>>> GetActiveAsync();
        Task<OperationResult<IEnumerable<PaymentTypeDto>>> SearchAsync(string term);

        // Verificações
        Task<bool> ExistsAsync(int id);
        Task<bool> NameExistsAsync(string name, int? excludeId = null);

        // CRUD
        Task<OperationResult<PaymentTypeDto>> AddAsync(PaymentTypeDto dto);
        Task<OperationResult<PaymentTypeDto>> UpdateAsync(PaymentTypeDto dto);
        Task<OperationResult<bool>> DeleteAsync(int id);

        // Paginação
        Task<OperationResult<IEnumerable<PaymentTypeDto>>> GetPaginatedAsync(int pageNumber, int pageSize);
        Task<int> GetTotalCountAsync();
    }
}
