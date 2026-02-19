using ClavierOr.Models;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace ClavierOr;

public partial class MainWindow : Window
{
    private readonly GameService _gameService = new();
    private Joueur? _currentPlayer;
    private Partie? _currentPartie;
    private Score? _currentScore;
    private Question? _currentQuestion;
    private bool _doublePointsActive;
    private bool _backEndRattrapageUsed;

    public MainWindow()
    {
        InitializeComponent();
        _gameService.InitialiserDonnees();
        ChargerRoles();
        RefreshScoresGrid();
        UpdateUiState();
    }

    private void ChargerRoles()
    {
        RoleComboBox.ItemsSource = _gameService.GetRoles();
        RoleComboBox.DisplayMemberPath = nameof(Role.Nom);
        RoleComboBox.SelectedValuePath = nameof(Role.Id);
        RoleComboBox.SelectedIndex = 0;
    }

    private void NewGameButton_Click(object sender, RoutedEventArgs e)
    {
        var pseudo = PlayerNameTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(pseudo))
        {
            MessageBox.Show("Saisissez un nom de joueur.");
            return;
        }

        if (RoleComboBox.SelectedItem is not Role role)
        {
            MessageBox.Show("Sélectionnez un rôle.");
            return;
        }

        _currentPlayer = _gameService.GetOrCreatePlayer(pseudo, role.Id);
        _currentPartie = _gameService.StartNewPartie(_currentPlayer.Id);
        _currentScore = _gameService.CreateScore(_currentPlayer.Id, _currentPartie.Id);
        _doublePointsActive = false;
        _backEndRattrapageUsed = false;

