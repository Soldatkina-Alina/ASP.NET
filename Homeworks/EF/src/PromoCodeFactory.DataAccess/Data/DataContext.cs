using Microsoft.EntityFrameworkCore;
using PromoCodeFactory.Core.Domain.Administration;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromoCodeFactory.DataAccess.Data
{
    public class DataContext: DbContext
    {
        public DbSet<PromoCode> PromoCodes { get; set; }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<Preference> Preferences { get; set; }

        public DbSet<Role> Roles { get; set; }

        public DbSet<Employee> Employees { get; set; }

        public DbSet<CustomerPreference> CustomerPreferences { get; set; }

        public DataContext()
        {
        }

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Если опции не переданы, используем SQLite файл в рабочем каталоге
            if (!optionsBuilder.IsConfigured)
            {
                var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "app.db");
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Employee - Role one-to-many
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Role)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            // Customer - PromoCode one-to-many
            modelBuilder.Entity<Customer>()
                .HasMany(c => c.PromoCodes)           // У Customer есть коллекция PromoCodes
                .WithOne(pc => pc.Customer)           // У PromoCode есть свойство Customer
                .OnDelete(DeleteBehavior.Cascade);    

            // CustomerPreference many-to-many
            modelBuilder.Entity<CustomerPreference>()
                .HasKey(cp => new { cp.CustomerId, cp.PreferenceId });

            modelBuilder.Entity<CustomerPreference>()
                .HasOne(cp => cp.Customer)
                .WithMany(c => c.Preferences)
                .HasForeignKey(cp => cp.CustomerId);

            modelBuilder.Entity<CustomerPreference>()
                .HasOne(cp => cp.Preference)
                .WithMany()
                .HasForeignKey(cp => cp.PreferenceId);

            // PromoCode - Preference one-to-many
            modelBuilder.Entity<PromoCode>()
                .HasOne(pc => pc.Preference)
                .WithMany()
                .OnDelete(DeleteBehavior.Cascade);


            // Ограничения MaxLength для строк
            modelBuilder.Entity<Role>(b =>
            {
                b.Property(r => r.Name).HasMaxLength(100).IsRequired();
                b.Property(r => r.Description).HasMaxLength(500);
            });

            modelBuilder.Entity<Employee>(b =>
            {
                b.Property(e => e.FirstName).HasMaxLength(100);
                b.Property(e => e.LastName).HasMaxLength(100);
                b.Property(e => e.Email).HasMaxLength(200);
            });

            modelBuilder.Entity<Customer>(b =>
            {
                b.Property(c => c.FirstName).HasMaxLength(100);
                b.Property(c => c.LastName).HasMaxLength(100);
                b.Property(c => c.Email).HasMaxLength(200);
            });

            modelBuilder.Entity<Preference>(b =>
            {
                b.Property(p => p.Name).HasMaxLength(200).IsRequired();
            });

            modelBuilder.Entity<PromoCode>(b =>
            {
                b.Property(p => p.Code).HasMaxLength(100).IsRequired();
                b.Property(p => p.ServiceInfo).HasMaxLength(500);
                b.Property(p => p.PartnerName).HasMaxLength(200);
            });
        }
    }
}
