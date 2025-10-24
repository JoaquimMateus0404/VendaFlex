using FluentValidation;


namespace VendaFlex.Core.DTOs.Validators
{
    /// <summary>
    /// Validador para PrivilegeDto usando FluentValidation.
    /// Define regras de validação para privilégios do sistema.
    /// </summary>
    public class PrivilegeDtoValidator : AbstractValidator<PrivilegeDto>
    {
        public PrivilegeDtoValidator()
        {
            // Validação de Nome
            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("O nome do privilégio é obrigatório.")
                .MinimumLength(3).WithMessage("O nome deve ter pelo menos 3 caracteres.")
                .MaximumLength(100).WithMessage("O nome não pode exceder 100 caracteres.");

            // Validação de Descrição
            RuleFor(p => p.Description)
                .MaximumLength(500).WithMessage("A descrição não pode exceder 500 caracteres.")
                .When(p => !string.IsNullOrWhiteSpace(p.Description));

            // Validação de Código (deve ser único e seguir padrão)
            RuleFor(p => p.Code)
                .NotEmpty().WithMessage("O código do privilégio é obrigatório.")
                .MaximumLength(50).WithMessage("O código não pode exceder 50 caracteres.")
                .Matches(@"^[A-Z_]+$").WithMessage("O código deve conter apenas letras maiúsculas e underscores (ex: USER_CREATE).");
        }
    }
}
