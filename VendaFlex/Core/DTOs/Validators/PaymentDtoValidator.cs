using FluentValidation;
using System;

namespace VendaFlex.Core.DTOs.Validators
{
    public class PaymentDtoValidator : AbstractValidator<PaymentDto>
    {
        public PaymentDtoValidator()
        {
            RuleFor(x => x.InvoiceId)
                .GreaterThan(0).WithMessage("Fatura é obrigatória.");

            RuleFor(x => x.PaymentTypeId)
                .GreaterThan(0).WithMessage("Tipo de pagamento é obrigatório.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Valor do pagamento deve ser maior que 0.");

            RuleFor(x => x.PaymentDate)
                .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
                .WithMessage("Data do pagamento não pode ser no futuro.");

            RuleFor(x => x.Reference)
                .MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.Reference));

            RuleFor(x => x.Notes)
                .MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Notes));
        }
    }
}
