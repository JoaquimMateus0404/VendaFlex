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
    /// Servi�o de gerenciamento de pessoas (clientes, fornecedores, funcion�rios).
    /// Respons�vel por l�gica de neg�cio relacionada a pessoas.
    /// USA APENAS O REPOSIT�RIO - n�o acessa DbContext diretamente.
    /// </summary>
    public class PersonService : IPersonService
    {
        private readonly PersonRepository _personRepository;
        private readonly IMapper _mapper;
        private readonly IValidator<PersonDto> _personValidator;

        public PersonService(
            PersonRepository personRepository,
            IMapper mapper,
            IValidator<PersonDto> personValidator)
        {
            _personRepository = personRepository;
            _mapper = mapper;
            _personValidator = personValidator;
        }

        #region CRUD Operations

        /// <summary>
        /// Busca uma pessoa por ID.
        /// </summary>
        public async Task<OperationResult<PersonDto>> GetByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return OperationResult<PersonDto>.CreateFailure("ID inv�lido.");

                var entity = await _personRepository.GetByIdAsync(id);

                if (entity == null)
                    return OperationResult<PersonDto>.CreateFailure("Pessoa n�o encontrada.");

                var dto = _mapper.Map<PersonDto>(entity);
                return OperationResult<PersonDto>.CreateSuccess(dto, "Pessoa encontrada com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<PersonDto>.CreateFailure(
                    "Erro ao buscar pessoa.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Retorna todas as pessoas cadastradas.
        /// </summary>
        public async Task<OperationResult<IEnumerable<PersonDto>>> GetAllAsync()
        {
            try
            {
                var entities = await _personRepository.GetAllAsync();
                var dtos = _mapper.Map<IEnumerable<PersonDto>>(entities);

                return OperationResult<IEnumerable<PersonDto>>.CreateSuccess(
                    dtos,
                    $"{dtos.Count()} pessoa(s) encontrada(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<PersonDto>>.CreateFailure(
                    "Erro ao listar pessoas.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Cria uma nova pessoa.
        /// </summary>
        public async Task<OperationResult<PersonDto>> CreateAsync(PersonDto dto)
        {
            try
            {
                // Validar DTO usando validator injetado
                var validationResult = await _personValidator.ValidateAsync(dto);
                if (!validationResult.IsValid)
                {
                    return OperationResult<PersonDto>.CreateFailure(
                        "Dados inv�lidos.",
                        validationResult.Errors.Select(e => e.ErrorMessage));
                }

                // Verificar duplica��o de email
                if (!string.IsNullOrWhiteSpace(dto.Email))
                {
                    var emailExists = await _personRepository.EmailExistsAsync(dto.Email);
                    if (emailExists)
                        return OperationResult<PersonDto>.CreateFailure("Email j� est� em uso.");
                }

                // Verificar duplica��o de documento fiscal
                if (!string.IsNullOrWhiteSpace(dto.TaxId))
                {
                    var taxIdExists = await _personRepository.TaxIdExistsAsync(dto.TaxId);
                    if (taxIdExists)
                        return OperationResult<PersonDto>.CreateFailure("Documento fiscal j� est� em uso.");
                }

                // Criar entidade
                var entity = _mapper.Map<Person>(dto);

                // Salvar atrav�s do reposit�rio
                var created = await _personRepository.AddAsync(entity);
                var resultDto = _mapper.Map<PersonDto>(created);

                return OperationResult<PersonDto>.CreateSuccess(
                    resultDto,
                    "Pessoa cadastrada com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<PersonDto>.CreateFailure(
                    "Erro ao cadastrar pessoa.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Atualiza dados de uma pessoa existente.
        /// </summary>
        public async Task<OperationResult<PersonDto>> UpdateAsync(PersonDto dto)
        {
            try
            {
                // Validar DTO usando validator injetado
                var validationResult = await _personValidator.ValidateAsync(dto);
                if (!validationResult.IsValid)
                {
                    return OperationResult<PersonDto>.CreateFailure(
                        "Dados inv�lidos.",
                        validationResult.Errors.Select(e => e.ErrorMessage));
                }

                // Verificar se pessoa existe
                var existingPerson = await _personRepository.GetByIdAsync(dto.PersonId);
                if (existingPerson == null)
                    return OperationResult<PersonDto>.CreateFailure("Pessoa n�o encontrada.");

                // Verificar duplica��o de email (se foi alterado)
                if (!string.IsNullOrWhiteSpace(dto.Email) && existingPerson.Email != dto.Email)
                {
                    var emailExists = await _personRepository.EmailExistsAsync(dto.Email, dto.PersonId);
                    if (emailExists)
                        return OperationResult<PersonDto>.CreateFailure("Email j� est� em uso.");
                }

                // Verificar duplica��o de documento fiscal (se foi alterado)
                if (!string.IsNullOrWhiteSpace(dto.TaxId) && existingPerson.TaxId != dto.TaxId)
                {
                    var taxIdExists = await _personRepository.TaxIdExistsAsync(dto.TaxId, dto.PersonId);
                    if (taxIdExists)
                        return OperationResult<PersonDto>.CreateFailure("Documento fiscal j� est� em uso.");
                }

                // Atualizar entidade
                _mapper.Map(dto, existingPerson);
                var updated = await _personRepository.UpdateAsync(existingPerson);
                var resultDto = _mapper.Map<PersonDto>(updated);

                return OperationResult<PersonDto>.CreateSuccess(
                    resultDto,
                    "Pessoa atualizada com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<PersonDto>.CreateFailure(
                    "Erro ao atualizar pessoa.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Remove uma pessoa do sistema.
        /// </summary>
        public async Task<OperationResult> DeleteAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return OperationResult.CreateFailure("ID inv�lido.");

                var person = await _personRepository.GetByIdAsync(id);
                if (person == null)
                    return OperationResult.CreateFailure("Pessoa n�o encontrada.");

                var deleted = await _personRepository.DeleteAsync(id);

                return deleted
                    ? OperationResult.CreateSuccess("Pessoa removida com sucesso.")
                    : OperationResult.CreateFailure("N�o foi poss�vel remover a pessoa.");
            }
            catch (Exception ex)
            {
                return OperationResult.CreateFailure(
                    "Erro ao remover pessoa.",
                    new[] { ex.Message });
            }
        }

        #endregion

        #region Pagination

        /// <summary>
        /// Retorna pessoas paginadas.
        /// </summary>
        public async Task<OperationResult<IEnumerable<PersonDto>>> GetPagedAsync(int page, int pageSize)
        {
            try
            {
                if (page < 1)
                    return OperationResult<IEnumerable<PersonDto>>.CreateFailure("P�gina deve ser maior ou igual a 1.");

                if (pageSize < 1 || pageSize > 100)
                    return OperationResult<IEnumerable<PersonDto>>.CreateFailure("Tamanho da p�gina deve estar entre 1 e 100.");

                var entities = await _personRepository.GetPagedAsync(page, pageSize);
                var dtos = _mapper.Map<IEnumerable<PersonDto>>(entities);

                return OperationResult<IEnumerable<PersonDto>>.CreateSuccess(
                    dtos,
                    $"P�gina {page} retornada com {dtos.Count()} pessoa(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<PersonDto>>.CreateFailure(
                    "Erro ao buscar pessoas paginadas.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Retorna o total de pessoas cadastradas.
        /// </summary>
        public async Task<int> GetTotalCountAsync()
        {
            try
            {
                return await _personRepository.GetTotalCountAsync();
            }
            catch
            {
                return 0;
            }
        }

        #endregion

        #region Search Operations

        /// <summary>
        /// Busca pessoas por termo (nome, email, documento).
        /// </summary>
        public async Task<OperationResult<IEnumerable<PersonDto>>> SearchAsync(string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term))
                    return OperationResult<IEnumerable<PersonDto>>.CreateFailure("Termo de busca � obrigat�rio.");

                var entities = await _personRepository.SearchAsync(term);
                var dtos = _mapper.Map<IEnumerable<PersonDto>>(entities);

                return OperationResult<IEnumerable<PersonDto>>.CreateSuccess(
                    dtos,
                    $"{dtos.Count()} pessoa(s) encontrada(s) para '{term}'.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<PersonDto>>.CreateFailure(
                    "Erro ao buscar pessoas.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Busca pessoa por email.
        /// </summary>
        public async Task<OperationResult<PersonDto>> GetByEmailAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return OperationResult<PersonDto>.CreateFailure("Email � obrigat�rio.");

                var entity = await _personRepository.GetByEmailAsync(email);

                if (entity == null)
                    return OperationResult<PersonDto>.CreateFailure("Pessoa n�o encontrada.");

                var dto = _mapper.Map<PersonDto>(entity);
                return OperationResult<PersonDto>.CreateSuccess(dto, "Pessoa encontrada com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<PersonDto>.CreateFailure(
                    "Erro ao buscar pessoa por email.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Busca pessoa por documento fiscal.
        /// </summary>
        public async Task<OperationResult<PersonDto>> GetByTaxIdAsync(string taxId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(taxId))
                    return OperationResult<PersonDto>.CreateFailure("Documento fiscal � obrigat�rio.");

                var entity = await _personRepository.GetByTaxIdAsync(taxId);

                if (entity == null)
                    return OperationResult<PersonDto>.CreateFailure("Pessoa n�o encontrada.");

                var dto = _mapper.Map<PersonDto>(entity);
                return OperationResult<PersonDto>.CreateSuccess(dto, "Pessoa encontrada com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<PersonDto>.CreateFailure(
                    "Erro ao buscar pessoa por documento.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Busca pessoa por n�mero de identifica��o.
        /// </summary>
        public async Task<OperationResult<PersonDto>> GetByIdentificationNumberAsync(string identificationNumber)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(identificationNumber))
                    return OperationResult<PersonDto>.CreateFailure("N�mero de identifica��o � obrigat�rio.");

                var entity = await _personRepository.GetByIdentificationNumberAsync(identificationNumber);

                if (entity == null)
                    return OperationResult<PersonDto>.CreateFailure("Pessoa n�o encontrada.");

                var dto = _mapper.Map<PersonDto>(entity);
                return OperationResult<PersonDto>.CreateSuccess(dto, "Pessoa encontrada com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<PersonDto>.CreateFailure(
                    "Erro ao buscar pessoa por n�mero de identifica��o.",
                    new[] { ex.Message });
            }
        }

        #endregion

        #region Query by Type

        /// <summary>
        /// Retorna apenas clientes.
        /// </summary>
        public async Task<OperationResult<IEnumerable<PersonDto>>> GetCustomersAsync()
        {
            try
            {
                var entities = await _personRepository.GetCustomersAsync();
                var dtos = _mapper.Map<IEnumerable<PersonDto>>(entities);

                return OperationResult<IEnumerable<PersonDto>>.CreateSuccess(
                    dtos,
                    $"{dtos.Count()} cliente(s) encontrado(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<PersonDto>>.CreateFailure(
                    "Erro ao buscar clientes.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Retorna apenas fornecedores.
        /// </summary>
        public async Task<OperationResult<IEnumerable<PersonDto>>> GetSuppliersAsync()
        {
            try
            {
                var entities = await _personRepository.GetSuppliersAsync();
                var dtos = _mapper.Map<IEnumerable<PersonDto>>(entities);

                return OperationResult<IEnumerable<PersonDto>>.CreateSuccess(
                    dtos,
                    $"{dtos.Count()} fornecedor(es) encontrado(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<PersonDto>>.CreateFailure(
                    "Erro ao buscar fornecedores.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Retorna apenas funcion�rios.
        /// </summary>
        public async Task<OperationResult<IEnumerable<PersonDto>>> GetEmployeesAsync()
        {
            try
            {
                var entities = await _personRepository.GetEmployeesAsync();
                var dtos = _mapper.Map<IEnumerable<PersonDto>>(entities);

                return OperationResult<IEnumerable<PersonDto>>.CreateSuccess(
                    dtos,
                    $"{dtos.Count()} funcion�rio(s) encontrado(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<PersonDto>>.CreateFailure(
                    "Erro ao buscar funcion�rios.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Retorna pessoas por tipo espec�fico.
        /// </summary>
        public async Task<OperationResult<IEnumerable<PersonDto>>> GetByTypeAsync(PersonType type)
        {
            try
            {
                var entities = await _personRepository.GetByTypeAsync(type);
                var dtos = _mapper.Map<IEnumerable<PersonDto>>(entities);

                return OperationResult<IEnumerable<PersonDto>>.CreateSuccess(
                    dtos,
                    $"{dtos.Count()} pessoa(s) do tipo {type} encontrada(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<PersonDto>>.CreateFailure(
                    "Erro ao buscar pessoas por tipo.",
                    new[] { ex.Message });
            }
        }

        #endregion

        #region Status Operations

        /// <summary>
        /// Retorna apenas pessoas ativas.
        /// </summary>
        public async Task<OperationResult<IEnumerable<PersonDto>>> GetActiveAsync()
        {
            try
            {
                var entities = await _personRepository.GetActiveAsync();
                var dtos = _mapper.Map<IEnumerable<PersonDto>>(entities);

                return OperationResult<IEnumerable<PersonDto>>.CreateSuccess(
                    dtos,
                    $"{dtos.Count()} pessoa(s) ativa(s) encontrada(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<PersonDto>>.CreateFailure(
                    "Erro ao buscar pessoas ativas.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Retorna apenas pessoas inativas.
        /// </summary>
        public async Task<OperationResult<IEnumerable<PersonDto>>> GetInactiveAsync()
        {
            try
            {
                var entities = await _personRepository.GetInactiveAsync();
                var dtos = _mapper.Map<IEnumerable<PersonDto>>(entities);

                return OperationResult<IEnumerable<PersonDto>>.CreateSuccess(
                    dtos,
                    $"{dtos.Count()} pessoa(s) inativa(s) encontrada(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<PersonDto>>.CreateFailure(
                    "Erro ao buscar pessoas inativas.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Ativa uma pessoa.
        /// </summary>
        public async Task<OperationResult> ActivateAsync(int id)
        {
            try
            {
                var person = await _personRepository.GetByIdAsync(id);
                if (person == null)
                    return OperationResult.CreateFailure("Pessoa n�o encontrada.");

                if (person.IsActive)
                    return OperationResult.CreateFailure("Pessoa j� est� ativa.");

                person.IsActive = true;
                await _personRepository.UpdateAsync(person);

                return OperationResult.CreateSuccess("Pessoa ativada com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult.CreateFailure(
                    "Erro ao ativar pessoa.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Desativa uma pessoa.
        /// </summary>
        public async Task<OperationResult> DeactivateAsync(int id)
        {
            try
            {
                var person = await _personRepository.GetByIdAsync(id);
                if (person == null)
                    return OperationResult.CreateFailure("Pessoa n�o encontrada.");

                if (!person.IsActive)
                    return OperationResult.CreateFailure("Pessoa j� est� inativa.");

                person.IsActive = false;
                await _personRepository.UpdateAsync(person);

                return OperationResult.CreateSuccess("Pessoa desativada com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult.CreateFailure(
                    "Erro ao desativar pessoa.",
                    new[] { ex.Message });
            }
        }

        #endregion

        #region Validation Operations

        /// <summary>
        /// Verifica se um email j� est� em uso.
        /// </summary>
        public async Task<bool> EmailExistsAsync(string email, int? excludePersonId = null)
        {
            try
            {
                return await _personRepository.EmailExistsAsync(email, excludePersonId);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Verifica se um documento fiscal j� est� em uso.
        /// </summary>
        public async Task<bool> TaxIdExistsAsync(string taxId, int? excludePersonId = null)
        {
            try
            {
                return await _personRepository.TaxIdExistsAsync(taxId, excludePersonId);
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Financial Operations

        /// <summary>
        /// Atualiza o saldo atual de um cliente.
        /// </summary>
        public async Task<OperationResult> UpdateBalanceAsync(int personId, decimal amount)
        {
            try
            {
                var person = await _personRepository.GetByIdAsync(personId);
                if (person == null)
                    return OperationResult.CreateFailure("Pessoa n�o encontrada.");

                if (!person.IsCustomer)
                    return OperationResult.CreateFailure("Opera��o v�lida apenas para clientes.");

                var newBalance = person.CurrentBalance + amount;

                if (newBalance < 0)
                    return OperationResult.CreateFailure("Saldo n�o pode ser negativo.");

                if (newBalance > person.CreditLimit && person.CreditLimit > 0)
                    return OperationResult.CreateFailure($"Opera��o excede o limite de cr�dito (R$ {person.CreditLimit:N2}).");

                person.CurrentBalance = newBalance;
                await _personRepository.UpdateAsync(person);

                return OperationResult.CreateSuccess($"Saldo atualizado com sucesso. Novo saldo: R$ {newBalance:N2}");
            }
            catch (Exception ex)
            {
                return OperationResult.CreateFailure(
                    "Erro ao atualizar saldo.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Retorna clientes com saldo devedor.
        /// </summary>
        public async Task<OperationResult<IEnumerable<PersonDto>>> GetCustomersWithDebtAsync()
        {
            try
            {
                var entities = await _personRepository.GetCustomersWithDebtAsync();
                var dtos = _mapper.Map<IEnumerable<PersonDto>>(entities);

                return OperationResult<IEnumerable<PersonDto>>.CreateSuccess(
                    dtos,
                    $"{dtos.Count()} cliente(s) com saldo devedor.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<PersonDto>>.CreateFailure(
                    "Erro ao buscar clientes com d�vidas.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Retorna clientes pr�ximos ou acima do limite de cr�dito.
        /// </summary>
        public async Task<OperationResult<IEnumerable<PersonDto>>> GetCustomersNearCreditLimitAsync(decimal percentageThreshold = 90)
        {
            try
            {
                if (percentageThreshold < 0 || percentageThreshold > 100)
                    return OperationResult<IEnumerable<PersonDto>>.CreateFailure("Percentual deve estar entre 0 e 100.");

                var entities = await _personRepository.GetCustomersNearCreditLimitAsync(percentageThreshold);
                var dtos = _mapper.Map<IEnumerable<PersonDto>>(entities);

                return OperationResult<IEnumerable<PersonDto>>.CreateSuccess(
                    dtos,
                    $"{dtos.Count()} cliente(s) pr�ximo(s) do limite de cr�dito.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<PersonDto>>.CreateFailure(
                    "Erro ao buscar clientes pr�ximos do limite.",
                    new[] { ex.Message });
            }
        }

        #endregion
    }
}