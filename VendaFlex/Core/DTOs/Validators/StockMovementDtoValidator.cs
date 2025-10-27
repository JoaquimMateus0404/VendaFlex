using FluentValidation;
using VendaFlex.Data.Entities;

namespace VendaFlex.Core.DTOs.Validators
{
    public class StockMovementDtoValidator : AbstractValidator<StockMovementDto>
    {
        public StockMovementDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("O produto é obrigatório");

            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("O usuário é obrigatório");

            RuleFor(x => x.Quantity)
                .NotEqual(0).WithMessage("A quantidade não pode ser zero")
                .WithMessage("A quantidade é obrigatória");

            RuleFor(x => x.Date)
                .NotEmpty().WithMessage("A data é obrigatória")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("A data não pode ser futura");

            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Tipo de movimento inválido");

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("As observações devem ter no máximo 500 caracteres");

            RuleFor(x => x.Reference)
                .MaximumLength(100).WithMessage("A referência deve ter no máximo 100 caracteres");

            RuleFor(x => x.UnitCost)
                .GreaterThanOrEqualTo(0).When(x => x.UnitCost.HasValue)
                .WithMessage("O custo unitário deve ser maior ou igual a 0")
                .WithMessage("O custo unitário deve ter no máximo 2 casas decimais");

            RuleFor(x => x.TotalCost)
                .GreaterThanOrEqualTo(0).When(x => x.TotalCost.HasValue)
                .WithMessage("O custo total deve ser maior ou igual a 0")
                .WithMessage("O custo total deve ter no máximo 2 casas decimais");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).When(x => x.Type == StockMovementType.Entry)
                .WithMessage("A quantidade de entrada deve ser maior que 0");

            RuleFor(x => x.Quantity)
                .LessThan(0).When(x => x.Type == StockMovementType.Exit || x.Type == StockMovementType.Loss)
                .WithMessage("A quantidade de saída/perda deve ser negativa");
        }
    }
}
