using VendaFlex.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VendaFlex.Data.Configurations
{
    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.HasIndex(i => i.InvoiceNumber).IsUnique();
            builder.HasIndex(i => i.Date);
            builder.HasIndex(i => i.Status);
            builder.HasQueryFilter(e => !e.IsDeleted);
            builder.HasOne(i => i.Person)
                .WithMany(p => p.Invoices)
                .HasForeignKey(i => i.PersonId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(i => i.User)
                .WithMany(u => u.Invoices)
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
