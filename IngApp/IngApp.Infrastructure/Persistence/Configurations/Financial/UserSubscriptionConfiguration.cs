using IngApp.Domain.Entities.Financial;
using IngApp.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IngApp.Infrastructure.Persistence.Configurations.Financial;

public class UserSubscriptionConfiguration : IEntityTypeConfiguration<UserSubscription>
{
    public void Configure(EntityTypeBuilder<UserSubscription> builder)
    {
        builder.HasKey(us => us.Id);

        builder.Property(us => us.UserId)
            .IsRequired();

        builder.Property(us => us.PlanId)
            .IsRequired();

        builder.Property(us => us.StatusId)
            .IsRequired();

        builder.Property(us => us.StartDate)
            .IsRequired();

        builder.Property(us => us.EndDate)
            .IsRequired();

        builder.Property(us => us.PurchasedAt)
            .IsRequired();

        builder.Property(us => us.CreatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(us => us.User)
            .WithMany(u => u.UserSubscriptions)
            .HasForeignKey(us => us.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(us => us.Plan)
            .WithMany(p => p.UserSubscriptions)
            .HasForeignKey(us => us.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(us => us.Status)
            .WithMany()
            .HasForeignKey(us => us.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(us => us.UserId);
        builder.HasIndex(us => new { us.UserId, us.StatusId });
        builder.HasIndex(us => us.EndDate);
    }
}











