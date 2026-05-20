using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TelehealthBooking.Domain.Entities;

namespace TelehealthBooking.Infrastructure.Persistence.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(p => p.Email)
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(p => p.Id).ValueGeneratedNever();
    }
}
