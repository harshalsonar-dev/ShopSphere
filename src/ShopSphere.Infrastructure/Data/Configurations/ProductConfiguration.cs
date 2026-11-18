using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopSphere.Domain.Entities;

namespace ShopSphere.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(2000);

        builder.Property(p => p.Price)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.StockQuantity)
            .IsRequired();

        // Database Indexes as required: CategoryId and Price
        builder.HasIndex(p => p.CategoryId)
            .HasDatabaseName("IX_Products_CategoryId");

        builder.HasIndex(p => p.Price)
            .HasDatabaseName("IX_Products_Price");

        // Composite Index for category-filtered price-range queries
        builder.HasIndex(p => new { p.CategoryId, p.Price })
            .HasDatabaseName("IX_Products_CategoryId_Price");

        builder.HasIndex(p => p.Name)
            .HasDatabaseName("IX_Products_Name");

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed initial products
        builder.HasData(
            new
            {
                Id = 1,
                Name = "Ultra HD 4K Smart Monitor 32-inch",
                Description = "Crisp IPS display with 144Hz refresh rate and USB-C connectivity.",
                Price = 399.99m,
                StockQuantity = 45,
                CategoryId = 1,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = (DateTime?)null
            },
            new
            {
                Id = 2,
                Name = "Wireless Noise-Cancelling Headphones Pro",
                Description = "High-fidelity active noise cancellation with 40-hour battery life.",
                Price = 199.99m,
                StockQuantity = 80,
                CategoryId = 2,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = (DateTime?)null
            },
            new
            {
                Id = 3,
                Name = "Ergonomic Mechanical Keyboard RGB",
                Description = "Tactile hot-swappable switches, PBT keycaps, and custom layout software.",
                Price = 129.50m,
                StockQuantity = 60,
                CategoryId = 3,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = (DateTime?)null
            },
            new
            {
                Id = 4,
                Name = "Pro Gaming Wireless Mouse",
                Description = "Ultra-lightweight 58g shell with 26K DPI optical sensor.",
                Price = 79.99m,
                StockQuantity = 110,
                CategoryId = 4,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = (DateTime?)null
            },
            new
            {
                Id = 5,
                Name = "Thunderbolt 4 Docking Station",
                Description = "Dual 4K display support, 100W PD charging, and 10Gbps USB-A ports.",
                Price = 179.00m,
                StockQuantity = 35,
                CategoryId = 1,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = (DateTime?)null
            }
        );
    }
}
