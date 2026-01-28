namespace ClavierOr.Models;

/// <summary>
/// Représente un rôle de développeur dans le jeu
/// </summary>
public class Role
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // Avantages spécifiques du rôle
    public int BonusPoints { get; set; }
    public double MultiplicateurScore { get; set; } = 1.0;
    public string AvantageSpecial { get; set; } = string.Empty;

    // Types de rôles prédéfinis
    public static class Types
    {
        public const string BackEnd = "Back-End";
        public const string FrontEnd = "Front-End";
        public const string FullStack = "Full-Stack";
    }

    public Role()
    {
    }

    public Role(string nom, string description, int bonusPoints, double multiplicateur, string avantageSpecial)
    {
        Nom = nom;
        Description = description;
        BonusPoints = bonusPoints;
        MultiplicateurScore = multiplicateur;
        AvantageSpecial = avantageSpecial;
    }
}
