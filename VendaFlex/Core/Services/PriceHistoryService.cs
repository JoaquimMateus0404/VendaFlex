using AutoMapper;
using FluentValidation;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.Core.Utils;
using VendaFlex.Data.Entities;
using VendaFlex.Data.Repositories;

namespace VendaFlex.Core.Services
{
    public class PriceHistoryService : IPriceHistoryService
    {
        private readonly PriceHistoryRepository _priceHistoryRepository;
        private readonly IValidator<PriceHistoryDto> _validator;
        private readonly IMapper _mapper;

        public PriceHistoryService(
            PriceHistoryRepository priceHistoryRepository,
            IValidator<PriceHistoryDto> validator,
            IMapper mapper)
        {
            _priceHistoryRepository = priceHistoryRepository;
            _validator = validator;
            _mapper = mapper;
        }

        public Task<OperationResult<PriceHistoryDto>> AddAsync(PriceHistoryDto priceHistoryDto)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<IEnumerable<PriceHistoryDto>>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<IEnumerable<PriceHistoryDto>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<PriceHistoryDto>> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<IEnumerable<PriceHistoryDto>>> GetByProductIdAsync(int productId)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<PriceHistoryDto>> GetLatestByProductIdAsync(int productId)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<IEnumerable<PriceHistoryDto>>> GetPaginatedAsync(int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<IEnumerable<PriceHistoryDto>>> GetPriceDecreaseHistoryAsync()
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<IEnumerable<PriceHistoryDto>>> GetPriceIncreaseHistoryAsync()
        {
            throw new NotImplementedException();
        }

        public Task<int> GetTotalCountAsync()
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<PriceHistoryDto>> UpdateAsync(PriceHistoryDto priceHistoryDto)
        {
            throw new NotImplementedException();
        }
    }
}
