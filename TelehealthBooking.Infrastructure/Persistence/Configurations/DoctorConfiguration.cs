using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TelehealthBooking.Domain.Entities;

namespace TelehealthBooking.Infrastructure.Persistence.Configurations;

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name)
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(d => d.Specialization)
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(d => d.Id).ValueGeneratedNever();
    }
}
