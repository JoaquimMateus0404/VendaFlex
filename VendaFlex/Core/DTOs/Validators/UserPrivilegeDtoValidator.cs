using FluentValidation;

namespace VendaFlex.Core.DTOs.Validators
{
    /// <summary>
    /// Validador para UserPrivilegeDto usando FluentValidation.
    /// Define regras de validação para associação de privilégios a usuários.
    /// </summary>
    public class UserPrivilegeDtoValidator : AbstractValidator<UserPrivilegeDto>
    {
        public UserPrivilegeDtoValidator()
        {
            // Validação de UserId
            RuleFor(up => up.UserId)
                .GreaterThan(0).WithMessage("ID do usuário deve ser maior que zero.");

            // Validação de PrivilegeId
            RuleFor(up => up.PrivilegeId)
                .GreaterThan(0).WithMessage("ID do privilégio deve ser maior que zero.");

            // Validação de GrantedAt
            RuleFor(up => up.GrantedAt)
                .NotEmpty().WithMessage("Data de concessão é obrigatória.")
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Data de concessão não pode ser futura.");

            // Validação de GrantedByUserId (opcional)
            RuleFor(up => up.GrantedByUserId)
                .GreaterThan(0).WithMessage("ID do usuário que concedeu deve ser maior que zero.")
                .When(up => up.GrantedByUserId.HasValue);
        }
    }
}
