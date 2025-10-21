using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace VendaFlex.Infrastructure
{
    public interface IDatabaseStatusService
    {
        bool SqlServerConfigured { get; }
        bool SqlServerConnected { get; }
        int SqlServerAppliedMigrations { get; }
        int SqlServerPendingMigrations { get; }
        string SqlServerError { get; }

        bool SqliteAvailable { get; }
        int SqliteAppliedMigrations { get; }
        int SqlitePendingMigrations { get; }
        string SqlitePath { get; }
        string SqliteError { get; }

        void UpdateSqlServerStatus(bool configured, bool connected, int applied, int pending, string? error = null);
        void UpdateSqliteStatus(bool available, int applied, int pending, string path, string? error = null);
    }

    public class DatabaseStatusService : IDatabaseStatusService
    {
        private readonly object _lock = new();

        public bool SqlServerConfigured { get; private set; }
        public bool SqlServerConnected { get; private set; }
        public int SqlServerAppliedMigrations { get; private set; }
        public int SqlServerPendingMigrations { get; private set; }
        public string SqlServerError { get; private set; } = string.Empty;

        public bool SqliteAvailable { get; private set; }
        public int SqliteAppliedMigrations { get; private set; }
        public int SqlitePendingMigrations { get; private set; }
        public string SqlitePath { get; private set; } = string.Empty;
        public string SqliteError { get; private set; } = string.Empty;

        public void UpdateSqlServerStatus(bool configured, bool connected, int applied, int pending, string? error = null)
        {
            lock (_lock)
            {
                SqlServerConfigured = configured;
                SqlServerConnected = connected;
                SqlServerAppliedMigrations = applied;
                SqlServerPendingMigrations = pending;
                SqlServerError = error ?? string.Empty;
            }
        }

        public void UpdateSqliteStatus(bool available, int applied, int pending, string path, string? error = null)
        {
            lock (_lock)
            {
                SqliteAvailable = available;
                SqliteAppliedMigrations = applied;
                SqlitePendingMigrations = pending;
                SqlitePath = path;
                SqliteError = error ?? string.Empty;
            }
        }
    }
}
