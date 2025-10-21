using VendaFlex.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VendaFlex.Data.Configurations
{
    public class PersonConfiguration : IEntityTypeConfiguration<Person>
    {
        public void Configure(EntityTypeBuilder<Person> builder)
        {
            builder.HasIndex(p => p.TaxId);
            builder.HasIndex(p => p.Email);
            builder.HasIndex(p => p.Type);
            builder.HasQueryFilter(e => !e.IsDeleted);
            builder.HasOne(p => p.User)
                .WithOne(u => u.Person)
                .HasForeignKey<User>(u => u.PersonId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
