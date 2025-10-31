using System;

namespace VendaFlex.Core.DTOs
{
    /// <summary>
    /// DTO para notificações do dashboard
    /// </summary>
    public class DashboardNotificationDto
    {
        /// <summary>
        /// Título da notificação
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Mensagem da notificação
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Ícone do Material Design
        /// </summary>
        public string IconKind { get; set; } = string.Empty;

        /// <summary>
        /// Cor de fundo do ícone
        /// </summary>
        public string IconBackgroundColor { get; set; } = string.Empty;

        /// <summary>
        /// Cor do ícone
        /// </summary>
        public string IconColor { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp da notificação
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Texto de tempo relativo (ex: "Há 2 horas")
        /// </summary>
        public string TimeAgo
        {
            get
            {
                var difference = DateTime.Now - Timestamp;
                
                if (difference.TotalMinutes < 1)
                    return "Agora mesmo";
                if (difference.TotalMinutes < 60)
                    return $"Há {(int)difference.TotalMinutes} minuto{((int)difference.TotalMinutes > 1 ? "s" : "")}";
                if (difference.TotalHours < 24)
                    return $"Há {(int)difference.TotalHours} hora{((int)difference.TotalHours > 1 ? "s" : "")}";
                if (difference.TotalDays < 7)
                    return $"Há {(int)difference.TotalDays} dia{((int)difference.TotalDays > 1 ? "s" : "")}";
                
                return Timestamp.ToString("dd/MM/yyyy");
            }
        }

        /// <summary>
        /// Indica se a notificação foi lida
        /// </summary>
        public bool IsRead { get; set; }
    }
}
