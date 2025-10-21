namespace VendaFlex.Core.Interfaces
{
    public interface ISyncService
    {
        Task<bool> SyncToSqlServerAsync();
        Task<bool> SyncToSqliteAsync();
        Task<bool> HasPendingChangesAsync();
    }
}
