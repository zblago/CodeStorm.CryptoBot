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

            modelBuilder.Entity<Ohlc>().Property(p => p.Open).HasPrecision(10, 6);
            modelBuilder.Entity<Ohlc>().Property(p => p.High).HasPrecision(10, 6);
            modelBuilder.Entity<Ohlc>().Property(p => p.Low).HasPrecision(10, 6);
            modelBuilder.Entity<Ohlc>().Property(p => p.Close).HasPrecision(10, 6);
            modelBuilder.Entity<Ohlc>().Property(p => p.VWap).HasPrecision(10, 6);
            modelBuilder.Entity<Ohlc>().Property(p => p.Volume).HasPrecision(10, 6);

            modelBuilder.Entity<TimelineAnalysis>().Property(p => p.K).HasPrecision(10, 2);
            modelBuilder.Entity<TimelineAnalysis>().Property(p => p.D).HasPrecision(10, 2);
        }

        public virtual DbSet<Response> Response { get; set; }
        public virtual DbSet<Ohlc> Ohlcs { get; set; }
        public virtual DbSet<TimelineAnalysis> TimelineAnalysis { get; set; }
    }
}
