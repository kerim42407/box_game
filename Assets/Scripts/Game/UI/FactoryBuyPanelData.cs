using Michsky.MUIP;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FactoryBuyPanelData : MonoBehaviour
{
    public string[] productionType = { "Clay", "Copper", "Iron", "Cotton", "Coal" };
    public int productionTypeIndex;

    public Image windowFrame;
    public TextMeshProUGUI locationNameText;
    public ButtonManager previousButton;
    public TextMeshProUGUI productionTypeText;
    public ButtonManager nextButton;
    public TextMeshProUGUI factoryLevelText;
    public TextMeshProUGUI productivityText;
    public TextMeshProUGUI buyPriceText;
    public TextMeshProUGUI rentRateText;
    public ButtonManager buyButton;
    public ButtonManager cancelButton;
    public TextMeshProUGUI ownerNameText;
}
