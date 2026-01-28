using System.Collections.ObjectModel;

namespace ClavierOr.Models;

/// <summary>
/// Représente une partie du jeu
/// </summary>
public class Partie
{
    public int Id { get; set; }
    public DateTime DateDebut { get; set; }
    public DateTime? DateFin { get; set; }
    public EtatPartie Etat { get; set; }
    public ModeJeu Mode { get; set; }
    
    // Questions de la partie
    public ObservableCollection<Question> Questions { get; set; } = new();
    public int QuestionActuelleIndex { get; set; }
    
    // Joueurs et scores
    public ObservableCollection<Joueur> Joueurs { get; set; } = new();
    public ObservableCollection<Score> Scores { get; set; } = new();
    
    // Progression
    public int StreakActuelle { get; set; } // Série de bonnes réponses
    public int MeilleurStreak { get; set; }
    
    public Partie()
    {
        DateDebut = DateTime.Now;
        Etat = EtatPartie.EnAttente;
        QuestionActuelleIndex = 0;
        StreakActuelle = 0;
        MeilleurStreak = 0;
    }

    public Partie(ModeJeu mode) : this()
    {
        Mode = mode;
    }

    /// <summary>
    /// Démarre la partie
    /// </summary>
    public void Demarrer()
    {
        if (Etat == EtatPartie.EnAttente)
        {
            Etat = EtatPartie.EnCours;
            DateDebut = DateTime.Now;
        }
    }

    /// <summary>
    /// Termine la partie
    /// </summary>
    public void Terminer()
    {
        if (Etat == EtatPartie.EnCours)
        {
            Etat = EtatPartie.Terminee;
            DateFin = DateTime.Now;
        }
    }

    /// <summary>
    /// Passe à la question suivante
    /// </summary>
    public bool QuestionSuivante()
    {
        if (QuestionActuelleIndex < Questions.Count - 1)
        {
            QuestionActuelleIndex++;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Obtient la question actuelle
    /// </summary>
    public Question? ObtenirQuestionActuelle()
    {
        if (QuestionActuelleIndex >= 0 && QuestionActuelleIndex < Questions.Count)
        {
            return Questions[QuestionActuelleIndex];
        }
        return null;
    }

    /// <summary>
    /// Enregistre une bonne réponse
    /// </summary>
    public void EnregistrerBonneReponse()
    {
        StreakActuelle++;
        if (StreakActuelle > MeilleurStreak)
        {
            MeilleurStreak = StreakActuelle;
        }
    }

    /// <summary>
    /// Réinitialise le streak suite à une mauvaise réponse
    /// </summary>
    public void ReinitialiserStreak()
    {
        StreakActuelle = 0;
    }

    /// <summary>
    /// Met la partie en pause
    /// </summary>
    public void Pause()
    {
        if (Etat == EtatPartie.EnCours)
        {
            Etat = EtatPartie.EnPause;
        }
    }

    /// <summary>
    /// Reprend la partie
    /// </summary>
    public void Reprendre()
    {
        if (Etat == EtatPartie.EnPause)
        {
            Etat = EtatPartie.EnCours;
        }
    }
}

/// <summary>
/// États possibles d'une partie
/// </summary>
public enum EtatPartie
{
    EnAttente,
    EnCours,
    EnPause,
    Terminee,
    Abandonnee
}

/// <summary>
/// Modes de jeu disponibles
/// </summary>
public enum ModeJeu
{
    NouvellePartie,
    Continuer,
    ModeBoss,
    Entrainement,
    Rejouer
}
