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
            iconToCollect = collectionIcons[iconIndex];
        }
        else if (useCustomIcons && currentCollections < collectionIcons.Length)
        {
            iconToCollect = collectionIcons[currentCollections];
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
            }
        }
        
        if (iconToCollect == null)
        {
            Debug.LogWarning("No icon found for collection! Check your Collection Icons array.");
            return;
        }
        
        // Fill the next slot instantly - no flying animation
        Image targetSlot = collectionSlots[currentCollections];
        targetSlot.sprite = iconToCollect;
        Color slotColor = targetSlot.color;
        slotColor.a = 1.0f;
        targetSlot.color = slotColor;
        
        StartCoroutine(PopEffect(targetSlot.rectTransform));
        
        currentCollections++;
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