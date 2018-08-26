using Lucent.Common.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lucent.Portal.Data
{
    public class PortalDbContext : DbContext
    {
        public PortalDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Campaign> Campaigns { get; set; }
    }
}