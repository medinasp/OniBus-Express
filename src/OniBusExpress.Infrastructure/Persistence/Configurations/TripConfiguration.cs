using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OniBusExpress.Domain.Trips;

namespace OniBusExpress.Infrastructure.Persistence.Configurations;

public sealed class TripConfiguration : IEntityTypeConfiguration<Trip>
{
    public void Configure(EntityTypeBuilder<Trip> builder)
    {
        builder.ToTable("trip");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id");
        builder.Property(t => t.RouteId).HasColumnName("route_id");
        builder.Property(t => t.DepartureAt).HasColumnName("departure_at");
        builder.Property(t => t.ArrivalAt).HasColumnName("arrival_at");
        builder.Property(t => t.Price).HasColumnName("price").HasColumnType("numeric(10,2)");
        builder.Property(t => t.TotalSeats).HasColumnName("total_seats");

        builder.HasOne<Route>().WithMany().HasForeignKey(t => t.RouteId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => new { t.RouteId, t.DepartureAt }).HasDatabaseName("ix_trip_route_departure");
    }
}