        AjouterHistoriqueLocal($"Nouvelle partie pour {_currentPlayer.Pseudo}.");
        ChargerQuestionActuelle();
        UpdateUiState();
        FooterTextBlock.Text = "Partie démarrée.";
    }

    private void ResumeGameButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(PlayerNameTextBox.Text))
        {
            MessageBox.Show("Indiquez le pseudo pour reprendre.");
            return;
        }

        var pseudo = PlayerNameTextBox.Text.Trim();
        var player = _gameService.FindPlayerByPseudo(pseudo);
        if (player is null)
        {
            MessageBox.Show("Aucune partie trouvée pour ce joueur.");
            return;
        }

        _currentPlayer = player;
        _currentPartie = _gameService.ResumeOrCreatePartie(player.Id);
        _currentScore = _gameService.ResumeOrCreateScore(player.Id, _currentPartie.Id);
        _currentQuestion = _gameService.GetCurrentQuestion(_currentPartie.Id);

        if (_currentQuestion is null)
        {
            ChargerQuestionActuelle();
        }
        else
        {
            AfficherQuestion(_currentQuestion);
        }

        AjouterHistoriqueLocal("Partie reprise.");
        UpdateUiState();
        FooterTextBlock.Text = "Partie reprise.";
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPlayer is null || _currentPartie is null || _currentScore is null)
        {
            MessageBox.Show("Aucune partie active à enregistrer.");
            return;
        }

        _gameService.PersistProgress(_currentPlayer, _currentPartie, _currentScore);
        AjouterHistoriqueLocal("Progression enregistrée.");
        RefreshScoresGrid();
        FooterTextBlock.Text = "Progression enregistrée.";
    }

    private void FinishButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPlayer is null || _currentPartie is null || _currentScore is null)
        {
            MessageBox.Show("Aucune partie en cours.");
            return;
        }

        _gameService.FinishPartie(_currentPlayer, _currentPartie, _currentScore);
        AjouterHistoriqueLocal("Partie terminée.");
        RefreshScoresGrid();
        MessageBox.Show($"Partie terminée. Score final: {_currentScore.Points}");
        FooterTextBlock.Text = "Partie terminée.";
    }

    private void ChargerQuestionActuelle()
    {
        if (_currentPartie is null)
        {
            return;
        }

        _currentQuestion = _gameService.GetCurrentQuestion(_currentPartie.Id);
        if (_currentQuestion is null)
        {
            MessageBox.Show("Plus de questions. Terminez la partie.");
            return;
        }

        AfficherQuestion(_currentQuestion);
    }

    private void AfficherQuestion(Question question)
    {
        QuestionTextBlock.Text = question.Enonce;
        StatusTextBlock.Text = $"Question {_gameService.GetQuestionIndex(_currentPartie!.Id) + 1}/{_gameService.GetQuestionCount(_currentPartie.Id)} | Difficulté: {question.Difficulte}";
        HintTextBlock.Text = string.Empty;

        AnswersPanel.Children.Clear();
        foreach (var reponse in _gameService.GetReponsesForQuestion(question.Id))
        {
            var button = new Button
            {
                Content = reponse.Texte,
                Tag = reponse,
                Margin = new Thickness(0, 0, 0, 8),
                Height = 34
            };
            button.Click += AnswerButton_Click;
            AnswersPanel.Children.Add(button);
        }
    }

    private void AnswerButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPlayer is null || _currentPartie is null || _currentScore is null || _currentQuestion is null)
        {
            return;
        }

        if (sender is not Button { Tag: Reponse selected })
        {
            return;
        }

        var role = _gameService.GetRoleById(_currentPlayer.RoleId);
        var isCorrect = selected.EstCorrect;

        if (!isCorrect && role?.Nom == "Back-End" && !_backEndRattrapageUsed)
        {
            isCorrect = true;
            _backEndRattrapageUsed = true;
            HintTextBlock.Text = "Rattrapage Back-End utilisé: réponse sauvée automatiquement.";
        }

        if (isCorrect)
        {
            var points = _currentQuestion.PointsAttribues;
            if (_doublePointsActive)
            {
                points *= 2;
                _doublePointsActive = false;
            }

            if (role is not null)
            {
                points += role.BonusPoints;
                points = (int)(points * role.MultiplicateurScore);
            }

            _currentScore.BonnesReponses++;
            _currentScore.Points += points;
            _currentPartie.EnregistrerBonneReponse();
            _currentPlayer.AjouterExperience(15);
            HintTextBlock.Text = "Bonne réponse !";
        }
        else
        {
            _currentScore.MauvaisesReponses++;
            _currentPartie.ReinitialiserStreak();
            HintTextBlock.Text = selected.Explication ?? "Mauvaise réponse.";
        }

        _currentScore.StreakMaximum = Math.Max(_currentScore.StreakMaximum, _currentPartie.MeilleurStreak);
        _currentScore.CalculerPourcentage();

        _gameService.LogAction(_currentPlayer.Id, TypeAction.ScoreEnregistre, $"Réponse à la question {_currentQuestion.Id}.", _currentPartie.Id);

        var hasNext = _gameService.MoveNextQuestion(_currentPartie.Id);
        if (!hasNext)
        {
            QuestionTextBlock.Text = "Quiz terminé. Cliquez sur 'Terminer partie'.";
            AnswersPanel.Children.Clear();
            StatusTextBlock.Text = "Fin des questions";
        }
        else
        {
            ChargerQuestionActuelle();
        }

        UpdateUiState();
    }

    private void RoleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (RoleComboBox.SelectedItem is Role role)
        {
            RoleInfoTextBlock.Text = $"Rôle: {role.Nom} - {role.AvantageSpecial}";
        }

        UpdateUiState();
    }

    private void HintButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentQuestion is null || _currentPlayer is null)
        {
            return;
        }

        var role = _gameService.GetRoleById(_currentPlayer.RoleId);
        if (role?.Nom != "Mobile")
        {
            HintTextBlock.Text = "L'indice est réservé au rôle Mobile.";
            return;
        }

        var hint = _gameService.GenerateHint(_currentQuestion.Id);
        HintTextBlock.Text = $"Indice: {hint}";
    }

    private void SkipButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPlayer is null || _currentPartie is null)
        {
            return;
        }

        var role = _gameService.GetRoleById(_currentPlayer.RoleId);
        if (role?.Nom != "Front-End")
        {
            HintTextBlock.Text = "Le changement de question est réservé au rôle Front-End.";
            return;
        }

        if (_gameService.MoveNextQuestion(_currentPartie.Id))
        {
            ChargerQuestionActuelle();
            HintTextBlock.Text = "Question changée grâce au bonus Front-End.";
        }
        else
        {
            HintTextBlock.Text = "Aucune autre question disponible.";
        }
    }

    private void DoubleButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPlayer is null)
        {
            return;
        }

        _doublePointsActive = true;
        HintTextBlock.Text = "Double points activé pour la prochaine bonne réponse.";
    }

    private void ExportPdfButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPlayer is null)
        {
            MessageBox.Show("Démarrez une partie pour exporter votre score.");
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "Fichier PDF (*.pdf)|*.pdf",
            FileName = $"score_{_currentPlayer.Pseudo}_{DateTime.Now:yyyyMMdd_HHmm}.pdf"
        };

        if (dialog.ShowDialog() == true)
        {
            var savedPath = _gameService.ExportLastScorePdf(_currentPlayer.Id, dialog.FileName);
            MessageBox.Show($"PDF exporté: {savedPath}");
            FooterTextBlock.Text = "PDF exporté avec succès.";
        }
    }

    private void AjouterHistoriqueLocal(string texte)
    {
        HistoryListBox.Items.Insert(0, $"{DateTime.Now:HH:mm:ss} - {texte}");
    }

    private void RefreshScoresGrid()
    {
        ScoresDataGrid.ItemsSource = _gameService.GetScoresWithPlayers();
    }

    private void UpdateUiState()
    {
        var hasGame = _currentPlayer is not null && _currentPartie is not null && _currentScore is not null;

        PlayerInfoTextBlock.Text = _currentPlayer is null
            ? "Joueur: -"
            : $"Joueur: {_currentPlayer.Pseudo} | Niveau: {_currentPlayer.NiveauActuel} | XP: {_currentPlayer.ExperienceActuelle}";

        ScoreInfoTextBlock.Text = _currentScore is null
            ? "Score: -"
            : $"Score: {_currentScore.Points} | Bonnes: {_currentScore.BonnesReponses} | Mauvaises: {_currentScore.MauvaisesReponses} | Réussite: {_currentScore.PourcentageReussite:F0}%";

        if (!hasGame)
        {
            StatusTextBlock.Text = "Prêt";
        }

        var selectedRole = RoleComboBox.SelectedItem as Role;
        var roleName = selectedRole?.Nom ?? string.Empty;
        HintButton.IsEnabled = hasGame && roleName == "Mobile";
        SkipButton.IsEnabled = hasGame && roleName == "Front-End";
        DoubleButton.IsEnabled = hasGame;
        SaveButton.IsEnabled = hasGame;
        FinishButton.IsEnabled = hasGame;
        ExportPdfButton.IsEnabled = hasGame;
    }
}
