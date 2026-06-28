using UnityEngine;
using System;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public int scorePerMatch = 100; // base points per good match
    public string[] evilTileNames = {"EvilTile1", "EvilTile2"};
    private List<TileFlipper> revealedEvilTiles = new List<TileFlipper>();

    [Header("Scoring Formula")]
    public int streakIncrement = 20;
    public int streakBonusCap = 100;
    public int miniGameMaxBonus = 150;
    public int completionBonus = 500;
    public int timeBonusMax = 200;
    public float timeBonusTargetSeconds = 180f; // clear under this for max time bonus

    [Header("Mini-Game Window")]
    public MiniGameWindowController miniGameWindow;
    public EndScreenController endScreenController;

    [Header("UI References")]
    public TMPro.TextMeshProUGUI scoreText;
    public TMPro.TextMeshProUGUI gameStatusText;

    private List<TileFlipper> flippedTiles = new List<TileFlipper>();
    private int currentScore = 0;
    private int totalGoodMatches = 7;
    private int foundGoodMatches = 0;
    private int currentStreak = 0;
    private float roundStartTime;
    private bool canFlip = true;
    private bool gameOver = false;
    private bool gameStarted = false;

    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        roundStartTime = Time.time;
        UpdateUI();
    }

    public void RegisterFlippedTile(TileFlipper tile)
    {
        if (!canFlip || gameOver) return;

        if (IsEvilTile(tile))
        {
            HandleEvilTileFlipped(tile);
            revealedEvilTiles.Add(tile);

            if (flippedTiles.Count > 0)
            {
                List<TileFlipper> tilesToFlipBack = new List<TileFlipper>(flippedTiles);
                flippedTiles.Clear();

                foreach (TileFlipper goodTile in tilesToFlipBack)
                {
                    goodTile.FlipBack();
                }

                canFlip = true;
            }

            CheckForEvilPairMatch();
            return;
        }

        flippedTiles.Add(tile);

        if (flippedTiles.Count == 2)
        {
            canFlip = false;
            Invoke(nameof(CheckForMatch), 1f);
        }
    }

    private void CheckForEvilPairMatch()
    {
        if (revealedEvilTiles.Count < 2) return;

        for (int i = 0; i < revealedEvilTiles.Count - 1; i++)
        {
            for (int j = i + 1; j < revealedEvilTiles.Count; j++)
            {
                if (DoTilesMatch(revealedEvilTiles[i], revealedEvilTiles[j]))
                {
                    HandleEvilPairMatch(revealedEvilTiles[i], revealedEvilTiles[j]);
                    return;
                }
            }
        }
    }

    public void UnregisterFlippedTile(TileFlipper tile)
    {
        flippedTiles.Remove(tile);
    }

    private void CheckForMatch()
    {
        if (flippedTiles.Count != 2 || gameOver)
        {
            canFlip = true;
            return;
        }

        TileFlipper tile1 = flippedTiles[0];
        TileFlipper tile2 = flippedTiles[1];

        if (DoTilesMatch(tile1, tile2))
        {
            HandleGoodMatch(tile1, tile2);
        }
        else
        {
            HandleMismatch(tile1, tile2);
            flippedTiles.Clear();
            canFlip = true;
        }
    }

    private bool DoTilesMatch(TileFlipper tile1, TileFlipper tile2)
    {
        string name1 = tile1.gameObject.name.Replace("(Clone)", "").Trim();
        string name2 = tile2.gameObject.name.Replace("(Clone)", "").Trim();
        return string.Equals(name1, name2, StringComparison.OrdinalIgnoreCase);
    }

    private void HandleGoodMatch(TileFlipper tile1, TileFlipper tile2)
    {
        tile1.SetMatched(true);
        tile2.SetMatched(true);

        Vector3 animationStartPos = (tile1.transform.position + tile2.transform.position) * 0.5f;
        string tileType = tile1.gameObject.name.Replace("(Clone)", "").Trim();

        if (CollectionBox.Instance != null)
        {
            CollectionBox.Instance.CollectIcon(tile1, tile2, animationStartPos, tileType);
        }

        currentStreak++;
        foundGoodMatches++;
        flippedTiles.Clear();

        // Board stays locked (canFlip is false) until the mini-game finishes.
        if (miniGameWindow != null)
        {
            miniGameWindow.TriggerMiniGame(tileType, performancePercent => OnMiniGameComplete(performancePercent));
        }
        else
        {
            // No window controller assigned (e.g. testing) - just award base score.
            OnMiniGameComplete(0f);
        }
    }

    private void OnMiniGameComplete(float performancePercent)
    {
        int streakBonus = Mathf.Min((currentStreak - 1) * streakIncrement, streakBonusCap);
        int miniGameBonus = Mathf.RoundToInt(Mathf.Clamp01(performancePercent) * miniGameMaxBonus);
        int matchScore = scorePerMatch + streakBonus + miniGameBonus;

        currentScore += matchScore;

        canFlip = true;
        UpdateUI();
        CheckWinCondition();
    }

    private void HandleEvilTileFlipped(TileFlipper tile)
    {
        tile.SetEvilFlipped(true);
        currentStreak = 0; // an evil reveal breaks the streak bonus

        if (gameStatusText != null)
            gameStatusText.text = "Evil tile revealed! Be careful...";
    }

    private void HandleEvilPairMatch(TileFlipper tile1, TileFlipper tile2)
    {
        gameOver = true;
        canFlip = false;

        tile1.SetMatched(true);
        tile2.SetMatched(true);

        UpdateUI();

        if (endScreenController != null)
        {
            endScreenController.ShowGameOver(currentScore, foundGoodMatches, totalGoodMatches);
        }
    }

    private void HandleMismatch(TileFlipper tile1, TileFlipper tile2)
    {
        tile1.FlipBack();
        tile2.FlipBack();
    }

    private void CheckWinCondition()
    {
        if (foundGoodMatches >= totalGoodMatches && !gameOver)
        {
            gameOver = true;
            canFlip = false;

            float elapsed = Time.time - roundStartTime;
            int timeBonus = Mathf.RoundToInt(Mathf.Clamp01(1f - (elapsed / timeBonusTargetSeconds)) * timeBonusMax);
            currentScore += completionBonus + timeBonus;

            UpdateUI();

            if (endScreenController != null)
            {
                endScreenController.ShowWin(currentScore, foundGoodMatches, totalGoodMatches, completionBonus, timeBonus);
            }
        }
    }

    private bool IsEvilTile(TileFlipper tile)
    {
        string tileName = tile.gameObject.name.Replace("(Clone)", "").Trim();

        foreach (string evilName in evilTileNames)
        {
            if (string.Equals(tileName, evilName.Trim(), StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + currentScore;

        if (gameStatusText != null && !gameOver)
            gameStatusText.text = "Good Matches: " + foundGoodMatches + "/" + totalGoodMatches;
    }

    public void ResetGame()
    {
        currentScore = 0;
        foundGoodMatches = 0;
        currentStreak = 0;
        roundStartTime = Time.time;
        flippedTiles.Clear();
        revealedEvilTiles.Clear();
        canFlip = true;
        gameOver = false;

        if (CollectionBox.Instance != null)
        {
            CollectionBox.Instance.ResetCollection();
        }

        TileSpawner spawner = FindObjectOfType<TileSpawner>();
        if (spawner != null)
        {
            spawner.RespawnTiles();
        }

        UpdateUI();
    }

    public bool CanFlipTiles()
    {
        return gameStarted && canFlip && !gameOver;
    }

    // Called by HomeScreenController's Play button
    public void StartGame()
    {
        gameStarted = true;
    }

    // Called by EndScreenController's Play Again button
    public void PlayAgain()
    {
        ResetGame();
    }

    // Called by EndScreenController's Home Screen button
    public void ReturnToHomeScreen()
    {
        gameStarted = false;
        ResetGame();
    }
}