using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NubeSync.Server.Data;
using NubeSync.Service.DTO;

namespace NubeSync.Service.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<TodoItem> TodoItems { get; set; }

        public DbSet<NubeOperation> Operations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // use a different clustered index than the string Id for performance reasons, 
            // this should be done for every table!

            modelBuilder.Entity<NubeOperation>().HasKey(e => e.Id).IsClustered(false);
            modelBuilder.Entity<NubeOperation>().HasIndex(e => e.ClusteredIndex).IsClustered();
            modelBuilder.Entity<NubeOperation>().Property(e => e.ClusteredIndex).ValueGeneratedOnAdd();
            modelBuilder.Entity<NubeOperation>().HasIndex(e => new { e.ItemId, e.Property, e.CreatedAt });
            modelBuilder.Entity<NubeOperation>().HasIndex(e => new { e.ItemId, e.TableName, e.ServerUpdatedAt, e.ProcessingType, e.InstallationId });

            modelBuilder.Entity<TodoItem>().HasKey(e => e.Id).IsClustered(false);
            modelBuilder.Entity<TodoItem>().HasIndex(e => e.ClusteredIndex).IsClustered();
            modelBuilder.Entity<TodoItem>().Property(e => e.ClusteredIndex).ValueGeneratedOnAdd();
            modelBuilder.Entity<TodoItem>().HasIndex(e => new { e.UserId, e.ServerUpdatedAt });
        }
    }
}
