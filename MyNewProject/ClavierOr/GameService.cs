using ClavierOr.Models;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ClavierOr;

public class GameService
{
    public void InitialiserDonnees()
    {
        using var db = new ClavierOrContext();

        if (!db.Set<Role>().Any())
        {
            db.Set<Role>().AddRange(new[]
            {
                new Role("Front-End", "Spécialiste interface", 3, 1.0, "Peut changer de question + bonus de fluidité UI"),
                new Role("Back-End", "Spécialiste logique serveur", 5, 1.0, "Rattrapage automatique une fois + robustesse"),
                new Role("Mobile", "Spécialiste mobile", 2, 1.05, "Accès aux indices + multiplicateur léger")
            });
        }

        if (!db.Questions.Any())
        {
            var q1 = new Question("Quel principe POO permet de cacher l'implémentation ?", DifficulteQuestion.Facile, CategorieQuestion.POO)
            {
                Reponses =
                {
                    new Reponse("Encapsulation", true, "L'encapsulation protège l'état interne."),
                    new Reponse("Héritage", false),
                    new Reponse("Polymorphisme", false),
                    new Reponse("Abstraction uniquement", false)
                }
            };

            var q2 = new Question("Quel mot-clé C# permet de définir une classe héritée ?", DifficulteQuestion.Facile, CategorieQuestion.POO)
            {
                Reponses =
                {
                    new Reponse(":", true, "On utilise ':' pour hériter en C#."),
                    new Reponse("inherits", false),
                    new Reponse("extends", false),
                    new Reponse("->", false)
                }
            };

            var q3 = new Question("Quelle commande Git combine fetch et merge ?", DifficulteQuestion.Moyen, CategorieQuestion.Git)
            {
                Reponses =
                {
                    new Reponse("git pull", true, "git pull = fetch + merge/rebase selon config."),
                    new Reponse("git stash", false),
                    new Reponse("git clone", false),
                    new Reponse("git reset", false)
                }
            };

            var q4 = new Question("En SQL, quelle clause filtre les groupes agrégés ?", DifficulteQuestion.Moyen, CategorieQuestion.BaseDeDonnees)
            {
                Reponses =
                {
                    new Reponse("HAVING", true, "HAVING agit après GROUP BY."),
                    new Reponse("WHERE", false),
                    new Reponse("ORDER BY", false),
                    new Reponse("LIMIT", false)
                }
            };

            var q5 = new Question("Quel pattern évite de créer plusieurs instances globales d'une classe ?", DifficulteQuestion.Difficile, CategorieQuestion.Architecture)
            {
                Reponses =
                {
                    new Reponse("Singleton", true),
                    new Reponse("Factory", false),
                    new Reponse("Adapter", false),
                    new Reponse("Builder", false)
                }
            };

            var q6 = new Question("Lors d'une attaque XSS, quel mécanisme est prioritaire ?", DifficulteQuestion.Boss, CategorieQuestion.Securite)
            {
                Reponses =
                {
                    new Reponse("Encoder/échapper la sortie HTML", true, "L'encodage output est une défense clé contre XSS."),
                    new Reponse("Désactiver JavaScript", false),
                    new Reponse("Changer le port HTTP", false),
                    new Reponse("Compresser les scripts", false)
                }
            };

            db.Questions.AddRange(q1, q2, q3, q4, q5, q6);
        }

        db.SaveChanges();
    }

    public List<Role> GetRoles()
    {
        using var db = new ClavierOrContext();
        return db.Set<Role>().OrderBy(r => r.Nom).ToList();
    }

    public Role? GetRoleById(int? roleId)
    {
        if (roleId is null)
        {
            return null;
        }

        using var db = new ClavierOrContext();
        return db.Set<Role>().FirstOrDefault(r => r.Id == roleId.Value);
    }

    public Joueur GetOrCreatePlayer(string pseudo, int roleId)
    {
        using var db = new ClavierOrContext();

        var joueur = db.Joueurs.FirstOrDefault(j => j.Pseudo == pseudo);
        if (joueur is null)
        {
            joueur = new Joueur(pseudo, $"{pseudo.ToLowerInvariant()}@clavieror.local")
            {
                RoleId = roleId
            };
            db.Joueurs.Add(joueur);
        }
        else
        {
            joueur.RoleId = roleId;
        }

        db.SaveChanges();
        return joueur;
    }

