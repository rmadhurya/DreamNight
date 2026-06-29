using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class TileFlipper : MonoBehaviour
{
    private bool isFlipped = false;
    private bool isAnimating = false;
    private bool isMatched = false;

    public float flipDuration = 0.5f; // seconds
    public float liftHeight = 0.5f;   // how high it arcs up

    private Quaternion startRot;
    private Quaternion endRot;
    private Vector3 startPos;
    private float flipProgress;

    private bool isEvilFlipped = false;

    [Header("Glow Effects")]
    public float glowIntensity = 0.01f;
    public float pulseDuration = 0.5f; // How long the pulse lasts
    
    private Renderer tileRenderer;
    private Material tileMaterial;
    private Color originalEmissionColor;
    private bool isGlowing = false;

    void Update()
    {
        HandleClick();
        HandleFlip();
    }

    private void HandleClick()
    {
        // Don't allow clicking if matched, animating, already face-up, or game doesn't allow flipping.
        // A face-up tile should only flip back down via FlipBack() (called by GameManager on a
        // real mismatch) - never by the player clicking it again directly.
        if (isMatched || isAnimating || isFlipped) return;
        if (GameManager.Instance != null && !GameManager.Instance.CanFlipTiles()) return;
        
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform == transform)
                {
                    Debug.Log("Tile clicked: " + gameObject.name);
                    StartFlip();
                }
            }
        }
    }

    private void HandleFlip()
    {
        if (!isAnimating) return;

        flipProgress += Time.deltaTime / flipDuration;
        flipProgress = Mathf.Clamp01(flipProgress);

        // Rotation
        transform.rotation = Quaternion.Slerp(startRot, endRot, flipProgress);

        // Arc lift
        float yOffset = Mathf.Sin(flipProgress * Mathf.PI) * liftHeight; // peaks in middle of flip
        transform.position = startPos + new Vector3(0, yOffset, 0);

        if (flipProgress >= 1f)
        {
            isAnimating = false;
            isFlipped = !isFlipped;
            
            // Notify GameManager when tile finishes flipping
            if (isFlipped && GameManager.Instance != null)
            {
                GameManager.Instance.RegisterFlippedTile(this);
            }
        }
    }

    private void StartFlip()
    {
        isAnimating = true;
        flipProgress = 0f;
        startRot = transform.rotation;
        startPos = transform.position;

        // Rotation for flipped/not flipped
        endRot = isFlipped ? Quaternion.Euler(90, 0, -90) : Quaternion.Euler(270, 0, -90);
    }
    
    public void FlipBack()
    {
        if (!isFlipped || isMatched) return;
        
        // Unregister from GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterFlippedTile(this);
        }
        
        StartFlip();
    }

    void Start()
    {
        tileRenderer = GetComponent<Renderer>();
        if (tileRenderer != null)
        {
            // Create a unique material instance for this tile
            tileMaterial = new Material(tileRenderer.material);
            tileRenderer.material = tileMaterial;
            
            // Store original emission color
            originalEmissionColor = tileMaterial.GetColor("_EmissionColor");
        }
    }
    
    public void SetMatched(bool matched)
    {
        isMatched = matched;
        
        if (matched)
        {
            // Optional: Add visual feedback for matched tiles
            // You could change color, add particle effects, etc.
            Debug.Log(gameObject.name + " is now matched!");

            // Add blue glow for good matches
            StartGlowEffect(new Color(0, 0.8f, 1f));
        }
    }
    
    public bool IsFlipped()
    {
        return isFlipped;
    }
    
    public bool IsMatched()
    {
        return isMatched;
    }

    public void SetEvilFlipped(bool evilFlipped)
    {
        isEvilFlipped = evilFlipped;
        
        if (evilFlipped)
        {
            // Evil tile stays flipped and can't be clicked anymore
            isMatched = true; // Prevents further clicking

            // Add red glow for evil tiles
            StartGlowEffect(new Color(1f, 0.2f, 0.2f));
            
            // Optional: Add visual feedback for evil tiles
            // You could change the material color, add a red glow, etc.
            Debug.Log(gameObject.name + " is now permanently revealed as EVIL!");
        }
    }

    public bool IsEvilFlipped()
    {
        return isEvilFlipped;
    }

    private void StartGlowEffect(Color glowColor)
    {
        if (tileMaterial == null || isGlowing) return;
        
        isGlowing = true;
        
        // Enable emission on the material
        tileMaterial.EnableKeyword("_EMISSION");
        
        // Start the glow coroutine
        StartCoroutine(GlowPulse(glowColor));
    }
    
    private IEnumerator GlowPulse(Color glowColor)
    {
        float elapsed = 0f;
        
        while (elapsed < pulseDuration)
        {
            elapsed += Time.deltaTime;
            
            // Create a pulsing effect
            float intensity = Mathf.Sin(elapsed / pulseDuration * Mathf.PI) * glowIntensity;
            intensity = Mathf.Abs(intensity); // Keep it positive
            
            // Apply the glow color with intensity
            Color emissionColor = glowColor * intensity;
            tileMaterial.SetColor("_EmissionColor", emissionColor);
            
            yield return null;
        }
        
        // Keep a subtle constant glow after the pulse
        Color finalGlow = glowColor * (glowIntensity * 0.1f);
        tileMaterial.SetColor("_EmissionColor", finalGlow);
    }
    
    public void RemoveGlow()
    {
        if (tileMaterial == null) return;
        
        isGlowing = false;
        StopAllCoroutines();
        
        // Reset to original emission
        tileMaterial.SetColor("_EmissionColor", originalEmissionColor);
        
        // Disable emission if it was originally off
        if (originalEmissionColor == Color.black)
        {
            tileMaterial.DisableKeyword("_EMISSION");
        }
    }
    
    void OnDestroy()
    {
        // Clean up the material instance
        if (tileMaterial != null)
        {
            Destroy(tileMaterial);
        }
    }
}