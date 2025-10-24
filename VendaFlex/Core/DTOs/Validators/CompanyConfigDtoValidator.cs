using FluentValidation;

namespace VendaFlex.Core.DTOs.Validators
{
    /// <summary>
    /// Validador para CompanyConfigDto usando FluentValidation.
    /// Define regras de validação para configurações da empresa.
    /// </summary>
    public class CompanyConfigDtoValidator : AbstractValidator<CompanyConfigDto>
    {
        public CompanyConfigDtoValidator()
        {
            // Validação de Nome da Empresa
            RuleFor(c => c.CompanyName)
                .NotEmpty().WithMessage("O nome da empresa é obrigatório.")
                .MinimumLength(3).WithMessage("O nome da empresa deve ter pelo menos 3 caracteres.")
                .MaximumLength(200).WithMessage("O nome da empresa não pode exceder 200 caracteres.");

            // Validação de Tipo de Indústria
            RuleFor(c => c.IndustryType)
                .MaximumLength(100).WithMessage("Tipo de indústria não pode exceder 100 caracteres.")
                .When(c => !string.IsNullOrWhiteSpace(c.IndustryType));

            // Validação de Regime Fiscal
            RuleFor(c => c.TaxRegime)
                .MaximumLength(100).WithMessage("Regime fiscal não pode exceder 100 caracteres.")
                .When(c => !string.IsNullOrWhiteSpace(c.TaxRegime));

            // Validação de TaxId (NIF/CNPJ)
            RuleFor(c => c.TaxId)
                .NotEmpty().WithMessage("O documento fiscal (NIF/CNPJ) é obrigatório.")
                .MaximumLength(50).WithMessage("Documento fiscal não pode exceder 50 caracteres.");

            // Validação de Email
            RuleFor(c => c.Email)
                .NotEmpty().WithMessage("O email é obrigatório.")
                .EmailAddress().WithMessage("Email inválido.")
                .MaximumLength(200).WithMessage("Email não pode exceder 200 caracteres.");

            // Validação de Telefone
            RuleFor(c => c.PhoneNumber)
                .Matches(@"^[\d\s\-\+\(\)]+$").WithMessage("Telefone contém caracteres inválidos.")
                .MaximumLength(200).WithMessage("Telefone não pode exceder 200 caracteres.")
                .When(c => !string.IsNullOrWhiteSpace(c.PhoneNumber));

            // Validação de Website
            RuleFor(c => c.Website)
                .Must(BeValidUrl).WithMessage("Website inválido.")
                .MaximumLength(200).WithMessage("Website não pode exceder 200 caracteres.")
                .When(c => !string.IsNullOrWhiteSpace(c.Website));

            // Validação de Endereço
            RuleFor(c => c.Address)
                .MaximumLength(500).WithMessage("Endereço não pode exceder 500 caracteres.")
                .When(c => !string.IsNullOrWhiteSpace(c.Address));

            // Validação de Cidade
            RuleFor(c => c.City)
                .MaximumLength(100).WithMessage("Cidade não pode exceder 100 caracteres.")
                .When(c => !string.IsNullOrWhiteSpace(c.City));

            // Validação de Código Postal
            RuleFor(c => c.PostalCode)
                .MaximumLength(20).WithMessage("Código postal não pode exceder 20 caracteres.")
                .When(c => !string.IsNullOrWhiteSpace(c.PostalCode));

            // Validação de País
            RuleFor(c => c.Country)
                .MaximumLength(100).WithMessage("País não pode exceder 100 caracteres.")
                .When(c => !string.IsNullOrWhiteSpace(c.Country));

            // Validação de LogoUrl
            RuleFor(c => c.LogoUrl)
                .MaximumLength(500).WithMessage("URL do logo não pode exceder 500 caracteres.")
                .When(c => !string.IsNullOrWhiteSpace(c.LogoUrl));

            // Validação de Moeda
            RuleFor(c => c.Currency)
                .NotEmpty().WithMessage("A moeda é obrigatória.")
                .MaximumLength(50).WithMessage("Moeda não pode exceder 50 caracteres.");

            // Validação de Símbolo da Moeda
            RuleFor(c => c.CurrencySymbol)
                .NotEmpty().WithMessage("O símbolo da moeda é obrigatório.")
                .MaximumLength(10).WithMessage("Símbolo da moeda não pode exceder 10 caracteres.");

            // Validação de Taxa de Imposto Padrão
            RuleFor(c => c.DefaultTaxRate)
                .GreaterThanOrEqualTo(0).WithMessage("Taxa de imposto não pode ser negativa.")
                .LessThanOrEqualTo(100).WithMessage("Taxa de imposto não pode exceder 100%.");

            // Validação de Texto de Rodapé da Fatura
            RuleFor(c => c.InvoiceFooterText)
                .MaximumLength(1000).WithMessage("Texto de rodapé não pode exceder 1000 caracteres.")
                .When(c => !string.IsNullOrWhiteSpace(c.InvoiceFooterText));

            // Validação de Prefixo da Fatura
            RuleFor(c => c.InvoicePrefix)
                .NotEmpty().WithMessage("O prefixo da fatura é obrigatório.")
                .MaximumLength(100).WithMessage("Prefixo da fatura não pode exceder 100 caracteres.")
                .Matches(@"^[A-Z0-9\-]+$").WithMessage("Prefixo deve conter apenas letras maiúsculas, números e hífens.");

            // Validação de Próximo Número da Fatura
            RuleFor(c => c.NextInvoiceNumber)
                .GreaterThan(0).WithMessage("Próximo número da fatura deve ser maior que zero.");

            // Validação de Formato da Fatura
            RuleFor(c => c.InvoiceFormat)
                .IsInEnum().WithMessage("Formato de fatura inválido.");

            // Validação de Horário de Funcionamento
            RuleFor(c => c.BusinessHours)
                .MaximumLength(100).WithMessage("Horário de funcionamento não pode exceder 100 caracteres.")
                .When(c => !string.IsNullOrWhiteSpace(c.BusinessHours));
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