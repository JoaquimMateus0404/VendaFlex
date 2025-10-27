using FluentValidation;

namespace VendaFlex.Core.DTOs.Validators
{
    public class StockDtoValidator : AbstractValidator<StockDto>
    {
        public StockDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("O produto é obrigatório");

            RuleFor(x => x.Quantity)
                .GreaterThanOrEqualTo(0).WithMessage("A quantidade deve ser maior ou igual a 0");

            RuleFor(x => x.ReservedQuantity)
                .GreaterThanOrEqualTo(0).When(x => x.ReservedQuantity.HasValue)
                .WithMessage("A quantidade reservada deve ser maior ou igual a 0");

            RuleFor(x => x.ReservedQuantity)
                .LessThanOrEqualTo(x => x.Quantity).When(x => x.ReservedQuantity.HasValue)
                .WithMessage("A quantidade reservada não pode ser maior que a quantidade em estoque");
        }
    }
}