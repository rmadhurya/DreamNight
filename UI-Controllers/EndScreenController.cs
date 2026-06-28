using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EndScreenController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject endScreenPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI breakdownText;
    public GameObject leaderboardPanel; // assign your leaderboard list UI here

    [Header("Buttons")]
    public Button playAgainButton;
    public Button homeButton;

    [Header("Home Screen Link")]
    public HomeScreenController homeScreenController; // assign the home screen object here

    void Awake()
    {
        if (playAgainButton != null) playAgainButton.onClick.AddListener(OnPlayAgainClicked);
        if (homeButton != null) homeButton.onClick.AddListener(OnHomeClicked);
    }

    public void ShowWin(int finalScore, int matchesFound, int totalMatches, int completionBonus, int timeBonus)
    {
        endScreenPanel.SetActive(true);
        titleText.text = "You win!";
        scoreText.text = finalScore.ToString("N0");
        breakdownText.text = $"{matchesFound}/{totalMatches} matches  +{completionBonus} clear bonus  +{timeBonus} speed bonus";

        // TODO: submit finalScore to your Talo leaderboard here, then refresh leaderboardPanel
    }

    public void ShowGameOver(int finalScore, int matchesFound, int totalMatches)
    {
        endScreenPanel.SetActive(true);
        titleText.text = "Caught by the evil pair";
        scoreText.text = finalScore.ToString("N0");
        breakdownText.text = $"{matchesFound}/{totalMatches} matches found before the evil pair matched";

        // TODO: submit finalScore to your Talo leaderboard here, then refresh leaderboardPanel
    }

    public void Hide()
    {
        if (endScreenPanel != null) endScreenPanel.SetActive(false);
    }

    private void OnPlayAgainClicked()
    {
        Hide();
        GameManager.Instance.PlayAgain();
    }

    private void OnHomeClicked()
    {
        Hide();
        GameManager.Instance.ReturnToHomeScreen();

        if (homeScreenController != null)
        {
            homeScreenController.Show();
        }
    }
}