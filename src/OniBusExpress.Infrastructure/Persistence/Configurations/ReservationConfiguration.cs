using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OniBusExpress.Domain.Passengers;
using OniBusExpress.Domain.Reservations;
using OniBusExpress.Domain.Trips;

namespace OniBusExpress.Infrastructure.Persistence.Configurations;

public sealed class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("reservation");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");

        builder.Property(r => r.Code)
            .HasColumnName("code")
            .HasMaxLength(9)
            .HasConversion(code => code.Value, value => ReservationCode.FromPersistence(value))
            .IsRequired();

        builder.Property(r => r.TripId).HasColumnName("trip_id");
        builder.Property(r => r.SeatNumber).HasColumnName("seat_number");

        builder.Property(r => r.PassengerName)
            .HasColumnName("passenger_name")
            .HasMaxLength(120)
            .HasConversion(name => name.Value, value => PassengerName.FromPersistence(value))
            .IsRequired();

        builder.Property(r => r.PassengerCpf)
            .HasColumnName("passenger_cpf")
            .HasMaxLength(11)
            .HasConversion(cpf => cpf.Value, value => Cpf.FromPersistence(value))
            .IsRequired();

        builder.Property(r => r.PassengerEmail)
            .HasColumnName("passenger_email")
            .HasMaxLength(320)
            .HasConversion(email => email.Value, value => PassengerEmail.FromPersistence(value))
            .IsRequired();

        builder.Property(r => r.PassengerDateOfBirth).HasColumnName("passenger_date_of_birth");

        builder.Property(r => r.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(r => r.CreatedAt).HasColumnName("created_at");
        builder.Property(r => r.CancelledAt).HasColumnName("cancelled_at");

        builder.HasOne<Trip>().WithMany().HasForeignKey(r => r.TripId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.Code).IsUnique().HasDatabaseName("ux_reservation_code");

        builder.HasIndex(r => new { r.TripId, r.SeatNumber })
            .IsUnique()
            .HasFilter("status = 'Confirmed'")
            .HasDatabaseName("ux_reservation_active_seat");
    }
}
