using UnityEngine;
using UnityEngine.InputSystem;

// TEMPORARY testing helper - lets you launch any mini-game directly with a
// keypress (1-7), without needing to actually make that match on the board.
// Attach this to an always-active object (e.g. your MiniGameSystem object).
// Delete this script (or just disable the GameObject) before shipping.
public class DebugMiniGameTester : MonoBehaviour
{
    public MiniGameWindowController miniGameWindow;

    [Tooltip("Index 0 = key 1, index 1 = key 2, etc. Must match tileTypeName values exactly (e.g. 'candle_tile').")]
    public string[] tileTypeNames =
    {
        "candle_tile",
        "cat_tile",
        "moon_tile",
        "pillow_tile",
        "star_tile",
        "telescope_tile",
        "book_tile"
    };

    void Update()
    {
        if (Keyboard.current == null || miniGameWindow == null) return;

        for (int i = 0; i < tileTypeNames.Length && i < 9; i++)
        {
            Key key = (Key)((int)Key.Digit1 + i);

            if (Keyboard.current[key].wasPressedThisFrame)
            {
                string tileType = tileTypeNames[i];
                Debug.Log($"[DebugMiniGameTester] Launching '{tileType}' mini-game");

                miniGameWindow.TriggerMiniGame(tileType, performancePercent =>
                {
                    Debug.Log($"[DebugMiniGameTester] '{tileType}' finished with performance {performancePercent:P0}");
                });

                break;
            }
        }
    }
}