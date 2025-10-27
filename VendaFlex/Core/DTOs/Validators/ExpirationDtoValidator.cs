using FluentValidation;

namespace VendaFlex.Core.DTOs.Validators
{
    public class ExpirationDtoValidator : AbstractValidator<ExpirationDto>
    {
        public ExpirationDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("O produto é obrigatório");

            RuleFor(x => x.ExpirationDate)
                .NotEmpty().WithMessage("A data de validade é obrigatória")
                .GreaterThan(DateTime.Now).WithMessage("A data de validade deve ser futura");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("A quantidade deve ser maior que 0");

            RuleFor(x => x.BatchNumber)
                .MaximumLength(100).WithMessage("O número do lote deve ter no máximo 100 caracteres");

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("As observações devem ter no máximo 500 caracteres");
        }
    }
}
