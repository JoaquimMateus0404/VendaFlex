namespace VendaFlex.Core.Interfaces
{
    /// <summary>
    /// Provedor simples para manter o ID do usu�rio atual no escopo.
    /// N�o deve depender de DbContext nem de reposit�rios.
    /// </summary>
    public interface ICurrentUserContext
    {
        int? UserId { get; set; }
    }
}
