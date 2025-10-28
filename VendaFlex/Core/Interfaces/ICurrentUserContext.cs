namespace VendaFlex.Core.Interfaces
{
    /// <summary>
    /// Provedor simples para manter o ID do usuário atual no escopo.
    /// Não deve depender de DbContext nem de repositórios.
    /// </summary>
    public interface ICurrentUserContext
    {
        int? UserId { get; set; }
    }
}
