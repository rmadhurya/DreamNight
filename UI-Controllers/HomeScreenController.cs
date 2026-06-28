using UnityEngine;
using UnityEngine.UI;

public class HomeScreenController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject homeScreenPanel;
    public Button playButton;

    void Awake()
    {
        if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);
    }

    void Start()
    {
        // The home screen covers everything until the player presses Play.
        // The board can already be spawned underneath it - it's just locked
        // from input via GameManager.CanFlipTiles() until StartGame() runs.
        if (homeScreenPanel != null) homeScreenPanel.SetActive(true);
    }

    public void Show()
    {
        if (homeScreenPanel != null) homeScreenPanel.SetActive(true);
    }

    public void Hide()
    {
        if (homeScreenPanel != null) homeScreenPanel.SetActive(false);
    }

    private void OnPlayClicked()
    {
        Hide();
        GameManager.Instance.StartGame();
    }
}