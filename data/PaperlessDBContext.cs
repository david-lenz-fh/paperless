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
        public DbSet<DocumentFile> DocumentFiles => Set<DocumentFile>();

    }
}   
