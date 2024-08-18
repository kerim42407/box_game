using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    #region Fields and Variables

    private Card card;

    [Header("Prefab Elements")]
    [SerializeField] private Image cardImage;
    [SerializeField] private TextMeshProUGUI cardName;
    [SerializeField] private TextMeshProUGUI cardEffectDescription;
    [SerializeField] private TextMeshProUGUI cardDescription;

    #endregion

    #region Methods

    private void Awake()
    {
        card = GetComponent<Card>();
        SetCardUI();
    }

    private void OnValidate()
    {
        Awake();
    }

    public void SetCardUI()
    {
        if (card != null && card.CardData != null)
        {
            SetCardTexts();
        }
    }

    private void SetCardTexts()
    {
        cardName.text = card.CardData.CardName;
        cardEffectDescription.text = card.CardData.CardEffectDescription;
        cardDescription.text = card.CardData.CardDescription;
    }

    private void SetCardImage()
    {
        cardImage.sprite = card.CardData.Image;
    }

    #endregion
}
