using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendaFlex.Core.Utils
{
    /// <summary>
    /// Representa o resultado de uma operação sem dados de retorno.
    /// </summary>
    public class OperationResult
    {
        /// <summary>
        /// Indica se a operação foi bem-sucedida.
        /// </summary>
        public bool Success { get; private set; }

        /// <summary>
        /// Mensagem descritiva sobre o resultado da operação.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Lista de erros de validação ou problemas encontrados.
        /// </summary>
        public IEnumerable<string> Errors { get; private set; }

        private OperationResult(bool success, string message, IEnumerable<string>? errors = null)
        {
            Success = success;
            Message = message;
            Errors = errors ?? new List<string>();
        }

        /// <summary>
        /// Cria um resultado de sucesso.
        /// </summary>
        /// <param name="message">Mensagem de sucesso</param>
        /// <returns>Resultado de sucesso</returns>
        public static OperationResult CreateSuccess(string message = "Operação realizada com sucesso.")
        {
            return new OperationResult(true, message);
        }

        /// <summary>
        /// Cria um resultado de falha.
        /// </summary>
        /// <param name="message">Mensagem de erro principal</param>
        /// <param name="errors">Lista de erros detalhados (opcional)</param>
        /// <returns>Resultado de falha</returns>
        public static OperationResult CreateFailure(string message = "Operação falhou.", IEnumerable<string>? errors = null)
        {
            return new OperationResult(false, message, errors);
        }
    }

    /// <summary>
    /// Representa o resultado de uma operação com dados de retorno.
    /// </summary>
    /// <typeparam name="T">Tipo de dados retornado pela operação</typeparam>
    public class OperationResult<T>
    {
        /// <summary>
        /// Indica se a operação foi bem-sucedida.
        /// </summary>
        public bool Success { get; private set; }

        /// <summary>
        /// Mensagem descritiva sobre o resultado da operação.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Dados retornados pela operação (disponível apenas em caso de sucesso).
        /// </summary>
        public T? Data { get; private set; }

        /// <summary>
        /// Lista de erros de validação ou problemas encontrados.
        /// </summary>
        public IEnumerable<string> Errors { get; private set; }

        private OperationResult(bool success, string message, T? data = default, IEnumerable<string>? errors = null)
        {
            Success = success;
            Message = message;
            Data = data;
            Errors = errors ?? new List<string>();
        }

        /// <summary>
        /// Cria um resultado de sucesso com dados.
        /// </summary>
        /// <param name="data">Dados retornados</param>
        /// <param name="message">Mensagem de sucesso</param>
        /// <returns>Resultado de sucesso com dados</returns>
        public static OperationResult<T> CreateSuccess(T data, string message = "Operação realizada com sucesso.")
        {
            return new OperationResult<T>(true, message, data);
        }

        /// <summary>
        /// Cria um resultado de falha sem dados.
        /// </summary>
        /// <param name="message">Mensagem de erro principal</param>
        /// <param name="errors">Lista de erros detalhados (opcional)</param>
        /// <returns>Resultado de falha</returns>
        public static OperationResult<T> CreateFailure(string message = "Operação falhou.", IEnumerable<string>? errors = null)
        {
            return new OperationResult<T>(false, message, default, errors);
        }
    }
}
