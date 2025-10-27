using FluentValidation;

namespace VendaFlex.Core.DTOs.Validators
{
    /// <summary>
    /// Validador para PersonDto usando FluentValidation.
    /// Define regras de validação para criação e atualização de pessoas.
    /// </summary>
    public class PersonDtoValidator : AbstractValidator<PersonDto>
    {
        public PersonDtoValidator()
        {
            // Validação de Nome
            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("O nome é obrigatório.")
                .MinimumLength(3).WithMessage("O nome deve ter pelo menos 3 caracteres.")
                .MaximumLength(200).WithMessage("O nome não pode exceder 200 caracteres.");

            // Validação de Tipo
            RuleFor(p => p.Type)
                .IsInEnum().WithMessage("O tipo de pessoa é inválido.");

            // Validação de Email (quando fornecido)
            RuleFor(p => p.Email)
                .EmailAddress().WithMessage("Email inválido.")
                .MaximumLength(200).WithMessage("Email não pode exceder 200 caracteres.")
                .When(p => !string.IsNullOrWhiteSpace(p.Email));

            // Validação de TaxId (quando fornecido)
            RuleFor(p => p.TaxId)
                .MaximumLength(50).WithMessage("Documento fiscal não pode exceder 50 caracteres.")
                .When(p => !string.IsNullOrWhiteSpace(p.TaxId));

            // Validação de IdentificationNumber (quando fornecido)
            RuleFor(p => p.IdentificationNumber)
                .MaximumLength(50).WithMessage("Número de identificação não pode exceder 50 caracteres.")
                .When(p => !string.IsNullOrWhiteSpace(p.IdentificationNumber));

            // Validação de PhoneNumber (quando fornecido)
            RuleFor(p => p.PhoneNumber)
                .Matches(@"^[\d\s\-\+\(\)]+$").WithMessage("Telefone contém caracteres inválidos.")
                .MaximumLength(20).WithMessage("Telefone não pode exceder 20 caracteres.")
                .When(p => !string.IsNullOrWhiteSpace(p.PhoneNumber));

            // Validação de MobileNumber (quando fornecido)
            RuleFor(p => p.MobileNumber)
                .Matches(@"^[\d\s\-\+\(\)]+$").WithMessage("Celular contém caracteres inválidos.")
                .MaximumLength(20).WithMessage("Celular não pode exceder 20 caracteres.")
                .When(p => !string.IsNullOrWhiteSpace(p.MobileNumber));

            // Validação de Website (quando fornecido)
            RuleFor(p => p.Website)
                .Must(BeValidUrl).WithMessage("Website inválido.")
                .MaximumLength(200).WithMessage("Website não pode exceder 200 caracteres.")
                .When(p => !string.IsNullOrWhiteSpace(p.Website));

            // Validação de Address
            RuleFor(p => p.Address)
                .MaximumLength(500).WithMessage("Endereço não pode exceder 500 caracteres.")
                .When(p => !string.IsNullOrWhiteSpace(p.Address));

            // Validação de City
            RuleFor(p => p.City)
                .MaximumLength(100).WithMessage("Cidade não pode exceder 100 caracteres.")
                .When(p => !string.IsNullOrWhiteSpace(p.City));

            // Validação de State
            RuleFor(p => p.State)
                .MaximumLength(100).WithMessage("Estado não pode exceder 100 caracteres.")
                .When(p => !string.IsNullOrWhiteSpace(p.State));

            // Validação de PostalCode
            RuleFor(p => p.PostalCode)
                .MaximumLength(20).WithMessage("Código postal não pode exceder 20 caracteres.")
                .When(p => !string.IsNullOrWhiteSpace(p.PostalCode));

            // Validação de Country
            RuleFor(p => p.Country)
                .MaximumLength(100).WithMessage("País não pode exceder 100 caracteres.")
                .When(p => !string.IsNullOrWhiteSpace(p.Country));

            // Validação de CreditLimit
            RuleFor(p => p.CreditLimit)
                .GreaterThanOrEqualTo(0).WithMessage("Limite de crédito não pode ser negativo.");

            // Validação de CurrentBalance
            RuleFor(p => p.CurrentBalance)
                .GreaterThanOrEqualTo(0).WithMessage("Saldo atual não pode ser negativo.");

            // Validação de ContactPerson
            RuleFor(p => p.ContactPerson)
                .MaximumLength(200).WithMessage("Pessoa de contato não pode exceder 200 caracteres.")
                .When(p => !string.IsNullOrWhiteSpace(p.ContactPerson));

            // Validação de Notes
            RuleFor(p => p.Notes)
                .MaximumLength(1000).WithMessage("Observações não podem exceder 1000 caracteres.")
                .When(p => !string.IsNullOrWhiteSpace(p.Notes));

            // Validação de ProfileImageUrl
            RuleFor(p => p.ProfileImageUrl)
                .MaximumLength(500).WithMessage("URL do logo não pode exceder 500 caracteres.")
                .When(c => !string.IsNullOrWhiteSpace(c.ProfileImageUrl));

            // Validação de Rating
            RuleFor(p => p.Rating)
                .InclusiveBetween(1, 5).WithMessage("Avaliação deve estar entre 1 e 5.")
                .When(p => p.Rating.HasValue);
        }

        /// <summary>
        /// Valida se a URL é válida.
        /// </summary>
        private bool BeValidUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return true;

            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}