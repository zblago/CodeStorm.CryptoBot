using CodeStorm.CryptoTrader.Repository.DbEntities;
using DataFetcher.Repository;
using Microsoft.EntityFrameworkCore;

namespace CodeStorm.CryptoTrader.Repository.DatabaseContext
{
    public class CryptoTraderDbContext : DbContext
    {
        public CryptoTraderDbContext() { }

        public CryptoTraderDbContext(DbContextOptions<CryptoTraderDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //modelBuilder.Entity<Customer>()
            //    .HasMany(e => e.Devices)
            //    .WithOne(e => e.Customer)
            //    .HasForeignKey(e => e.CustomerId)
            //    .IsRequired();

            //modelBuilder.Entity<Device>()
            //    .HasMany(e => e.Sensors)
            //    .WithOne(e => e.Device)
            //    .HasForeignKey(e => e.DeviceId)
            //    .IsRequired();

            //modelBuilder.Entity<Sensor>()
            //    .HasMany(e => e.SensorValues)
            //    .WithOne(e => e.Sensor)
            //    .HasForeignKey(e => e.SensorId)
            //    .IsRequired();

            //modelBuilder.Entity<Device>(b =>
            //{
            //    b.HasOne<IdentityUser<Guid>>()
            //        .WithMany()
            //        .HasForeignKey(c => c.CreatedByUserId);
            //});

            //modelBuilder.Entity<Sensor>(b =>
            //{
            //    b.HasOne<IdentityUser<Guid>>()
            //        .WithMany()
            //        .HasForeignKey(c => c.CreatedByUserId);
            //});

            /* Revert this once we create system user on DB level
            modelBuilder.Entity<SensorValue>(b =>
            {
                b.HasOne<IdentityUser<Guid>>()
                    .WithMany()
                    .HasForeignKey(c => c.CreatedByUserId);

                b.HasOne<IdentityUser<Guid>>()
                    .WithMany()
                    .HasForeignKey(c => c.UpdatedByUserId);
            });
            */
        }

        public virtual DbSet<Response> Response { get; set; }
    }
}
