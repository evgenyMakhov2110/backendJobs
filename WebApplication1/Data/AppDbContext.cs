using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Vacancy> Vacancies { get; set; }
    public DbSet<Response> Responses { get; set; }
    public DbSet<User> Users { get; set; }
}