using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendaFlex.Core.DTOs.Validators
{
    public class CategoryDtoValidator : AbstractValidator<CategoryDto>
    {
        public CategoryDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("O nome é obrigatório")
                .MaximumLength(100).WithMessage("O nome deve ter no máximo 100 caracteres");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("A descrição deve ter no máximo 500 caracteres");

            RuleFor(x => x.Code)
                .MaximumLength(50).WithMessage("O código deve ter no máximo 50 caracteres");

            RuleFor(x => x.ImageUrl)
                .MaximumLength(500).WithMessage("A URL da imagem deve ter no máximo 500 caracteres")
                .Must(BeAValidUrl).When(x => !string.IsNullOrEmpty(x.ImageUrl))
                .WithMessage("URL inválida");

            RuleFor(x => x.ParentCategoryId)
                .NotEqual(x => x.CategoryId).When(x => x.ParentCategoryId.HasValue)
                .WithMessage("A categoria não pode ser sua própria categoria pai");

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("A ordem de exibição deve ser maior ou igual a 0");
        }

        private bool BeAValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }
    }
}

