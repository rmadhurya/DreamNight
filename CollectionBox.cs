using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CollectionBox : MonoBehaviour
{
    [Header("Collection Box Settings")]
    public RectTransform collectionContainer;
    public GameObject collectionSlotPrefab; // A simple UI Image prefab for showing collected icons
    public float animationDuration = 1.0f;
    public AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Collection Icons")]
    public Sprite[] collectionIcons; // Array of sprites to show in collection slots
    public bool useCustomIcons = true; // Toggle to use custom icons vs tile textures
    public string[] tileNameToIconMapping; // Optional: Map tile names to specific icon indices
    
    [Header("UI References")]
    public TMPro.TextMeshProUGUI collectionCountText;
    
    private List<Image> collectionSlots = new List<Image>();
    private int maxCollections = 7;
    private int currentCollections = 0;
    
    public static CollectionBox Instance { get; private set; }
    
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
        // Validate that we have the right number of collection icons
        if (useCustomIcons && collectionIcons.Length != maxCollections)
        {
            Debug.LogWarning($"CollectionBox: Expected {maxCollections} collection icons, but got {collectionIcons.Length}. Some slots may be empty or use default icons.");
        }
        
        InitializeCollectionSlots();
        UpdateCollectionDisplay();
    }
    
    private void InitializeCollectionSlots()
    {
        // Clear existing slots
        foreach (Transform child in collectionContainer)
        {
            Destroy(child.gameObject);
        }
        collectionSlots.Clear();
        
        // Create empty collection slots
        for (int i = 0; i < maxCollections; i++)
        {
            GameObject slot = Instantiate(collectionSlotPrefab, collectionContainer);
            Image slotImage = slot.GetComponent<Image>();
            
            // Make slot initially transparent/empty (no preview icons)
            slotImage.sprite = null;
            Color slotColor = slotImage.color;
            slotColor.a = 0.1f; // Semi-transparent to show empty slots
            slotImage.color = slotColor;
            
            collectionSlots.Add(slotImage);
        }
    }
    
    public void CollectIcon(TileFlipper tile1, TileFlipper tile2, Vector3 worldStartPos, string tileType = "")
    {
        if (currentCollections >= maxCollections) 
        {
            Debug.Log("Collection box is full!");
            return;
        }
        
        if (collectionContainer == null)
        {
            Debug.LogError("Collection Container is not assigned in CollectionBox!");
            return;
        }
        
        if (collectionSlots == null || collectionSlots.Count == 0)
        {
            Debug.LogError("Collection slots are not initialized! Make sure InitializeCollectionSlots() ran.");
            return;
        }
        
        Sprite iconToCollect = null;
        int iconIndex = -1;
        
        // Try to find icon index by tile name mapping
        if (useCustomIcons && !string.IsNullOrEmpty(tileType) && tileNameToIconMapping != null)
        {
            for (int i = 0; i < tileNameToIconMapping.Length && i < collectionIcons.Length; i++)
            {
                if (tileNameToIconMapping[i] == tileType)
                {
                    iconIndex = i;
                    break;
                }
            }
        }
        
        // Determine which icon to use
        if (useCustomIcons && iconIndex >= 0 && iconIndex < collectionIcons.Length)
        {
            // Use the mapped custom icon
            iconToCollect = collectionIcons[iconIndex];
            Debug.Log("Using mapped icon for " + tileType + " at index " + iconIndex);
        }
        else if (useCustomIcons && currentCollections < collectionIcons.Length)
        {
            // Use the next custom icon in sequence
            iconToCollect = collectionIcons[currentCollections];
            Debug.Log("Using sequential icon at index " + currentCollections);
        }
        else
        {
            // Fallback: Extract sprite from tile texture
            Renderer tileRenderer = tile1.GetComponent<Renderer>();
            if (tileRenderer != null && tileRenderer.material.mainTexture is Texture2D tileTexture)
            {
                iconToCollect = Sprite.Create(tileTexture, 
                    new Rect(0, 0, tileTexture.width, tileTexture.height), 
                    new Vector2(0.5f, 0.5f));
                Debug.Log("Using texture from tile");
            }
        }
        
        if (iconToCollect == null)
        {
            Debug.LogWarning("No icon found for collection! Check your Collection Icons array.");
            return;
        }
        
        // Start the collection animation
        Debug.Log("Starting collection animation for icon: " + (iconToCollect.name ?? "unnamed"));
        StartCoroutine(AnimateIconCollection(iconToCollect, worldStartPos));
    }
    
    private IEnumerator AnimateIconCollection(Sprite iconSprite, Vector3 worldStartPos, int targetSlotIndex = -1)
    {
        // Determine which slot to target
        int slotIndex;
        if (targetSlotIndex >= 0 && targetSlotIndex < collectionSlots.Count)
        {
            slotIndex = targetSlotIndex; // Use specific slot
        }
        else
        {
            slotIndex = currentCollections; // Use next available slot
        }
        
        // Safety checks
        if (collectionContainer == null)
        {
            Debug.LogError("Collection Container is not assigned!");
            yield break;
        }
        
        if (slotIndex >= collectionSlots.Count)
        {
            Debug.LogError("Target slot index " + slotIndex + " is out of range!");
            yield break;
        }
        
        // Check if target slot is already filled (for specific slot mode)
        if (targetSlotIndex >= 0 && collectionSlots[slotIndex].color.a >= 1.0f)
        {
            Debug.Log("Slot " + slotIndex + " already filled, skipping animation");
            yield break;
        }
        
        // Find the Canvas
        Canvas canvas = collectionContainer.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Cannot find Canvas parent!");
            yield break;
        }
        
        // Create a temporary UI icon for animation
        GameObject tempIcon = new GameObject("TempCollectionIcon");
        tempIcon.transform.SetParent(canvas.transform); // Put it in the Canvas
        
        Image tempImage = tempIcon.AddComponent<Image>();
        if (iconSprite != null)
        {
            tempImage.sprite = iconSprite;
        }
        
        RectTransform tempRect = tempIcon.GetComponent<RectTransform>();
        tempRect.sizeDelta = new Vector2(80, 80); // Size of the animated icon
        
        // Convert world position to screen/UI position
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("No main camera found!");
            Destroy(tempIcon);
            yield break;
        }
        
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldStartPos);
        Vector2 startUIPos;
        
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        bool screenToLocal = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, 
            screenPos, 
            canvas.worldCamera, 
            out startUIPos);
            
        if (!screenToLocal)
        {
            Debug.LogError("Could not convert screen position to local position!");
            Destroy(tempIcon);
            yield break;
        }
        
        tempRect.anchoredPosition = startUIPos;
        
        // Get target position (the specific collection slot)
        if (collectionSlots[slotIndex] == null)
        {
            Debug.LogError("Collection slot " + slotIndex + " is null!");
            Destroy(tempIcon);
            yield break;
        }
        
        Vector2 targetPos = collectionSlots[slotIndex].rectTransform.anchoredPosition;
        targetPos = collectionContainer.anchoredPosition + targetPos;
        
        // Animate the icon moving to the collection box
        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / animationDuration;
            float curveValue = movementCurve.Evaluate(progress);
            
            // Interpolate position
            Vector2 currentPos = Vector2.Lerp(startUIPos, targetPos, curveValue);
            tempRect.anchoredPosition = currentPos;
            
            // Scale effect - start normal, shrink in middle, then grow to final size
            float scale = 1.0f + Mathf.Sin(progress * Mathf.PI) * 0.3f;
            tempRect.localScale = Vector3.one * scale;
            
            yield return null;
        }
        
        // Animation complete - update the actual collection slot
        Image targetSlot = collectionSlots[slotIndex];
        if (iconSprite != null)
        {
            targetSlot.sprite = iconSprite;
        }
        
        // Make the slot fully opaque now that it contains an icon
        Color slotColor = targetSlot.color;
        slotColor.a = 1.0f;
        targetSlot.color = slotColor;
        
        // Add a small "pop" effect to the collection slot
        StartCoroutine(PopEffect(targetSlot.rectTransform));
        
        // Clean up temporary icon
        Destroy(tempIcon);
        
        // Update collection count only if using sequential mode
        if (targetSlotIndex < 0)
        {
            currentCollections++;
        }
        else
        {
            // For specific slot mode, count filled slots
            currentCollections = 0;
            foreach (Image slot in collectionSlots)
            {
                if (slot.color.a >= 1.0f)
                    currentCollections++;
            }
        }
        
        UpdateCollectionDisplay();
    }
    
    private IEnumerator PopEffect(RectTransform target)
    {
        Vector3 originalScale = target.localScale;
        float popDuration = 0.3f;
        float elapsed = 0f;
        
        while (elapsed < popDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / popDuration;
            
            // Create a "pop" scaling effect
            float scale = 1.0f + Mathf.Sin(progress * Mathf.PI) * 0.4f;
            target.localScale = originalScale * scale;
            
            yield return null;
        }
        
        target.localScale = originalScale;
    }
    
    private void UpdateCollectionDisplay()
    {
        if (collectionCountText != null)
        {
            collectionCountText.text = currentCollections + "/" + maxCollections;
        }
    }
    
    public void ResetCollection()
    {
        currentCollections = 0;
        
        // Reset all slots to empty state
        foreach (Image slot in collectionSlots)
        {
            slot.sprite = null;
            Color slotColor = slot.color;
            slotColor.a = 0.2f; // Semi-transparent empty slots
            slot.color = slotColor;
        }
        
        UpdateCollectionDisplay();
    }
    
    public int GetCollectionCount()
    {
        return currentCollections;
    }
    
    public bool IsCollectionComplete()
    {
        return currentCollections >= maxCollections;
    }
}