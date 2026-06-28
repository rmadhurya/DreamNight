using UnityEngine;
using UnityEngine.UI;

public class CollectionSlot : MonoBehaviour
{
    [Header("Slot Appearance")]
    public Color emptyColor = new Color(1f, 1f, 1f, 0.2f);
    public Color filledColor = new Color(1f, 1f, 1f, 1f);
    public Color slotBorderColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    
    private Image slotImage;
    private Image borderImage;
    private bool isFilled = false;
    
    void Awake()
    {
        slotImage = GetComponent<Image>();
        
        // Create border if it doesn't exist
        Transform borderTransform = transform.Find("Border");
        if (borderTransform == null)
        {
            GameObject borderObj = new GameObject("Border");
            borderObj.transform.SetParent(transform);
            borderImage = borderObj.AddComponent<Image>();
            
            RectTransform borderRect = borderImage.GetComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.anchoredPosition = Vector2.zero;
            borderRect.sizeDelta = Vector2.zero;
            borderRect.SetAsFirstSibling(); // Put border behind the main image
            
            borderImage.color = slotBorderColor;
        }
        else
        {
            borderImage = borderTransform.GetComponent<Image>();
        }
    }
    
    void Start()
    {
        SetEmpty();
    }
    
    public void SetEmpty()
    {
        isFilled = false;
        if (slotImage != null)
        {
            slotImage.color = emptyColor;
            slotImage.sprite = null;
        }
    }
    
    public void SetFilled(Sprite iconSprite)
    {
        isFilled = true;
        if (slotImage != null)
        {
            slotImage.sprite = iconSprite;
            slotImage.color = filledColor;
        }
    }
    
    public bool IsFilled()
    {
        return isFilled;
    }
}