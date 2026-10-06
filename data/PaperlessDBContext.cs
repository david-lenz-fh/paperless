using data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace data
{
    public class PaperlessDbContext : DbContext
    {
        public PaperlessDbContext(DbContextOptions<PaperlessDbContext> options):base(options)
        {
        }

        public DbSet<Document> Documents => Set<Document>();
        public DbSet<FileVersion> FileVersions => Set<FileVersion>();
        public DbSet<DocumentCategory> DocumentCategories => Set<DocumentCategory>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FileVersion>()
                .HasOne(f => f.Document)
                .WithMany()
                .HasForeignKey(f => f.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}   
