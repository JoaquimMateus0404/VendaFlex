using FluentValidation;

namespace VendaFlex.Core.DTOs.Validators
{
    public class PaymentTypeDtoValidator : AbstractValidator<PaymentTypeDto>
    {
        public PaymentTypeDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Nome é obrigatório.")
                .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres.");

            RuleFor(x => x.Description)
                .MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Description));
        }
    }
}
