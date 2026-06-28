using UnityEngine;
using System;

// Every mini-game prefab needs a script that inherits from this.
// Implement StartMiniGame() to kick off your specific 2D mini-game,
// and call FinishMiniGame() when it ends.
public abstract class MiniGameBase : MonoBehaviour
{
    public event Action<float> OnMiniGameFinished;

    // Called by CameraRigController right after the prefab is instantiated
    // into the mini-game canvas. Use this to reset state and start input.
    public abstract void StartMiniGame();

    // Call this from inside your specific mini-game script when it ends.
    // performancePercent should be 0.0 (worst) to 1.0 (best/perfect) so
    // every mini-game feeds the same scoring formula in GameManager,
    // no matter how different its actual mechanics are.
    protected void FinishMiniGame(float performancePercent)
    {
        OnMiniGameFinished?.Invoke(Mathf.Clamp01(performancePercent));
    }
}