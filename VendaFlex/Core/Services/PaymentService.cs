using AutoMapper;
using FluentValidation;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.Data.Entities;
using VendaFlex.Data.Repositories;

namespace VendaFlex.Core.Services
{
    /// <summary>
    /// Serviço para pagamentos e tipos de pagamento.
    /// </summary>
    public class PaymentService : IPaymentService
    {
        private readonly PaymentRepository _paymentRepository;
        private readonly IValidator<PaymentDto> _paymentValidator;
        private readonly IMapper _mapper;

        public PaymentService(
            PaymentRepository paymentRepository,
            IValidator<PaymentDto> paymentValidator,
            IMapper mapper)
        {
            _paymentRepository = paymentRepository;
            _paymentValidator = paymentValidator;
            _mapper = mapper;
        }


    }
}
