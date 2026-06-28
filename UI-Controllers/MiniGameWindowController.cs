using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class MiniGameEntry
{
    [Tooltip("Must exactly match the good tile prefab's GameObject name, e.g. 'candle'")]
    public string tileTypeName;

    [Tooltip("Icon shown in the popup while the player decides to press Play Game")]
    public Sprite windowIcon;

    [Tooltip("Prefab containing your 2D mini-game UI plus a script inheriting from MiniGameBase")]
    public GameObject miniGamePrefab;
}

public class MiniGameWindowController : MonoBehaviour
{
    [Header("Window UI")]
    public RectTransform windowPanel;        // the popup frame itself (starts hidden/scaled down)
    public CanvasGroup windowCanvasGroup;     // for fade in/out
    public Transform miniGameContentParent;   // mini-game prefab instantiates inside here
    public Image windowIconImage;             // shows MiniGameEntry.windowIcon

    [Header("Pre-Game Screen")]
    public GameObject preGamePanel;  // contains the icon + Play Game button, shown before the mini-game starts
    public Button playGameButton;

    [Header("Mini-Games")]
    public List<MiniGameEntry> miniGames = new List<MiniGameEntry>();

    [Header("Timing")]
    public float openDuration = 0.25f;
    public float closeDuration = 0.2f;
    public AnimationCurve openCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private GameObject activeMiniGameInstance;
    private bool playButtonPressed = false;

    void Awake()
    {
        if (windowCanvasGroup != null) windowCanvasGroup.alpha = 0f;
        if (windowPanel != null) windowPanel.localScale = Vector3.one * 0.85f;
        if (windowPanel != null) windowPanel.gameObject.SetActive(false);

        if (playGameButton != null)
        {
            playGameButton.onClick.AddListener(() => playButtonPressed = true);
        }
    }

    // Call this from GameManager when a good match is found.
    // tileType should be the tile's name with "(Clone)" already stripped.
    public void TriggerMiniGame(string tileType, Action<float> onComplete)
    {
        MiniGameEntry entry = miniGames.Find(e => e.tileTypeName == tileType);

        if (entry == null)
        {
            Debug.LogWarning($"MiniGameWindowController: no entry configured for '{tileType}'. Skipping mini-game, awarding 0% performance.");
            onComplete?.Invoke(0f);
            return;
        }

        StartCoroutine(RunMiniGameSequence(entry, onComplete));
    }

    private IEnumerator RunMiniGameSequence(MiniGameEntry entry, Action<float> onComplete)
    {
        // Show the icon + Play Game button first
        if (windowIconImage != null)
        {
            windowIconImage.sprite = entry.windowIcon;
            windowIconImage.enabled = entry.windowIcon != null;
        }

        if (preGamePanel != null) preGamePanel.SetActive(true);
        playButtonPressed = false;

        windowPanel.gameObject.SetActive(true);
        yield return AnimateWindow(open: true);

        // Wait until the player presses "Play Game"
        while (!playButtonPressed)
        {
            yield return null;
        }

        if (preGamePanel != null) preGamePanel.SetActive(false);

        // Now actually launch the mini-game content
        activeMiniGameInstance = Instantiate(entry.miniGamePrefab, miniGameContentParent);
        MiniGameBase miniGame = activeMiniGameInstance.GetComponent<MiniGameBase>();

        if (miniGame == null)
        {
            Debug.LogError($"Mini-game prefab for '{entry.tileTypeName}' has no MiniGameBase component!");
            Destroy(activeMiniGameInstance);
            activeMiniGameInstance = null;
            yield return AnimateWindow(open: false);
            windowPanel.gameObject.SetActive(false);
            onComplete?.Invoke(0f);
            yield break;
        }

        float result = -1f;
        miniGame.OnMiniGameFinished += score => result = score;
        miniGame.StartMiniGame();

        while (result < 0f)
        {
            yield return null;
        }

        Destroy(activeMiniGameInstance);
        activeMiniGameInstance = null;

        yield return AnimateWindow(open: false);
        windowPanel.gameObject.SetActive(false);

        onComplete?.Invoke(result);
    }

    private IEnumerator AnimateWindow(bool open)
    {
        float duration = open ? openDuration : closeDuration;
        float startAlpha = open ? 0f : 1f;
        float endAlpha = open ? 1f : 0f;
        float startScale = open ? 0.85f : 1f;
        float endScale = open ? 1f : 0.85f;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float curveT = openCurve.Evaluate(Mathf.Clamp01(t));

            if (windowCanvasGroup != null)
                windowCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, curveT);

            if (windowPanel != null)
                windowPanel.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, curveT);

            yield return null;
        }

        if (windowCanvasGroup != null) windowCanvasGroup.alpha = endAlpha;
        if (windowPanel != null) windowPanel.localScale = Vector3.one * endScale;
    }
}