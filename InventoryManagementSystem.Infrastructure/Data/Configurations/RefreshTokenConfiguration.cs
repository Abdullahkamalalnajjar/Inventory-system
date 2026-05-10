using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InventoryManagementSystem.Domain.Identity;

namespace InventoryManagementSystem.Infrastructure.Data.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(token => token.Id);

        builder.Property(token => token.Token)
            .IsRequired();

        builder.Property(token => token.UserId)
            .IsRequired();

        builder.Property(token => token.ExpiresOnUtc)
            .IsRequired();

        builder.HasIndex(token => token.Token)
            .IsUnique();
    }
}