    public Joueur? FindPlayerByPseudo(string pseudo)
    {
        using var db = new ClavierOrContext();
        return db.Joueurs.FirstOrDefault(j => j.Pseudo == pseudo);
    }

    public Partie StartNewPartie(int joueurId)
    {
        using var db = new ClavierOrContext();

        var partie = new Partie(ModeJeu.NouvellePartie)
        {
            Etat = EtatPartie.EnCours,
            DateDebut = DateTime.Now,
            QuestionActuelleIndex = 0
        };

        var joueur = db.Joueurs.First(j => j.Id == joueurId);
        partie.Joueurs.Add(joueur);

        db.Parties.Add(partie);
        db.SaveChanges();

        LogAction(joueurId, TypeAction.NouvellePartie, "Nouvelle partie démarrée", partie.Id);
        return partie;
    }

    public Partie ResumeOrCreatePartie(int joueurId)
    {
        using var db = new ClavierOrContext();
        var partie = db.Parties
            .Include(p => p.Joueurs)
            .Where(p => p.Etat == EtatPartie.EnCours || p.Etat == EtatPartie.EnPause)
            .OrderByDescending(p => p.DateDebut)
            .FirstOrDefault(p => p.Joueurs.Any(j => j.Id == joueurId));

        if (partie is null)
        {
            return StartNewPartie(joueurId);
        }

        if (partie.Etat == EtatPartie.EnPause)
        {
            partie.Reprendre();
            db.SaveChanges();
        }

        return partie;
    }

    public Score CreateScore(int joueurId, int partieId)
    {
        using var db = new ClavierOrContext();

        var score = new Score(joueurId, partieId)
        {
            DatePartie = DateTime.Now,
            TempsTotal = TimeSpan.Zero
        };

        db.Scores.Add(score);
        db.SaveChanges();

        return score;
    }

    public Score ResumeOrCreateScore(int joueurId, int partieId)
    {
        using var db = new ClavierOrContext();
        var score = db.Scores
            .OrderByDescending(s => s.DatePartie)
            .FirstOrDefault(s => s.JoueurId == joueurId && s.PartieId == partieId);

        return score ?? CreateScore(joueurId, partieId);
    }

    public Question? GetCurrentQuestion(int partieId)
    {
        using var db = new ClavierOrContext();

        var partie = db.Parties.FirstOrDefault(p => p.Id == partieId);

        if (partie is null)
        {
            return null;
        }

        var totalQuestions = db.Questions.Count();
        if (totalQuestions == 0)
        {
            return null;
        }

        if (partie.QuestionActuelleIndex < 0 || partie.QuestionActuelleIndex >= totalQuestions)
        {
            return null;
        }

        return db.Questions
            .OrderBy(q => q.Id)
            .Skip(partie.QuestionActuelleIndex)
            .FirstOrDefault();
    }

    public int GetQuestionIndex(int partieId)
    {
        using var db = new ClavierOrContext();
        return db.Parties.Where(p => p.Id == partieId).Select(p => p.QuestionActuelleIndex).FirstOrDefault();
    }

    public int GetQuestionCount(int partieId)
    {
        using var db = new ClavierOrContext();
        return db.Questions.Count();
    }

    public List<Reponse> GetReponsesForQuestion(int questionId)
    {
        using var db = new ClavierOrContext();
        return db.Reponses.Where(r => r.QuestionId == questionId).OrderBy(r => r.Id).ToList();
    }

    public bool MoveNextQuestion(int partieId)
    {
        using var db = new ClavierOrContext();
        var partie = db.Parties.FirstOrDefault(p => p.Id == partieId);
        if (partie is null)
        {
            return false;
        }

        var questionCount = db.Questions.Count();
        if (partie.QuestionActuelleIndex + 1 >= questionCount)
        {
            return false;
        }

        partie.QuestionActuelleIndex++;
        db.SaveChanges();
        return true;
    }

    public void PersistProgress(Joueur joueur, Partie partie, Score score)
    {
        using var db = new ClavierOrContext();

        db.Joueurs.Update(joueur);
        db.Parties.Update(partie);
        db.Scores.Update(score);
        db.SaveChanges();

        LogAction(joueur.Id, TypeAction.ScoreEnregistre, "Progression enregistrée", partie.Id);
    }

