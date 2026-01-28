namespace ClavierOr.Models;

/// <summary>
/// Représente le score d'une partie
/// </summary>
public class Score
{
    public int Id { get; set; }
    public int JoueurId { get; set; }
    public Joueur? Joueur { get; set; }
    
    public int PartieId { get; set; }
    public Partie? Partie { get; set; }
    
    public int Points { get; set; }
    public int BonnesReponses { get; set; }
    public int MauvaisesReponses { get; set; }
    public TimeSpan TempsTotal { get; set; }
    public DateTime DatePartie { get; set; }
    
    // Statistiques détaillées
    public int JokersUtilises { get; set; }
    public double PourcentageReussite { get; set; }
    public int StreakMaximum { get; set; } // Plus longue série de bonnes réponses
    
    public Score()
    {
        DatePartie = DateTime.Now;
    }

    public Score(int joueurId, int partieId) : this()
    {
        JoueurId = joueurId;
        PartieId = partieId;
    }

    /// <summary>
    /// Calcule le pourcentage de réussite
    /// </summary>
    public void CalculerPourcentage()
    {
        int total = BonnesReponses + MauvaisesReponses;
        if (total > 0)
        {
            PourcentageReussite = (double)BonnesReponses / total * 100;
        }
    }

    /// <summary>
    /// Ajoute des points au score avec multiplicateur potentiel
    /// </summary>
    public void AjouterPoints(int points, double multiplicateur = 1.0)
    {
        Points += (int)(points * multiplicateur);
    }
}
