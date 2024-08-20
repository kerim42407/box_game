using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CardUI))]
public class Card : MonoBehaviour
{
    #region Fields and Properties

    [field: SerializeField] public ScriptableCard CardData { get; private set; }
    public CardAnimation cardAnimation;
    private UnityEvent playCardEvent = new();

    #endregion

    #region Methods

    public void SetUp(ScriptableCard data, bool shouldHide)
    {
        CardData = data;
        SetPlayCardEvent();
        GetComponent<CardUI>().SetCardUI(shouldHide);
    }

    public void PlayCard()
    {
        playCardEvent?.Invoke();
    }

    public void SetPlayCardEvent()
    {
        if(CardData.Category == CardCategory.Market)
        {
            playCardEvent.AddListener(PlayMarketCard);
        }
    }

    public void PlayMarketCard()
    {
        foreach (LocationController locationController in PlaygroundController.Instance.allFactories)
        {
            if (locationController.productionType == CardData.ProductionType)
            {
                locationController.activeCards.Add(this);
                locationController.UpdateProductivity(CardData.ProductivityValue);
            }
        }
    }
    #endregion
}
