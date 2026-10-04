using Microsoft.EntityFrameworkCore;
using StyleCart.ProductService.Models;

namespace StyleCart.ProductService.Data;

public sealed class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();

    public DbSet<Inventory> Inventories => Set<Inventory>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Product>()
            .Property(product => product.BasePrice)
            .HasPrecision(18, 2);

        builder.Entity<ProductVariant>()
            .Property(variant => variant.Price)
            .HasPrecision(18, 2);

        builder.Entity<ProductVariant>()
            .HasIndex(variant => variant.Sku)
            .IsUnique();

        builder.Entity<ProductVariant>()
            .HasOne(variant => variant.Product)
            .WithMany(product => product.Variants)
            .HasForeignKey(variant => variant.ProductId);

        builder.Entity<Inventory>()
            .HasOne(inventory => inventory.ProductVariant)
            .WithOne(variant => variant.Inventory)
            .HasForeignKey<Inventory>(inventory => inventory.ProductVariantId);
    }
}