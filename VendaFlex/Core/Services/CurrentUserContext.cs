using VendaFlex.Core.Interfaces;

namespace VendaFlex.Core.Services
{
    /// <summary>
    /// Implementação simples com propriedade mutável para o ID do usuário atual.
    /// Registrada como Scoped para acompanhar o escopo de requisição/serviço.
    /// </summary>
    public class CurrentUserContext : ICurrentUserContext
    {
        public int? UserId { get; set; }
    }
}
