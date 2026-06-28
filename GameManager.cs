using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public float matchCheckDelay = 1f; // Time to wait before checking matches
    public int scorePerMatch = 100;
    public string[] evilTileNames = {"EvilTile1", "EvilTile2"}; // Names of the 2 evil tile prefabs
    private List<TileFlipper> revealedEvilTiles = new List<TileFlipper>();
    
    [Header("UI References")]
    public TMPro.TextMeshProUGUI scoreText;
    public TMPro.TextMeshProUGUI gameStatusText;
    
    private List<TileFlipper> flippedTiles = new List<TileFlipper>();
    private int currentScore = 0;
    private int totalGoodMatches = 7; // Only 7 good pairs
    private int foundGoodMatches = 0;
    private bool canFlip = true;
    private bool gameOver = false;
    
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
        UpdateUI();
    }
    
    public void RegisterFlippedTile(TileFlipper tile)
    {
        if (!canFlip || gameOver) return;
        
        // Check if this is an evil tile
        if (IsEvilTile(tile))
        {
            Debug.Log("Evil tile registered: " + tile.gameObject.name);
            HandleEvilTileFlipped(tile);
            
            // Add to revealed evil tiles array
            revealedEvilTiles.Add(tile);
            Debug.Log("Revealed evil tiles count: " + revealedEvilTiles.Count);

            // If there's already a good tile flipped, flip it back (mismatch with evil)
            if (flippedTiles.Count > 0)
            {
                Debug.Log("Evil tile flipped while good tile was waiting - flipping good tile back");
                
                // Create a copy to avoid collection modification during enumeration
                List<TileFlipper> tilesToFlipBack = new List<TileFlipper>(flippedTiles);
                flippedTiles.Clear(); // Clear first to avoid UnregisterFlippedTile issues
                
                // Now flip back the tiles
                foreach (TileFlipper goodTile in tilesToFlipBack)
                {
                    goodTile.FlipBack();
                }
                
                canFlip = true; // Allow new round to begin immediately
            }
            
            // Check for evil pair matches
            CheckForEvilPairMatch();
            
            return; // Don't add to normal flippedTiles list
        }
        
        flippedTiles.Add(tile);
        Debug.Log("Total flipped tiles now: " + flippedTiles.Count);
        
        if (flippedTiles.Count == 2)
        {
            canFlip = false;
            Invoke(nameof(CheckForMatch), matchCheckDelay);
        }
    }

    private void CheckForEvilPairMatch()
    {
        Debug.Log("=== CheckForEvilPairMatch called ===");
        Debug.Log("Total revealed evil tiles: " + revealedEvilTiles.Count);
        
        if (revealedEvilTiles.Count < 2) return;
        
        // Check all combinations of revealed evil tiles for matches
        for (int i = 0; i < revealedEvilTiles.Count - 1; i++)
        {
            for (int j = i + 1; j < revealedEvilTiles.Count; j++)
            {
                TileFlipper tile1 = revealedEvilTiles[i];
                TileFlipper tile2 = revealedEvilTiles[j];
                
                Debug.Log("Checking evil pair: " + tile1.gameObject.name + " vs " + tile2.gameObject.name);
                
                if (DoTilesMatch(tile1, tile2))
                {
                    Debug.Log("EVIL PAIR MATCH FOUND! GAME OVER!");
                    HandleEvilPairMatch(tile1, tile2);
                    return; // Game over, no need to check more
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
        Debug.Log("=== CheckForMatch called ===");
        Debug.Log("Flipped tiles count: " + flippedTiles.Count);
        Debug.Log("Game over status: " + gameOver);
        
        if (flippedTiles.Count != 2 || gameOver)
        {
            canFlip = true;
            return;
        }
        
        TileFlipper tile1 = flippedTiles[0];
        TileFlipper tile2 = flippedTiles[1];
        
        Debug.Log("Checking match between: " + tile1.gameObject.name + " and " + tile2.gameObject.name);
        
        // Check if tiles match by comparing their prefab names
        bool isMatch = DoTilesMatch(tile1, tile2);
        Debug.Log("Do tiles match? " + isMatch);
        
        if (isMatch)
        {
            // Since evil tiles are handled separately, these should only be good matches
            Debug.Log("Good match found!");
            HandleGoodMatch(tile1, tile2);
        }
        else
        {
            Debug.Log("No match - flipping back");
            HandleMismatch(tile1, tile2);
        }
        
        flippedTiles.Clear();
        canFlip = true;
    }
    
    private bool DoTilesMatch(TileFlipper tile1, TileFlipper tile2)
    {
        // Compare by prefab name (remove "(Clone)" suffix)
        string name1 = tile1.gameObject.name.Replace("(Clone)", "");
        string name2 = tile2.gameObject.name.Replace("(Clone)", "");
        
        Debug.Log("Comparing: '" + name1 + "' vs '" + name2 + "'");
        bool match = name1 == name2;
        Debug.Log("Match result: " + match);
        
        return match;
    }
    
    private void HandleGoodMatch(TileFlipper tile1, TileFlipper tile2)
    {
        // Keep tiles flipped and disable interaction
        tile1.SetMatched(true);
        tile2.SetMatched(true);
        
        // Calculate the midpoint between the two matched tiles for animation start position
        Vector3 animationStartPos = (tile1.transform.position + tile2.transform.position) * 0.5f;
        
        // Get the tile type (remove "(Clone)" suffix)
        string tileType = tile1.gameObject.name.Replace("(Clone)", "");
        
        // Trigger collection animation
        if (CollectionBox.Instance != null)
        {
            CollectionBox.Instance.CollectIcon(tile1, tile2, animationStartPos, tileType);
        }
        
        // Update score
        currentScore += scorePerMatch;
        foundGoodMatches++;
        
        UpdateUI();
        CheckWinCondition();
    }
    
    private void HandleEvilTileFlipped(TileFlipper tile)
    {
        Debug.Log("Evil tile flipped! " + tile.gameObject.name);
        // Evil tile stays flipped for the rest of the game
        tile.SetEvilFlipped(true);
        
        // Visual feedback for evil tile
        if (gameStatusText != null)
            gameStatusText.text = "Evil tile revealed! Be careful...";
    }
    
    private void HandleEvilPairMatch(TileFlipper tile1, TileFlipper tile2)
    {
        Debug.Log("=== EVIL PAIR MATCHED - TRIGGERING GAME OVER ===");
        
        gameOver = true;
        canFlip = false;
        
        // Keep evil tiles visible
        tile1.SetMatched(true);
        tile2.SetMatched(true);
        
        string gameOverMessage = "GAME OVER! Evil Pair Matched! Final Score: " + foundGoodMatches + "/" + totalGoodMatches;
        Debug.Log(gameOverMessage);
        
        // Try multiple ways to show game over
        if (gameStatusText != null)
        {
            gameStatusText.text = gameOverMessage;
            gameStatusText.color = Color.red; // Make it red for emphasis
            Debug.Log("Game over message set to UI text");
        }
        else
        {
            Debug.LogError("gameStatusText is NULL! UI not properly linked!");
        }
        
        // Also update score text to show final score
        if (scoreText != null)
        {
            scoreText.text = "FINAL SCORE: " + currentScore + " (" + foundGoodMatches + "/7 matches)";
            scoreText.color = Color.red;
        }
    }
    
    private void HandleMismatch(TileFlipper tile1, TileFlipper tile2)
    {
        // Flip tiles back
        tile1.FlipBack();
        tile2.FlipBack();
    }
    
    private void CheckWinCondition()
    {
        if (foundGoodMatches >= totalGoodMatches && !gameOver)
        {
            Debug.Log("Game Won! All good matches found!");
            if (gameStatusText != null)
                gameStatusText.text = "Congratulations! Perfect Score: " + foundGoodMatches + "/" + totalGoodMatches;
        }
    }
    
    private bool IsEvilTile(TileFlipper tile)
    {
        string tileName = tile.gameObject.name.Replace("(Clone)", "");
        
        foreach (string evilName in evilTileNames)
        {
            if (tileName == evilName)
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
        flippedTiles.Clear();
        revealedEvilTiles.Clear(); // Clear the evil tiles array
        canFlip = true;
        gameOver = false;
        
        // Reset collection box
        if (CollectionBox.Instance != null)
        {
            CollectionBox.Instance.ResetCollection();
        }
        
        // Find and reset all tiles
        TileSpawner spawner = FindObjectOfType<TileSpawner>();
        if (spawner != null)
        {
            spawner.RespawnTiles();
        }
        
        UpdateUI();
    }
    
    public bool CanFlipTiles()
    {
        return canFlip && !gameOver;
    }
}