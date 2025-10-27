using FluentValidation;

namespace VendaFlex.Core.DTOs.Validators
{
    public class ProductDtoValidator : AbstractValidator<ProductDto>
    {
        public ProductDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("O nome é obrigatório")
                .MaximumLength(200).WithMessage("O nome deve ter no máximo 200 caracteres");

            RuleFor(x => x.Barcode)
                .MaximumLength(100).WithMessage("O código de barras deve ter no máximo 100 caracteres");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("A descrição deve ter no máximo 1000 caracteres");

            RuleFor(x => x.ShortDescription)
                .MaximumLength(500).WithMessage("A descrição curta deve ter no máximo 500 caracteres");

            RuleFor(x => x.SKU)
                .MaximumLength(50).WithMessage("O SKU deve ter no máximo 50 caracteres");

            RuleFor(x => x.Weight)
                .MaximumLength(50).WithMessage("O peso deve ter no máximo 50 caracteres");

            RuleFor(x => x.Dimensions)
                .MaximumLength(100).WithMessage("As dimensões devem ter no máximo 100 caracteres");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("A categoria é obrigatória");

            RuleFor(x => x.SupplierId)
                .GreaterThan(0).WithMessage("O fornecedor é obrigatório");

            RuleFor(x => x.SalePrice)
                .GreaterThanOrEqualTo(0).WithMessage("O preço de venda deve ser maior ou igual a 0");

            RuleFor(x => x.CostPrice)
                .GreaterThanOrEqualTo(0).WithMessage("O preço de custo deve ser maior ou igual a 0");

            RuleFor(x => x.SalePrice)
                .GreaterThanOrEqualTo(x => x.CostPrice)
                .When(x => x.SalePrice > 0 && x.CostPrice > 0)
                .WithMessage("O preço de venda deve ser maior ou igual ao preço de custo");

            RuleFor(x => x.DiscountPercentage)
                .InclusiveBetween(0, 100).When(x => x.DiscountPercentage.HasValue)
                .WithMessage("A porcentagem de desconto deve estar entre 0 e 100")
                .WithMessage("A porcentagem de desconto deve ter no máximo 2 casas decimais");

            RuleFor(x => x.TaxRate)
                .InclusiveBetween(0, 100).When(x => x.TaxRate.HasValue)
                .WithMessage("A taxa de imposto deve estar entre 0 e 100")
                .WithMessage("A taxa de imposto deve ter no máximo 2 casas decimais");

            RuleFor(x => x.PhotoUrl)
                .MaximumLength(500).WithMessage("A URL da foto deve ter no máximo 500 caracteres")
                .Must(BeAValidUrl).When(x => !string.IsNullOrEmpty(x.PhotoUrl))
                .WithMessage("URL inválida");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Status inválido");

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("A ordem de exibição deve ser maior ou igual a 0");

            RuleFor(x => x.MinimumStock)
                .GreaterThanOrEqualTo(0).When(x => x.MinimumStock.HasValue)
                .WithMessage("O estoque mínimo deve ser maior ou igual a 0");

            RuleFor(x => x.MaximumStock)
                .GreaterThanOrEqualTo(0).When(x => x.MaximumStock.HasValue)
                .WithMessage("O estoque máximo deve ser maior ou igual a 0")
                .GreaterThanOrEqualTo(x => x.MinimumStock.GetValueOrDefault())
                .When(x => x.MaximumStock.HasValue && x.MinimumStock.HasValue)
                .WithMessage("O estoque máximo deve ser maior ou igual ao estoque mínimo");

            RuleFor(x => x.ReorderPoint)
                .GreaterThanOrEqualTo(0).When(x => x.ReorderPoint.HasValue)
                .WithMessage("O ponto de reposição deve ser maior ou igual a 0");

            RuleFor(x => x.ExpirationDays)
                .GreaterThan(0).When(x => x.HasExpirationDate && x.ExpirationDays.HasValue)
                .WithMessage("Os dias de validade devem ser maiores que 0");

            RuleFor(x => x.ExpirationWarningDays)
                .GreaterThan(0).When(x => x.HasExpirationDate && x.ExpirationWarningDays.HasValue)
                .WithMessage("Os dias de aviso de validade devem ser maiores que 0")
                .LessThanOrEqualTo(x => x.ExpirationDays.GetValueOrDefault())
                .When(x => x.HasExpirationDate && x.ExpirationWarningDays.HasValue && x.ExpirationDays.HasValue)
                .WithMessage("Os dias de aviso devem ser menores ou iguais aos dias de validade");
        }

        private bool BeAValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }
    }

}
