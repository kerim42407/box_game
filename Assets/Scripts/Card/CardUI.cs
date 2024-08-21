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
    public Image hiddenImage;

    #endregion

    #region Methods

    private void Awake()
    {
        card = GetComponent<Card>();
        SetCardUI(false);
    }

    private void OnValidate()
    {
        Awake();
    }

    public void SetCardUI(bool shouldHide)
    {
        if (card != null && card.CardData != null)
        {
            SetCardTexts();
            SetCardImage(shouldHide);
        }
    }

    private void SetCardTexts()
    {
        cardName.text = card.CardData.CardName;
        cardEffectDescription.text = card.CardData.CardEffectDescription;
        cardDescription.text = card.CardData.CardDescription;
    }

    private void SetCardImage(bool shouldHide)
    {
        cardImage.sprite = card.CardData.Image;
        if (shouldHide)
        {
            hiddenImage.gameObject.SetActive(true);
        }
    }

    #endregion
}
