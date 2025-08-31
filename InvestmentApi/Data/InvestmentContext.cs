using Microsoft.EntityFrameworkCore;
using InvestmentApi.Models;

namespace InvestmentApi.Data
{
    public class InvestmentContext : DbContext
    {
        public InvestmentContext(DbContextOptions<InvestmentContext> options)
            : base(options) { }

        public DbSet<Investment> Investments => Set<Investment>();
    }
}
