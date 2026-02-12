using ClavierOr.Models;
using Microsoft.EntityFrameworkCore;

namespace ClavierOr;

public class ClavierOrContext : DbContext
{
    public ClavierOrContext()
    {
    }

    public ClavierOrContext(DbContextOptions<ClavierOrContext> options)
        : base(options)
    {
    }

    public DbSet<Joueur> Joueurs { get; set; }
    public DbSet<Partie> Parties { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Reponse> Reponses { get; set; }
    public DbSet<Score> Scores { get; set; }
    public DbSet<Joker> Jokers { get; set; }
    public DbSet<Historique> Historiques { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!options.IsConfigured)
        {
            options.UseSqlite("Data Source=clavieror.db");
        }
    }
}
