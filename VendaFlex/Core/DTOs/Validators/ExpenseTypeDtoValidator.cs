using FluentValidation;

namespace VendaFlex.Core.DTOs.Validators
{
    /// <summary>
    /// Validador para ExpenseTypeDto.
    /// </summary>
    public class ExpenseTypeDtoValidator : AbstractValidator<ExpenseTypeDto>
    {
        public ExpenseTypeDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("O nome é obrigatório.")
                .MaximumLength(100).WithMessage("O nome deve ter no máximo 100 caracteres.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("A descrição deve ter no máximo 500 caracteres.");
        }
    }
}

