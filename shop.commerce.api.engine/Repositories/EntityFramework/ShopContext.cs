using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using shop.commerce.api.infrastructure.Configuration;
using shop.commerce.api.infrastructure.Repositories.Entities;
using shop.commerce.api.infrastructure.Utilities;
using System.Linq;

namespace shop.commerce.api.infrastructure.Repositories.EntityFramework
{
    public partial class ShopContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderTracking> OrderTrackings { get; set; }
        public DbSet<Slide> Slides { get; set; }

        public ShopContext(DbContextOptions<ShopContext> options) : base(options)
        {

        }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);
        //    //modelBuilder.Entity<Product>().ToTable("Products");
        //    modelBuilder.ApplyConfiguration(new ProductConfiguration());
        //    modelBuilder.ApplyConfiguration(new ProductImageConfiguration());
        //    modelBuilder.ApplyConfiguration(new OrderTrackingConfiguration());
        //    modelBuilder.ApplyConfiguration(new SlideConfiguration());
        //}
    }

    public partial class ShopContext
    {
        private readonly ILoggerFactory loggerFactory;

        /// <summary>
        /// default constructor with option builder
        /// </summary>
        /// <param name="options">the options builder</param>
        public ShopContext(
            DbContextOptions<ShopContext> options,
            ILoggerFactory loggerFactory) : base(options)
        {
            this.loggerFactory = loggerFactory;
        }

        /// <inheritdoc/>>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder
                .ApplyStringDefaultSize()
                .ApplyBaseEntityConfiguration()
                .ApplyAllConfigurations();

        /// <inheritdoc/>>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLoggerFactory(this.loggerFactory);
        }

        /// <summary>
        /// this method is used to detach all entities being tracked by the change tracker
        /// </summary>
        public void DetachAllEntities()
        {
            var changedEntriesCopy = ChangeTracker.Entries()
                .Where(entry =>
                    entry.State == EntityState.Added ||
                    entry.State == EntityState.Modified ||
                    entry.State == EntityState.Unchanged ||
                    entry.State == EntityState.Deleted)
                .ToList();

            foreach (var entry in changedEntriesCopy)
                entry.State = EntityState.Detached;
        }

        /// <summary>
        /// this method is used to seed data
        /// </summary>
        /// <returns></returns>
        public void SeedData(bool seedData = false, bool updateEquipmentIds = false)
        {
            if (seedData)
            {
            }
        }
    }
}
