using Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure
{
    public class DataContext : IdentityDbContext<Owner>
    {
        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {

        }


        public DbSet<Owner> Owners { get; set; }
       
        public DbSet<PortfolioItem> PortfolioItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

       
            modelBuilder.Entity<PortfolioItem>().Property(x => x.Id).HasDefaultValueSql("NEWID()");

            modelBuilder.Entity<Owner>()
              .ToTable("Owner", "dbo").Property(p => p.Id);

            modelBuilder.Entity<PortfolioItem>()
             .HasOne(O => O.owner)
             .WithMany(PI => PI.portfolioItems)
             .OnDelete(DeleteBehavior.SetNull);
        }

       

    }
}
