namespace VendaFlex.Core.DTOs
{
    public class ExpirationDto
    {
        public int ExpirationId { get; set; }
        public int ProductId { get; set; }
        public DateTime ExpirationDate { get; set; }
        public int Quantity { get; set; }
        public string BatchNumber { get; set; }
        public string Notes { get; set; }
        
        // Propriedades adicionais para exibição
        public string ProductName { get; set; }
        
        /// <summary>
        /// Verifica se o lote já está vencido
        /// </summary>
        public bool IsExpired => ExpirationDate.Date < DateTime.Now.Date;
        
        /// <summary>
        /// Calcula quantos dias faltam para vencer (negativo se já venceu)
        /// </summary>
        public int DaysUntilExpiration => (ExpirationDate.Date - DateTime.Now.Date).Days;
        
        /// <summary>
        /// Verifica se está próximo do vencimento (baseado nos dias de aviso do produto)
        /// </summary>
        public bool IsNearExpiration { get; set; }
        
        /// <summary>
        /// Dias de aviso do produto (para calcular IsNearExpiration)
        /// </summary>
        public int? ExpirationWarningDays { get; set; }
        
        /// <summary>
        /// Status em texto formatado
        /// </summary>
        public string ExpirationStatus
        {
            get
            {
                if (IsExpired)
                    return $"Vencido há {Math.Abs(DaysUntilExpiration)} dias";
                    
                if (DaysUntilExpiration == 0)
                    return "Vence HOJE";
                    
                if (DaysUntilExpiration == 1)
                    return "Vence AMANHÃ";
                    
                if (IsNearExpiration)
                    return $"Vence em {DaysUntilExpiration} dias ⚠️";
                    
                return $"Vence em {DaysUntilExpiration} dias";
            }
        }
        
        /// <summary>
        /// Cor de fundo baseada no status de vencimento
        /// </summary>
        public string StatusColor
        {
            get
            {
                if (IsExpired) return "#F44336"; // Vermelho
                if (IsNearExpiration) return "#FF9800"; // Laranja
                return "#4CAF50"; // Verde
            }
        }
        
        /// <summary>
        /// Texto do status simplificado
        /// </summary>
        public string StatusText
        {
            get
            {
                if (IsExpired) return "Vencido";
                if (IsNearExpiration) return "Próx. Venc.";
                return "Válido";
            }
        }
    }
}
