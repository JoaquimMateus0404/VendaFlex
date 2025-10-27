using AutoMapper;
using FluentValidation;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.Core.Utils;
using VendaFlex.Data.Repositories;

namespace VendaFlex.Core.Services
{
    public class ExpirationService : IExpirationService
    {
        private readonly ExpirationRepository _expirationRepository;
        private readonly IValidator<ExpirationDto> _expirationDtoValidator;
        private readonly IMapper _mapper;

        public ExpirationService(
            ExpirationRepository expirationRepository,
            IValidator<ExpirationDto> expirationDtoValidator,
            IMapper mapper)
        {
            _expirationRepository = expirationRepository;
            _expirationDtoValidator = expirationDtoValidator;
            _mapper = mapper;
        }

        public Task<OperationResult<ExpirationDto>> AddAsync(ExpirationDto expiration)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<IEnumerable<ExpirationDto>>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<IEnumerable<ExpirationDto>>> GetByBatchNumberAsync(string batchNumber)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<IEnumerable<ExpirationDto>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<ExpirationDto>> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<IEnumerable<ExpirationDto>>> GetByProductIdAsync(int productId)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<IEnumerable<ExpirationDto>>> GetExpiredAsync()
        {
            throw new NotImplementedException();
        }

        public Task<int> GetExpiredQuantityByProductAsync(int productId)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<IEnumerable<ExpirationDto>>> GetNearExpirationAsync()
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<IEnumerable<ExpirationDto>>> GetPaginatedAsync(int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetTotalCountAsync()
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<ExpirationDto>> UpdateAsync(ExpirationDto expiration)
        {
            throw new NotImplementedException();
        }
    }
}
