using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WaxWorx.Data.Entities;

namespace WaxWorx.Data
{
    public class InventoryDbContext : DbContext
    {
        public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
            : base(options) { }

        #region Application Core

        public DbSet<ApplicationSetting> ApplicationSettings { get; set; }
        public DbSet<UserSetting> UserSettings { get; set; }
        public DbSet<User> Users { get; set; }
        #endregion

        #region Features Core

        #endregion

        #region Features Custom

        #endregion

        public DbSet<Album> Albums { get; set; }
        public DbSet<Artist> Artists { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Image> Images { get; set; }

        // Add other DbSet properties here

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    // Album to Artist (Many-to-One)
        //    modelBuilder.Entity<Album>()
        //            .HasOne(a => a.Artist)
        //            .WithMany(artist => artist.Albums)
        //            .HasForeignKey(a => a.ArtistId);

        //    // Album to Genre (Many-to-One)
        //    modelBuilder.Entity<Album>()
        //        .HasOne(a => a.Genre)
        //        .WithMany(genre => genre.Albums)
        //        .HasForeignKey(a => a.GenreId);
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var clrType = entityType.ClrType;

                if (typeof(AuditBaseEntity).IsAssignableFrom(clrType))
                {
                    // Default value for CreatedDate
                    modelBuilder.Entity(clrType)
                        .Property(nameof(AuditBaseEntity.CreatedDate))
                        .HasDefaultValueSql("GETUTCDATE()");

                    // Default value for IsActive
                    modelBuilder.Entity(clrType)
                        .Property(nameof(AuditBaseEntity.IsActive))
                        .HasDefaultValue(true);

                    // Global query filter: IsActive == true
                    var parameter = Expression.Parameter(clrType, "e");
                    var property = Expression.Property(parameter, nameof(AuditBaseEntity.IsActive));
                    var filter = Expression.Lambda(Expression.Equal(property, Expression.Constant(true)), parameter);

                    modelBuilder.Entity(clrType).HasQueryFilter(filter);
                }
            }

            // Album relationships
            modelBuilder.Entity<Album>()
                .HasOne(a => a.Artist)
                .WithMany(artist => artist.Albums)
                .HasForeignKey(a => a.ArtistId);

            modelBuilder.Entity<Album>()
                .HasOne(a => a.Genre)
                .WithMany(genre => genre.Albums)
                .HasForeignKey(a => a.GenreId);

            // Ensure primary keys and auto-incrementing IDs for all entities
            modelBuilder.Entity<Album>()
                .Property(a => a.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Artist>()
                .Property(ar => ar.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Genre>()
                .Property(g => g.Id)
                .ValueGeneratedOnAdd();
        }
    }
}
