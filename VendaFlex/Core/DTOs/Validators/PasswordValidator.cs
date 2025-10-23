using FluentValidation;
using VendaFlex.Data.Entities;

namespace VendaFlex.Core.DTOs.Validators
{
    /// <summary>
    /// Validador específico para senha durante registro/alteração.
    /// Separa a validação de senha do DTO principal.
    /// </summary>
    public class PasswordValidator : AbstractValidator<string>
    {
        public PasswordValidator()
        {
            RuleFor(password => password)
                .NotEmpty().WithMessage("A senha é obrigatória.")
                .MinimumLength(8).WithMessage("A senha deve ter pelo menos 8 caracteres.")
                .Matches("[A-Z]").WithMessage("A senha deve conter pelo menos uma letra maiúscula.")
                .Matches("[a-z]").WithMessage("A senha deve conter pelo menos uma letra minúscula.")
                .Matches("[0-9]").WithMessage("A senha deve conter pelo menos um número.")
                .Must(BeValidPassword).WithMessage("A senha não atende aos requisitos mínimos de segurança.");
        }

        /// <summary>
        /// Valida força da senha usando regra de negócio da entidade User.
        /// </summary>
        private bool BeValidPassword(string password)
        {
            return User.ValidatePasswordStrength(password);
        }
    }
}