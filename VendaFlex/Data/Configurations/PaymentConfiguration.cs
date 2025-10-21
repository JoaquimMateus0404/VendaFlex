using VendaFlex.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VendaFlex.Data.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.HasQueryFilter(e => !e.IsDeleted);
            builder.HasOne(p => p.Invoice)
                .WithMany(i => i.Payments)
                .HasForeignKey(p => p.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(p => p.PaymentType)
                .WithMany(pt => pt.Payments)
                .HasForeignKey(p => p.PaymentTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
