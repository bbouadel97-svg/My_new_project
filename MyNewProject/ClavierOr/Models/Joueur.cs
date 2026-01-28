using System.Collections.ObjectModel;

namespace ClavierOr.Models;

/// <summary>
/// Représente un joueur dans le jeu Clavier d'Or
/// </summary>
public class Joueur
{
    public int Id { get; set; }
    public string Pseudo { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateInscription { get; set; }
    
    // Progression et statistiques
    public int ScoreTotal { get; set; }
    public int NiveauActuel { get; set; }
    public int ExperienceActuelle { get; set; }
    public bool EstClavierOr { get; set; } // A atteint le statut "Clavier d'Or"
    
    // Rôle et avantages
    public int? RoleId { get; set; }
    public Role? Role { get; set; }
    
    // Relations
    public ObservableCollection<Score> Scores { get; set; } = new();
    public ObservableCollection<Joker> Jokers { get; set; } = new();
    
    // Boss vaincus
    public int BossVaincus { get; set; }
    public DateTime? DateDernierBoss { get; set; }

    public Joueur()
    {
        DateInscription = DateTime.Now;
        NiveauActuel = 1;
        ExperienceActuelle = 0;
        EstClavierOr = false;
        BossVaincus = 0;
    }

    public Joueur(string pseudo, string email) : this()
    {
        Pseudo = pseudo;
        Email = email;
    }

    /// <summary>
    /// Ajoute de l'expérience et gère la montée de niveau
    /// </summary>
    public void AjouterExperience(int points)
    {
        ExperienceActuelle += points;
        int experienceRequise = CalculerExperienceRequise();
        
        while (ExperienceActuelle >= experienceRequise)
        {
            ExperienceActuelle -= experienceRequise;
            NiveauActuel++;
            experienceRequise = CalculerExperienceRequise();
        }
        
        // Vérifie si le joueur devient "Clavier d'Or"
        if (NiveauActuel >= 50 && BossVaincus >= 5)
        {
            EstClavierOr = true;
        }
    }

    /// <summary>
    /// Calcule l'expérience requise pour le niveau suivant
    /// </summary>
    private int CalculerExperienceRequise()
    {
        return 100 * NiveauActuel; // Formule simple, peut être ajustée
    }

    /// <summary>
    /// Ajoute un score à l'historique du joueur
    /// </summary>
    public void AjouterScore(Score score)
    {
        Scores.Add(score);
        ScoreTotal += score.Points;
    }

    /// <summary>
    /// Enregistre la victoire contre un boss
    /// </summary>
    public void VaincreBoss()
    {
        BossVaincus++;
        DateDernierBoss = DateTime.Now;
    }
}
