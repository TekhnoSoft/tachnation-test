using Microsoft.EntityFrameworkCore;
using TechNationTesteApp.Models;

namespace TechNationTesteApp.Configs
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<NotaFiscal> NotasFiscais { get; set; }
    }
}
