using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendaFlex.Data.Entities
{
    /// <summary>
    /// Entidade central que representa qualquer pessoa física ou jurídica
    /// Pode ser Cliente, Fornecedor, Funcionário ou múltiplos tipos
    /// </summary>
    public class Person : AuditableEntity
    {
        [Key]
        public int PersonId { get; set; }

        [Required]
        [StringLength(200)]
        public required string Name { get; set; }

        [Required]
        public PersonType Type { get; set; }

    // Dados de Identificação
    [StringLength(50)]
    public string? TaxId { get; set; } // NIF, CNPJ, CPF

    [StringLength(50)]
    public string? IdentificationNumber { get; set; } // BI, RG, Passaporte

    // Dados de Contato
    [StringLength(200)]
    [EmailAddress]
    public string? Email { get; set; }

    [StringLength(20)]
    [Phone]
    public string? PhoneNumber { get; set; }

    [StringLength(20)]
    public string? MobileNumber { get; set; }

    [StringLength(200)]
    public string? Website { get; set; }

    // Endereço Completo
    [StringLength(500)]
    public string? Address { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(100)]
    public string? State { get; set; }

    [StringLength(20)]
    public string? PostalCode { get; set; }

    [StringLength(100)]
    public string? Country { get; set; }        // Dados Financeiros (para Clientes)
        [Column(TypeName = "decimal(18,2)")]
        public decimal CreditLimit { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentBalance { get; set; } = 0;

    // Dados Adicionais
    [StringLength(200)]
    public string? ContactPerson { get; set; } // Para empresas

    [StringLength(1000)]
    public string? Notes { get; set; }

    [StringLength(500)]
    public string? ProfileImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    // Avaliação (para Fornecedores)
    [Range(1, 5)]
    public int? Rating { get; set; }

    // Flags de Tipo (computadas do enum Type)
    [NotMapped]
    public bool IsCustomer => Type == PersonType.Customer || Type == PersonType.Both;

    [NotMapped]
    public bool IsSupplier => Type == PersonType.Supplier || Type == PersonType.Both;

    [NotMapped]
    public bool IsEmployee => Type == PersonType.Employee;

    // Navigation Properties
    public virtual User? User { get; set; } // 1:1 se for Employee/User
        public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
        public virtual ICollection<Product> SuppliedProducts { get; set; } = new List<Product>();
    }

    public enum PersonType
    {
        Customer = 1, /// Cliente
        Supplier = 2, /// Fornecedor
        Employee = 3, /// Funcionário
        Both = 4  // Cliente e Fornecedor
    }
}
