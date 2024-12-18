using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Vacancy> Vacancies { get; set; }
    public DbSet<Response> Responses { get; set; }
}