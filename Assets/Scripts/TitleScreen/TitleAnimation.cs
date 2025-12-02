using UnityEngine;

public class TitleAnimation : MonoBehaviour
{
    [Header("Up-Down Movement")]
    [Tooltip("Movement Speed")]
    public float floatSpeed = 2.0f;
    [Tooltip("Distance")]
    public float floatDistance = 15f;

    [Header("Scaling")]
    [Tooltip("Scaling change speed")]
    public float scaleSpeed = 2.0f;
    [Tooltip("Scale amount")]
    public float scaleAmount = 0.05f;

    private RectTransform rectTransform;
    private Vector2 startAnchoredPos;
    private Vector3 startScale;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        if (rectTransform == null)
        {
            Debug.LogError("TitleAnimation: Este script debe ir en un objeto de UI (Image/Text).");
            return;
        }
        startAnchoredPos = rectTransform.anchoredPosition;
        startScale = rectTransform.localScale;
    }

    void Update()
    {
        if (rectTransform == null) return;
        float newY = startAnchoredPos.y + (Mathf.Sin(Time.time * floatSpeed) * floatDistance);
        rectTransform.anchoredPosition = new Vector2(startAnchoredPos.x, newY);

        float scaleFactor = 1f + (Mathf.Sin(Time.time * scaleSpeed) * scaleAmount);
        rectTransform.localScale = startScale * scaleFactor;
    }
}