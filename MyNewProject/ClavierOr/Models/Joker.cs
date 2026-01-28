namespace ClavierOr.Models;

/// <summary>
/// Représente un joker que le joueur peut utiliser
/// </summary>
public class Joker
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Cout { get; set; } // Coût en points ou autre ressource
    public bool EstUtilise { get; set; }
    public TypeJoker Type { get; set; }

    public Joker()
    {
    }

    public Joker(string nom, string description, TypeJoker type, int cout = 0)
    {
        Nom = nom;
        Description = description;
        Type = type;
        Cout = cout;
        EstUtilise = false;
    }

    /// <summary>
    /// Utilise le joker
    /// </summary>
    public bool Utiliser()
    {
        if (EstUtilise)
            return false;

        EstUtilise = true;
        return true;
    }

    /// <summary>
    /// Réinitialise le joker pour une nouvelle partie
    /// </summary>
    public void Reinitialiser()
    {
        EstUtilise = false;
    }
}

/// <summary>
/// Types de jokers disponibles
/// </summary>
public enum TypeJoker
{
    AideFiftyFifty,      // Élimine 50% des mauvaises réponses
    AppelAmi,            // Aide d'un ami (suggestion)
    TempsSupplementaire, // Temps additionnel
    DoublePoints,        // Double les points de la question
    PasserQuestion       // Passer à la question suivante
}
