using FluentValidation;

namespace VendaFlex.Core.DTOs.Validators
{
    public class InvoiceProductDtoValidator : AbstractValidator<InvoiceProductDto>
    {
        public InvoiceProductDtoValidator()
        {
            RuleFor(x => x.InvoiceId)
                .GreaterThan(0).WithMessage("Fatura � obrigat�ria.");

            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("Produto � obrigat�rio.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantidade deve ser maior que 0.");

            RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Pre�o unit�rio n�o pode ser negativo.");

            RuleFor(x => x.DiscountPercentage)
                .InclusiveBetween(0, 100).WithMessage("Desconto deve estar entre 0 e 100%.");

            RuleFor(x => x.TaxRate)
                .GreaterThanOrEqualTo(0).WithMessage("Taxa deve ser maior ou igual a 0.");
        }
    }
}
