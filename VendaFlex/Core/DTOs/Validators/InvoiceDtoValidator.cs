using FluentValidation;
using System;

namespace VendaFlex.Core.DTOs.Validators
{
    public class InvoiceDtoValidator : AbstractValidator<InvoiceDto>
    {
        public InvoiceDtoValidator()
        {
            RuleFor(x => x.InvoiceNumber)
                .NotEmpty().WithMessage("Número da fatura é obrigatório.")
                .MaximumLength(50).WithMessage("Número da fatura deve ter no máximo 50 caracteres.");

            RuleFor(x => x.Date)
                .NotEmpty().WithMessage("Data é obrigatória.")
                .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
                .WithMessage("Data da fatura não pode ser no futuro.");

            RuleFor(x => x.DueDate)
                .Must((dto, due) => !due.HasValue || due.Value >= dto.Date)
                .WithMessage("Data de vencimento deve ser maior ou igual à data da fatura.");

            RuleFor(x => x.PersonId)
                .GreaterThan(0).WithMessage("Cliente é obrigatório.");

            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("Usuário é obrigatório.");

            RuleFor(x => x.SubTotal)
                .GreaterThanOrEqualTo(0).WithMessage("Subtotal não pode ser negativo.");

            RuleFor(x => x.TaxAmount)
                .GreaterThanOrEqualTo(0).WithMessage("Impostos não podem ser negativos.");

            RuleFor(x => x.DiscountAmount)
                .GreaterThanOrEqualTo(0).WithMessage("Descontos não podem ser negativos.");

            RuleFor(x => x.ShippingCost)
                .GreaterThanOrEqualTo(0).WithMessage("Custos de envio não podem ser negativos.");

            RuleFor(x => x.Total)
                .GreaterThanOrEqualTo(0).WithMessage("Total não pode ser negativo.")
                .Must((dto, total) => total == Math.Round(dto.SubTotal + dto.TaxAmount - dto.DiscountAmount + dto.ShippingCost, 2))
                .WithMessage("Total deve ser igual a SubTotal + TaxAmount - DiscountAmount + ShippingCost.");

            RuleFor(x => x.PaidAmount)
                .GreaterThanOrEqualTo(0).WithMessage("Valor pago não pode ser negativo.")
                .LessThanOrEqualTo(x => x.Total).WithMessage("Valor pago não pode exceder o total.");

            RuleFor(x => x.Notes)
                .MaximumLength(1000).When(x => !string.IsNullOrWhiteSpace(x.Notes));

            RuleFor(x => x.InternalNotes)
                .MaximumLength(1000).When(x => !string.IsNullOrWhiteSpace(x.InternalNotes));
        }
    }
}
