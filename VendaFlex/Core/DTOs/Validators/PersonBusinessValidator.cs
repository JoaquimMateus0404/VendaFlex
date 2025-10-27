using FluentValidation;
using VendaFlex.Data.Entities;

namespace VendaFlex.Core.DTOs.Validators
{
    /// <summary>
    /// Validador específico para validações de negócio de Person.
    /// Validações que dependem do tipo de pessoa.
    /// </summary>
    public class PersonBusinessValidator : AbstractValidator<PersonDto>
    {
        public PersonBusinessValidator()
        {
            // Para Clientes (Customer ou Both), validar dados financeiros
            When(p => p.Type == PersonType.Customer || p.Type == PersonType.Both, () =>
            {
                RuleFor(p => p.Email)
                    .NotEmpty().WithMessage("Email é obrigatório para clientes.")
                    .EmailAddress().WithMessage("Email inválido.");

                RuleFor(p => p.PhoneNumber)
                    .NotEmpty().WithMessage("Telefone é obrigatório para clientes.")
                    .When(p => string.IsNullOrWhiteSpace(p.MobileNumber));

                RuleFor(p => p.MobileNumber)
                    .NotEmpty().WithMessage("Celular é obrigatório para clientes.")
                    .When(p => string.IsNullOrWhiteSpace(p.PhoneNumber));
            });

            // Para Fornecedores (Supplier ou Both), validar dados comerciais
            When(p => p.Type == PersonType.Supplier || p.Type == PersonType.Both, () =>
            {
                RuleFor(p => p.TaxId)
                    .NotEmpty().WithMessage("Documento fiscal é obrigatório para fornecedores.");

                RuleFor(p => p.Email)
                    .NotEmpty().WithMessage("Email é obrigatório para fornecedores.")
                    .EmailAddress().WithMessage("Email inválido.");
            });

            // Para Funcionários (Employee), validar identificação
            When(p => p.Type == PersonType.Employee, () =>
            {
                RuleFor(p => p.IdentificationNumber)
                    .NotEmpty().WithMessage("Número de identificação é obrigatório para funcionários.");

                RuleFor(p => p.Email)
                    .NotEmpty().WithMessage("Email é obrigatório para funcionários.")
                    .EmailAddress().WithMessage("Email inválido.");
            });
        }
    }
}