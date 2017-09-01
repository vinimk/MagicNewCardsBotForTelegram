using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Text;

namespace MagicBot
{
    public class SpoilDbContext : DbContext
    {
        public DbSet<SpoilItem> Spoils { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=spoils.db");
            this.Database.EnsureCreated();
        }
    }

} 