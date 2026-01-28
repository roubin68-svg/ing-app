using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class KycTemplateConfiguration : IEntityTypeConfiguration<KycTemplate>
{
    public void Configure(EntityTypeBuilder<KycTemplate> builder)
    {

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SortOrder)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasOne(x => x.KycAttributeDefinition)
            .WithMany()
            .HasForeignKey(x => x.KycAttributeDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
