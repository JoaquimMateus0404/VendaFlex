using AutoMapper;
using FluentValidation;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.Core.Utils;
using VendaFlex.Data.Entities;
using VendaFlex.Data.Repositories;

namespace VendaFlex.Core.Services
{
    /// <summary>
    /// Serviço de gerenciamento de configuração da empresa.
    /// Responsável por lógica de negócio relacionada às configurações globais do sistema.
    /// USA APENAS O REPOSITÓRIO - não acessa DbContext diretamente.
    /// </summary>
    public class CompanyConfigService : ICompanyConfigService
    {
        private readonly CompanyConfigRepository _repository;
        private readonly IMapper _mapper;
        private readonly IValidator<CompanyConfigDto> _validator;

        public CompanyConfigService(
            CompanyConfigRepository repository,
            IMapper mapper,
            IValidator<CompanyConfigDto> validator)
        {
            _repository = repository;
            _mapper = mapper;
            _validator = validator;
        }

        /// <summary>
        /// Obtém a configuração atual da empresa.
        /// </summary>
        public async Task<OperationResult<CompanyConfigDto>> GetAsync()
        {
            try
            {
                var entity = await _repository.GetAsNoTrackingAsync();

                if (entity == null)
                {
                    // Retornar configuração padrão se não existir
                    var defaultConfig = CreateDefaultConfig();
                    return OperationResult<CompanyConfigDto>.CreateSuccess(
                        defaultConfig,
                        "Configuração padrão retornada. Configure a empresa para personalizar.");
                }

                var dto = _mapper.Map<CompanyConfigDto>(entity);
                return OperationResult<CompanyConfigDto>.CreateSuccess(
                    dto,
                    "Configuração carregada com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<CompanyConfigDto>.CreateFailure(
                    "Erro ao carregar configuração.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Atualiza ou cria a configuração da empresa (Upsert).
        /// </summary>
        public async Task<OperationResult<CompanyConfigDto>> UpdateAsync(CompanyConfigDto dto)
        {
            try
            {
                // Validar DTO usando validator injetado
                var validationResult = await _validator.ValidateAsync(dto);
                if (!validationResult.IsValid)
                {
                    return OperationResult<CompanyConfigDto>.CreateFailure(
                        "Dados inválidos.",
                        validationResult.Errors.Select(e => e.ErrorMessage));
                }

                // Converter DTO para entidade
                var entity = _mapper.Map<CompanyConfig>(dto);

                // Upsert através do repositório
                var saved = await _repository.UpsertAsync(entity);
                var resultDto = _mapper.Map<CompanyConfigDto>(saved);

                return OperationResult<CompanyConfigDto>.CreateSuccess(
                    resultDto,
                    "Configuração salva com sucesso.");
            }
            catch (InvalidOperationException ex)
            {
                return OperationResult<CompanyConfigDto>.CreateFailure(
                    "Erro de operação.",
                    new[] { ex.Message });
            }
            catch (Exception ex)
            {
                return OperationResult<CompanyConfigDto>.CreateFailure(
                    "Erro ao salvar configuração.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Verifica se a configuração inicial já foi realizada.
        /// </summary>
        public async Task<bool> IsConfiguredAsync()
        {
            try
            {
                return await _repository.ExistsAsync();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Obtém e incrementa o próximo número de fatura.
        /// </summary>
        public async Task<OperationResult<int>> GetNextInvoiceNumberAsync()
        {
            try
            {
                var nextNumber = await _repository.GetAndIncrementInvoiceNumberAsync();

                return OperationResult<int>.CreateSuccess(
                    nextNumber,
                    $"Próximo número de fatura: {nextNumber}");
            }
            catch (InvalidOperationException ex)
            {
                return OperationResult<int>.CreateFailure(
                    "Configuração não encontrada.",
                    new[] { ex.Message });
            }
            catch (Exception ex)
            {
                return OperationResult<int>.CreateFailure(
                    "Erro ao obter próximo número de fatura.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Gera o número completo da fatura (Prefixo + Número).
        /// </summary>
        public async Task<OperationResult<string>> GenerateInvoiceNumberAsync()
        {
            try
            {
                var config = await _repository.GetAsync();

                if (config == null)
                    return OperationResult<string>.CreateFailure("Configuração não encontrada.");

                var nextNumberResult = await GetNextInvoiceNumberAsync();

                if (!nextNumberResult.Success)
                    return OperationResult<string>.CreateFailure(nextNumberResult.Message);

                // Formato: PREFIXO-00001
                var invoiceNumber = $"{config.InvoicePrefix}-{nextNumberResult.Data:D5}";

                return OperationResult<string>.CreateSuccess(
                    invoiceNumber,
                    $"Número da fatura gerado: {invoiceNumber}");
            }
            catch (Exception ex)
            {
                return OperationResult<string>.CreateFailure(
                    "Erro ao gerar número da fatura.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Atualiza apenas a URL do logo.
        /// </summary>
        public async Task<OperationResult> UpdateLogoAsync(string logoUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(logoUrl))
                    return OperationResult.CreateFailure("URL do logo é obrigatória.");

                var updated = await _repository.UpdateLogoUrlAsync(logoUrl);

                return updated
                    ? OperationResult.CreateSuccess("Logo atualizado com sucesso.")
                    : OperationResult.CreateFailure("Configuração não encontrada.");
            }
            catch (Exception ex)
            {
                return OperationResult.CreateFailure(
                    "Erro ao atualizar logo.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Remove o logo da empresa.
        /// </summary>
        public async Task<OperationResult> RemoveLogoAsync()
        {
            try
            {
                var updated = await _repository.UpdateLogoUrlAsync(string.Empty);

                return updated
                    ? OperationResult.CreateSuccess("Logo removido com sucesso.")
                    : OperationResult.CreateFailure("Configuração não encontrada.");
            }
            catch (Exception ex)
            {
                return OperationResult.CreateFailure(
                    "Erro ao remover logo.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Ativa a configuração da empresa.
        /// </summary>
        public async Task<OperationResult> ActivateAsync()
        {
            try
            {
                var updated = await _repository.UpdateActiveStatusAsync(true);

                return updated
                    ? OperationResult.CreateSuccess("Configuração ativada com sucesso.")
                    : OperationResult.CreateFailure("Configuração não encontrada.");
            }
            catch (Exception ex)
            {
                return OperationResult.CreateFailure(
                    "Erro ao ativar configuração.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Desativa a configuração da empresa.
        /// </summary>
        public async Task<OperationResult> DeactivateAsync()
        {
            try
            {
                var updated = await _repository.UpdateActiveStatusAsync(false);

                return updated
                    ? OperationResult.CreateSuccess("Configuração desativada com sucesso.")
                    : OperationResult.CreateFailure("Configuração não encontrada.");
            }
            catch (Exception ex)
            {
                return OperationResult.CreateFailure(
                    "Erro ao desativar configuração.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Cria uma configuração padrão.
        /// </summary>
        private CompanyConfigDto CreateDefaultConfig()
        {
            return new CompanyConfigDto
            {
                CompanyConfigId = 0,
                CompanyName = "Minha Empresa",
                Currency = "AOA",
                CurrencySymbol = "Kz",
                DefaultTaxRate = 0,
                InvoicePrefix = "INV",
                NextInvoiceNumber = 1,
                InvoiceFormat = 0, // A4
                IncludeCustomerData = true,
                AllowAnonymousInvoice = false,
                IsActive = true
            };
        }
    }
}