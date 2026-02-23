using ClavierOr.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace ClavierOr;

public class ClavierOrContext : DbContext
{
    public static string GetDatabasePath()
    {
        var solutionRoot = FindSolutionRoot(AppContext.BaseDirectory)
            ?? FindSolutionRoot(Directory.GetCurrentDirectory());

        if (solutionRoot is not null)
        {
            var projectDbDir = Path.Combine(solutionRoot, "MyNewProject", "ClavierOr");
            Directory.CreateDirectory(projectDbDir);
            return Path.Combine(projectDbDir, "clavieror.db");
        }

        var fallbackDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ClavierOr");
        Directory.CreateDirectory(fallbackDir);
        return Path.Combine(fallbackDir, "clavieror.db");
    }

    public static void ConfigureSqlite(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={GetDatabasePath()}");
    }

    private static string? FindSolutionRoot(string startPath)
    {
        var current = new DirectoryInfo(startPath);

        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "MyNewProject.sln")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        return null;
    }

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
            ConfigureSqlite(options);
        }
    }
}
