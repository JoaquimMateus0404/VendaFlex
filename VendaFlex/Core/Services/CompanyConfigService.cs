using AutoMapper;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.Data.Entities;
using VendaFlex.Data.Repositories;

namespace VendaFlex.Core.Services
{
    public class CompanyConfigService : ICompanyConfigService
    {
        private readonly IRepository<CompanyConfig> _repo;
        private readonly IMapper _mapper;
        public CompanyConfigService(IRepository<CompanyConfig> repo, IMapper mapper) { _repo = repo; _mapper = mapper; }

        public async Task<CompanyConfigDto> GetAsync()
        {
            var all = await _repo.GetAllAsync();
            var cfg = all.FirstOrDefault();
            return _mapper.Map<CompanyConfigDto>(cfg);
        }

        public async Task<CompanyConfigDto> UpdateAsync(CompanyConfigDto dto)
        {
            // Upsert: se não existir config, criar; caso contrário, atualizar
            var existing = (await _repo.GetAllAsync()).FirstOrDefault();
            if (existing == null || dto.CompanyConfigId == 0)
            {
                var entityToAdd = _mapper.Map<CompanyConfig>(dto);
                // garantir defaults
                entityToAdd.CompanyConfigId = 0;
                var created = await _repo.AddAsync(entityToAdd);
                return _mapper.Map<CompanyConfigDto>(created);
            }
            else
            {
                var entity = _mapper.Map<CompanyConfig>(dto);
                if (entity.CompanyConfigId == 0)
                {
                    entity.CompanyConfigId = existing.CompanyConfigId;
                }
                var updated = await _repo.UpdateAsync(entity);
                return _mapper.Map<CompanyConfigDto>(updated);
            }
        }

        public async Task<int> GetNextInvoiceNumberAsync()
        {
            var all = await _repo.GetAllAsync();
            var cfg = all.FirstOrDefault();
            if (cfg == null) return 1;
            cfg.NextInvoiceNumber++;
            await _repo.UpdateAsync(cfg);
            return cfg.NextInvoiceNumber;
        }
    }
}
