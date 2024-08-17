using TMPro;
using UnityEngine;

public class AutoSizeController : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    public RectTransform textArea;
    public float defaultFontSize = 36f;

    void Start()
    {
        AdjustFontSize();
    }

    public void AdjustFontSize()
    {
        // Disable auto size and set default font size
        textMeshPro.enableAutoSizing = false;
        textMeshPro.fontSize = defaultFontSize;

        // Set font size
        textMeshPro.ForceMeshUpdate();

        // Check if text reaches limits
        if (textMeshPro.preferredWidth > textArea.rect.width || textMeshPro.preferredHeight > textArea.rect.height)
        {
            // Enable auto size if text reaches limits
            textMeshPro.enableAutoSizing = true;
        }
    }
}
