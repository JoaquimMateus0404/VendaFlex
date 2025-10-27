using FluentValidation;
using System;

namespace VendaFlex.Core.DTOs.Validators
{
    public class InvoiceDtoValidator : AbstractValidator<InvoiceDto>
    {
        public InvoiceDtoValidator()
        {
            RuleFor(x => x.InvoiceNumber)
                .NotEmpty().WithMessage("N�mero da fatura � obrigat�rio.")
                .MaximumLength(50).WithMessage("N�mero da fatura deve ter no m�ximo 50 caracteres.");

            RuleFor(x => x.Date)
                .NotEmpty().WithMessage("Data � obrigat�ria.")
                .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
                .WithMessage("Data da fatura n�o pode ser no futuro.");

            RuleFor(x => x.DueDate)
                .Must((dto, due) => !due.HasValue || due.Value >= dto.Date)
                .WithMessage("Data de vencimento deve ser maior ou igual � data da fatura.");

            RuleFor(x => x.PersonId)
                .GreaterThan(0).WithMessage("Cliente � obrigat�rio.");

            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("Usu�rio � obrigat�rio.");

            RuleFor(x => x.SubTotal)
                .GreaterThanOrEqualTo(0).WithMessage("Subtotal n�o pode ser negativo.");

            RuleFor(x => x.TaxAmount)
                .GreaterThanOrEqualTo(0).WithMessage("Impostos n�o podem ser negativos.");

            RuleFor(x => x.DiscountAmount)
                .GreaterThanOrEqualTo(0).WithMessage("Descontos n�o podem ser negativos.");

            RuleFor(x => x.ShippingCost)
                .GreaterThanOrEqualTo(0).WithMessage("Custos de envio n�o podem ser negativos.");

            RuleFor(x => x.Total)
                .GreaterThanOrEqualTo(0).WithMessage("Total n�o pode ser negativo.")
                .Must((dto, total) => total == Math.Round(dto.SubTotal + dto.TaxAmount - dto.DiscountAmount + dto.ShippingCost, 2))
                .WithMessage("Total deve ser igual a SubTotal + TaxAmount - DiscountAmount + ShippingCost.");

            RuleFor(x => x.PaidAmount)
                .GreaterThanOrEqualTo(0).WithMessage("Valor pago n�o pode ser negativo.")
                .LessThanOrEqualTo(x => x.Total).WithMessage("Valor pago n�o pode exceder o total.");

            RuleFor(x => x.Notes)
                .MaximumLength(1000).When(x => !string.IsNullOrWhiteSpace(x.Notes));

            RuleFor(x => x.InternalNotes)
                .MaximumLength(1000).When(x => !string.IsNullOrWhiteSpace(x.InternalNotes));
        }
    }
}
