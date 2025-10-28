using VendaFlex.Core.Interfaces;

namespace VendaFlex.Core.Services
{
    /// <summary>
    /// Implementa��o simples com propriedade mut�vel para o ID do usu�rio atual.
    /// Registrada como Scoped para acompanhar o escopo de requisi��o/servi�o.
    /// </summary>
    public class CurrentUserContext : ICurrentUserContext
    {
        public int? UserId { get; set; }
    }
}
