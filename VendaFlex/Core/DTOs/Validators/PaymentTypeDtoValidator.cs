using FluentValidation;

namespace VendaFlex.Core.DTOs.Validators
{
    public class PaymentTypeDtoValidator : AbstractValidator<PaymentTypeDto>
    {
        public PaymentTypeDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Nome � obrigat�rio.")
                .MaximumLength(100).WithMessage("Nome deve ter no m�ximo 100 caracteres.");

            RuleFor(x => x.Description)
                .MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Description));
        }
    }
}
