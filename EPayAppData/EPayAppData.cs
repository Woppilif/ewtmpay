using System;
using EPayAppData.Models;
using Microsoft.EntityFrameworkCore;

namespace EPayAppData
{
    public class EPayAppData : DbContext
    {
        public DbSet<Payments> Payments { get; set; }
        public EPayAppData(DbContextOptions<EPayAppData> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
