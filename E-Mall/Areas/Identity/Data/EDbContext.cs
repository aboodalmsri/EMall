using E_Mall.Areas.Identity.Data;
using E_Mall.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace E_Mall.Areas.Identity.Data;

public class EDbContext : IdentityDbContext<E_MallUser, IdentityRole, string>
{
    public EDbContext(DbContextOptions<EDbContext> options)
        : base(options)
    {
    }
    public DbSet<DeliveryDriver> DeliveryDrivers { get; set; }
    public DbSet<Merchant> Merchants { get; set; }
    public DbSet<Store> Stores { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Offer> Offers { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Wishlist> Wishlists { get; set; }
    public DbSet<WishlistItem> WishlistItems { get; set; }
    public DbSet<ProductReview> ProductReviews { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Contact>()
            .HasKey(f => f.id);

        builder.Entity<Contact>()
            .Property(f => f.id)
            .HasMaxLength(36);

        builder.Entity<Order>()
            .HasMany(o => o.OrderItems)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        // *** IMPORTANT: Configure relationships here ***

        // E_MallUser to DeliveryDriver (One-to-One or One-to-Many - adjust as needed)
        builder.Entity<DeliveryDriver>()
            .HasOne(d => d.User)
            .WithMany() // Or .WithMany(u => u.DeliveryDrivers) if E_MallUser has a DeliveryDrivers collection
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Recommended: Cascade delete if a user is deleted


        // E_MallUser to Merchant (One-to-One or One-to-Many)
        builder.Entity<Merchant>()
            .HasOne(m => m.User)
            .WithMany() // Or .WithMany(u => u.Merchants)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);


        // CartItem to Cart (Many-to-One)
        builder.Entity<CartItem>()
            .HasOne(ci => ci.Cart)
            .WithMany(c => c.CartItems)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade); // Cascading delete is generally appropriate here

        // CartItem to Product (Many-to-One)
        builder.Entity<CartItem>()
            .HasOne(ci => ci.Product)
            .WithMany(p => p.CartItems)
            .HasForeignKey(ci => ci.ProductId)
            .OnDelete(DeleteBehavior.NoAction); // NoAction is OK here


        // Review to Store (Many-to-One)
        builder.Entity<Review>()
            .HasOne(r => r.Store)
            .WithMany(s => s.Reviews)
            .HasForeignKey(r => r.StoreId)
            .OnDelete(DeleteBehavior.NoAction); // NoAction is OK here

        // Store to Merchant (One-to-Many - One Merchant can have Many Stores)  **** CORRECTED ***
        builder.Entity<Store>()
            .HasOne(s => s.Merchant)
            .WithMany(m => m.Stores) // Merchant MUST have a Stores collection for this to work
            .HasForeignKey(s => s.MerchantId)
            .OnDelete(DeleteBehavior.Cascade);  // If a Merchant is deleted, their stores should be deleted

        // Store to Category (Many-to-One - Many Stores can belong to One Category)
        builder.Entity<Store>()
            .HasOne(s => s.Category)
            .WithMany(c => c.Stores)
            .HasForeignKey(s => s.CategoryId)
            .OnDelete(DeleteBehavior.NoAction); // NoAction is OK here,  or Cascade if desired.

        // Product to Store (Many-to-One - Many Products can belong to One Store)
        builder.Entity<Product>()
            .HasOne(p => p.Store)
            .WithMany(s => s.Products)  // Store MUST have a Products collection
            .HasForeignKey(p => p.StoreId)
            .OnDelete(DeleteBehavior.Cascade); // Cascade if a store is deleted, its products should be too.

        // Cart Configuration
        builder.Entity<Cart>()
            .HasOne(c => c.User)
            .WithMany()  // Or .WithOne() if a user can only have one cart
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);  // Decide what happens when a user is deleted

        // CartItem Configuration
        builder.Entity<CartItem>()
            .HasOne(ci => ci.Cart)
            .WithMany(c => c.CartItems)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);  // Delete cart items when cart is deleted

        builder.Entity<CartItem>()
            .HasOne(ci => ci.Product)
            .WithMany(p => p.CartItems)
            .HasForeignKey(ci => ci.ProductId)
            .OnDelete(DeleteBehavior.Restrict);  // Do not delete product when cart item is deleted

        // Wishlist Configuration (Similar to Cart)
        builder.Entity<Wishlist>()
            .HasOne(w => w.User)
            .WithMany() // Or .WithOne()
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // WishlistItem Configuration (Similar to CartItem)
        builder.Entity<WishlistItem>()
            .HasOne(wi => wi.Wishlist)
            .WithMany(w => w.WishlistItems)
            .HasForeignKey(wi => wi.WishlistId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<WishlistItem>()
            .HasOne(wi => wi.Product)
            .WithMany() // Products don't need to know about wishlist items
            .HasForeignKey(wi => wi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);  // Do not delete product when wishlist item is deleted

        // Other relationships (Store, Product, etc.) - Configure these as well!

        // Example: Store - Product
        builder.Entity<Store>()
            .HasMany(s => s.Products)
            .WithOne(p => p.Store)
            .HasForeignKey(p => p.StoreId)
            .OnDelete(DeleteBehavior.Cascade); // Example: Delete products when a store is deleted

        // Example: Store - Review
        builder.Entity<Store>()
            .HasMany(s => s.Reviews)
            .WithOne(r => r.Store)
            .HasForeignKey(r => r.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        //// Example: Product - Review
        //builder.Entity<Product>()
        //    .HasMany(p => p.ProductReviews)
        //    .WithOne() // No navigation back to Product
        //    .HasForeignKey(r => r.ProductId) // Corrected foreign key
        //    .OnDelete(DeleteBehavior.Restrict);

        // Example: Merchant - Store
        builder.Entity<Merchant>()
            .HasMany(m => m.Stores)
            .WithOne(s => s.Merchant)
            .HasForeignKey(s => s.MerchantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ApplyConfiguration(new ApplicationUserEntityConfiguration());
    }
}

public class ApplicationUserEntityConfiguration : IEntityTypeConfiguration<E_MallUser>
{
    public void Configure(EntityTypeBuilder<E_MallUser> builder)
    {
        builder.Property(x => x.FirstName).HasMaxLength(50);
        builder.Property(x => x.LastName).HasMaxLength(50);
        builder.Property(x => x.PhoneNumber);
    }
} 