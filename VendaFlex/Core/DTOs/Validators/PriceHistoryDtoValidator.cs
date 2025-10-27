using FluentValidation;

namespace VendaFlex.Core.DTOs.Validators
{
    public class PriceHistoryDtoValidator : AbstractValidator<PriceHistoryDto>
    {
        public PriceHistoryDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("O produto é obrigatório");

            RuleFor(x => x.OldSalePrice)
                .GreaterThanOrEqualTo(0).WithMessage("O preço de venda antigo deve ser maior ou igual a 0");

            RuleFor(x => x.NewSalePrice)
                .GreaterThanOrEqualTo(0).WithMessage("O novo preço de venda deve ser maior ou igual a 0");

            RuleFor(x => x.OldCostPrice)
                .GreaterThanOrEqualTo(0).WithMessage("O preço de custo antigo deve ser maior ou igual a 0");

            RuleFor(x => x.NewCostPrice)
                .GreaterThanOrEqualTo(0).WithMessage("O novo preço de custo deve ser maior ou igual a 0");

            RuleFor(x => x.Reason)
                .MaximumLength(500).WithMessage("A razão deve ter no máximo 500 caracteres");

            RuleFor(x => x.ChangeDate)
                .NotEmpty().WithMessage("A data de alteração é obrigatória")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("A data de alteração não pode ser futura");

            RuleFor(x => x.NewSalePrice)
                .NotEqual(x => x.OldSalePrice).WithMessage("O novo preço de venda deve ser diferente do antigo");

            RuleFor(x => x.NewCostPrice)
                .NotEqual(x => x.OldCostPrice).WithMessage("O novo preço de custo deve ser diferente do antigo");
        }
    }
}
