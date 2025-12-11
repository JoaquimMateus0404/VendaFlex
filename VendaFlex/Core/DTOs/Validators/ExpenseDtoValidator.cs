using FluentValidation;
using System;

namespace VendaFlex.Core.DTOs.Validators
{
    /// <summary>
    /// Validador para ExpenseDto.
    /// </summary>
    public class ExpenseDtoValidator : AbstractValidator<ExpenseDto>
    {
        public ExpenseDtoValidator()
        {
            RuleFor(x => x.ExpenseTypeId)
                .GreaterThan(0).WithMessage("O tipo de despesa é obrigatório.");

            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("O usuário é obrigatório.");

            RuleFor(x => x.Date)
                .NotEmpty().WithMessage("A data é obrigatória.")
                .LessThanOrEqualTo(DateTime.Now.AddDays(1)).WithMessage("A data não pode ser futura.");

            RuleFor(x => x.Value)
                .GreaterThan(0).WithMessage("O valor deve ser maior que zero.")
                .LessThanOrEqualTo(999999999.99m).WithMessage("O valor é muito alto.");

            RuleFor(x => x.Title)
                .MaximumLength(200).WithMessage("O título deve ter no máximo 200 caracteres.");

            RuleFor(x => x.Notes)
                .MaximumLength(1000).WithMessage("As notas devem ter no máximo 1000 caracteres.");

            RuleFor(x => x.Reference)
                .MaximumLength(100).WithMessage("A referência deve ter no máximo 100 caracteres.");

            RuleFor(x => x.AttachmentUrl)
                .MaximumLength(500).WithMessage("A URL do anexo deve ter no máximo 500 caracteres.")
                .Must(BeAValidUrl).When(x => !string.IsNullOrEmpty(x.AttachmentUrl))
                .WithMessage("URL do anexo inválida.");

            RuleFor(x => x.PaidDate)
                .LessThanOrEqualTo(DateTime.Now.AddDays(1)).When(x => x.PaidDate.HasValue)
                .WithMessage("A data de pagamento não pode ser futura.")
                .GreaterThanOrEqualTo(x => x.Date).When(x => x.PaidDate.HasValue)
                .WithMessage("A data de pagamento não pode ser anterior à data da despesa.");

            RuleFor(x => x.IsPaid)
                .Must((dto, isPaid) => !isPaid || dto.PaidDate.HasValue)
                .WithMessage("A data de pagamento é obrigatória quando a despesa está marcada como paga.");
        }

        private bool BeAValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }
    }
}

