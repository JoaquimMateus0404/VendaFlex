using AutoMapper;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VendaFlex.Core.Interfaces;
using VendaFlex.Data.Repositories;

namespace VendaFlex.Core.Services
{
    public class PaymentTypeService : IPaymentTypeService
    {
        private readonly PaymentTypeRepository _paymentTypeRepository;
        private readonly IValidator<PaymentTypeService> _validator;
        private readonly IMapper _mapper;

        public PaymentTypeService(PaymentTypeRepository paymentTypeRepository, IValidator<PaymentTypeService> validator, IMapper mapper)
        {
            _paymentTypeRepository = paymentTypeRepository;
            _validator = validator;
            _mapper = mapper;
        }
    }
}
