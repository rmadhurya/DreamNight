using UnityEngine;
using UnityEngine.UI;

// PLACEHOLDER mini-game for the 'star' tile.
// Replace the contents of StartMiniGame() (and add whatever you need)
// with the real 2D gameplay. The only requirement is calling
// FinishMiniGame(percent) when it's done, with percent from 0.0 to 1.0
// representing how well the player did.
public class StarMiniGame : MiniGameBase
{
    [Header("Temporary placeholder UI - remove once real gameplay exists")]
    public Button placeholderWinButton;

    public override void StartMiniGame()
    {
        if (placeholderWinButton != null)
        {
            placeholderWinButton.onClick.AddListener(() => FinishMiniGame(1f));
        }
    }
}