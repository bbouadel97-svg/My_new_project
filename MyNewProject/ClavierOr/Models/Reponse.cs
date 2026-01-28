namespace ClavierOr.Models;

/// <summary>
/// Représente une réponse possible à une question
/// </summary>
public class Reponse
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public Question? Question { get; set; }
    
    public string Texte { get; set; } = string.Empty;
    public bool EstCorrect { get; set; }
    public string? Explication { get; set; } // Explication de la réponse
    
    public Reponse()
    {
    }

    public Reponse(string texte, bool estCorrect, string? explication = null)
    {
        Texte = texte;
        EstCorrect = estCorrect;
        Explication = explication;
    }
}