    public void FinishPartie(Joueur joueur, Partie partie, Score score)
    {
        using var db = new ClavierOrContext();

        partie.Terminer();
        score.DatePartie = DateTime.Now;
        score.CalculerPourcentage();

        joueur.AjouterScore(score);
        if (score.PourcentageReussite >= 70)
        {
            joueur.VaincreBoss();
            LogAction(joueur.Id, TypeAction.BossVaincu, "Boss battu grâce à une bonne réussite.", partie.Id);
        }

        if (joueur.EstClavierOr)
        {
            LogAction(joueur.Id, TypeAction.ClavierOrObtenu, "Titre Clavier d'Or atteint !", partie.Id);
        }

        db.Joueurs.Update(joueur);
        db.Parties.Update(partie);
        db.Scores.Update(score);
        db.SaveChanges();

        LogAction(joueur.Id, TypeAction.PartieTerminee, $"Partie terminée avec {score.Points} points", partie.Id);
    }

    public void LogAction(int joueurId, TypeAction action, string description, int? partieId = null)
    {
        using var db = new ClavierOrContext();
        db.Historiques.Add(new Historique(joueurId, action, description)
        {
            PartieId = partieId,
            DateAction = DateTime.Now
        });
        db.SaveChanges();
    }

    public List<Score> GetScoresWithPlayers()
    {
        using var db = new ClavierOrContext();
        return db.Scores
            .Include(s => s.Joueur)
            .OrderByDescending(s => s.DatePartie)
            .Take(50)
            .ToList();
    }

    public string GenerateHint(int questionId)
    {
        using var db = new ClavierOrContext();
        var question = db.Questions.FirstOrDefault(q => q.Id == questionId);
        if (question is null)
        {
            return "Question introuvable.";
        }

        var correct = db.Reponses.FirstOrDefault(r => r.QuestionId == questionId && r.EstCorrect);
        if (correct is null)
        {
            return "Aucun indice disponible.";
        }

        if (correct.Texte.Length <= 3)
        {
            return $"La réponse commence par: {correct.Texte}";
        }

        return $"La réponse commence par: {correct.Texte[..3]}...";
    }

    public string ExportLastScorePdf(int joueurId, string outputPath)
    {
        using var db = new ClavierOrContext();
        var score = db.Scores
            .Include(s => s.Joueur)
            .Include(s => s.Partie)
            .Where(s => s.JoueurId == joueurId)
            .OrderByDescending(s => s.DatePartie)
            .FirstOrDefault();

        if (score is null)
        {
            throw new InvalidOperationException("Aucun score à exporter.");
        }

        var roleName = db.Set<Role>()
            .Where(r => r.Id == score.Joueur!.RoleId)
            .Select(r => r.Nom)
            .FirstOrDefault() ?? "-";

        QuestPDF.Settings.License = LicenseType.Community;

        Document
            .Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(32);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Content().Column(column =>
                    {
                        column.Spacing(8);

                        column.Item()
                            .Text("Clavier d'Or - Rapport de score")
                            .Bold()
                            .FontSize(20)
                            .FontColor(Colors.Blue.Darken2);

                        column.Item().Text($"Joueur: {score.Joueur!.Pseudo}").Bold();
                        column.Item().Text($"Rôle: {roleName}");
                        column.Item().Text($"Date: {score.DatePartie:dd/MM/yyyy HH:mm}");

                        column.Item().PaddingVertical(6).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);

                        column.Item().Text($"Points: {score.Points}").Bold().FontSize(14);
                        column.Item().Text($"Bonnes réponses: {score.BonnesReponses}");
                        column.Item().Text($"Mauvaises réponses: {score.MauvaisesReponses}");
                        column.Item().Text($"Réussite: {score.PourcentageReussite:F1}%");
                        column.Item().Text($"Streak max: {score.StreakMaximum}");

                        column.Item().PaddingTop(20).Text("Export généré automatiquement par Clavier d'Or.").Italic().FontSize(9).FontColor(Colors.Grey.Darken1);
                    });
                });
            })
            .GeneratePdf(outputPath);

        return outputPath;
    }
}
