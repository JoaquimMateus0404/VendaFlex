using AutoMapper;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.Data.Repositories;

namespace VendaFlex.Core.Services
{
    public class InvoiceProductService : IInvoiceProductService
    {
        private readonly InvoiceProductRepository _invoiceProductRepository;
        private readonly IValidator<InvoiceProductDto> _invoiceProductValidator;
        private readonly IMapper _mapper;
        public InvoiceProductService(
            InvoiceProductRepository invoiceProductRepository,
            IValidator<InvoiceProductDto> invoiceProductValidator,
            IMapper mapper)
        {
            _invoiceProductRepository = invoiceProductRepository;
            _invoiceProductValidator = invoiceProductValidator;
            _mapper = mapper;
        }
    }
}
