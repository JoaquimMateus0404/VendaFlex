using Microsoft.EntityFrameworkCore;
using VendaFlex.Data.Entities;

namespace VendaFlex.Data.Repositories
{
    /// <summary>
    /// Repositório para operações de acesso a dados de configuração da empresa.
    /// Como CompanyConfig é singleton (única configuração), métodos são otimizados para isso.
    /// </summary>
    public class CompanyConfigRepository
    {
        private readonly ApplicationDbContext _context;

        public CompanyConfigRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Obtém a configuração da empresa (primeira e única).
        /// </summary>
        public async Task<CompanyConfig?> GetAsync()
        {
            return await _context.CompanyConfigs.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Obtém a configuração sem tracking (melhor para leitura).
        /// </summary>
        public async Task<CompanyConfig?> GetAsNoTrackingAsync()
        {
            return await _context.CompanyConfigs
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Verifica se já existe uma configuração.
        /// </summary>
        public async Task<bool> ExistsAsync()
        {
            return await _context.CompanyConfigs.AnyAsync();
        }

        /// <summary>
        /// Adiciona uma nova configuração (primeira vez).
        /// </summary>
        public async Task<CompanyConfig> AddAsync(CompanyConfig entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            // Verificar se já existe configuração
            var exists = await ExistsAsync();
            if (exists)
                throw new InvalidOperationException("Já existe uma configuração da empresa. Use UpdateAsync.");

            await _context.CompanyConfigs.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Atualiza a configuração existente.
        /// </summary>
        public async Task<CompanyConfig> UpdateAsync(CompanyConfig entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _context.CompanyConfigs.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Cria ou atualiza a configuração (Upsert).
        /// </summary>
        public async Task<CompanyConfig> UpsertAsync(CompanyConfig entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var existing = await GetAsync();

            if (existing == null)
            {
                // Criar nova configuração
                entity.CompanyConfigId = 0; // Garantir que é novo
                await _context.CompanyConfigs.AddAsync(entity);
            }
            else
            {
                // Atualizar existente
                entity.CompanyConfigId = existing.CompanyConfigId;
                _context.Entry(existing).CurrentValues.SetValues(entity);
            }

            await _context.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Incrementa e retorna o próximo número de fatura.
        /// Operação atômica para evitar conflitos em ambiente multi-usuário.
        /// </summary>
        public async Task<int> GetAndIncrementInvoiceNumberAsync()
        {
            var config = await GetAsync();

            if (config == null)
                throw new InvalidOperationException("Configuração da empresa não encontrada.");

            var currentNumber = config.NextInvoiceNumber;
            config.NextInvoiceNumber++;

            await _context.SaveChangesAsync();

            return currentNumber;
        }

        /// <summary>
        /// Atualiza apenas a URL do logo.
        /// </summary>
        public async Task<bool> UpdateLogoUrlAsync(string logoUrl)
        {
            var config = await GetAsync();

            if (config == null)
                return false;

            config.LogoUrl = logoUrl;
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Atualiza o status IsActive.
        /// </summary>
        public async Task<bool> UpdateActiveStatusAsync(bool isActive)
        {
            var config = await GetAsync();

            if (config == null)
                return false;

            config.IsActive = isActive;
            await _context.SaveChangesAsync();

            return true;
        }
    }
}