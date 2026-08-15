using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OniBusExpress.Domain.Trips;

namespace OniBusExpress.Infrastructure.Persistence.Configurations;

public sealed class RouteConfiguration : IEntityTypeConfiguration<Route>
{
    public void Configure(EntityTypeBuilder<Route> builder)
    {
        builder.ToTable("route");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.Origin).HasColumnName("origin").HasMaxLength(80).IsRequired();
        builder.Property(r => r.Destination).HasColumnName("destination").HasMaxLength(80).IsRequired();
        builder.Property(r => r.EstimatedDuration).HasColumnName("estimated_duration");
    }
}
