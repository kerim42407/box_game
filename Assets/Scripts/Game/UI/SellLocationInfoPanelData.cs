using Michsky.MUIP;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SellLocationInfoPanelData : MonoBehaviour
{
    [Header("References")]
    public GameObject eventContainer;
    public TextMeshProUGUI locationNameText;
    public TextMeshProUGUI productivityText;
    public TextMeshProUGUI sellPriceText;
    public ButtonManager toggleButton;
    public Image icon;

    [Header("Prefabs")]
    public GameObject positiveEventPrefab;
    public GameObject negativeEventPrefab;

    [Header("Textures")]
    public Sprite uncheckedImage;
    public Sprite checkedImage;

    [HideInInspector] public bool toggleButtonState;
}
