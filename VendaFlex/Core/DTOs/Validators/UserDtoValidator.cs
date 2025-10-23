using FluentValidation;
using VendaFlex.Data.Entities;

namespace VendaFlex.Core.DTOs.Validators
{
    /// <summary>
    /// Validador para UserDto usando FluentValidation.
    /// Define regras de validação para criação e atualização de usuários.
    /// </summary>
    public class UserDtoValidator : AbstractValidator<UserDto>
    {
        public UserDtoValidator()
        {
            // Validação de Username
            RuleFor(user => user.Username)
                .NotEmpty().WithMessage("O nome de usuário é obrigatório.")
                .MinimumLength(3).WithMessage("O nome de usuário deve ter pelo menos 3 caracteres.")
                .MaximumLength(100).WithMessage("O nome de usuário não pode exceder 100 caracteres.")
                .Must(BeValidUsername).WithMessage("O nome de usuário pode conter apenas letras, números, pontos, hífens e underscores.");

            // Validação de PersonId
            RuleFor(user => user.PersonId)
                .GreaterThan(0).WithMessage("O ID da pessoa deve ser maior que zero.");

            // Validação de Status
            RuleFor(user => user.Status)
                .IsInEnum().WithMessage("O status do usuário é inválido.");
        }

        /// <summary>
        /// Valida se o username contém apenas caracteres permitidos.
        /// </summary>
        private bool BeValidUsername(string username)
        {
            return User.ValidateUsername(username);
        }
    }
}