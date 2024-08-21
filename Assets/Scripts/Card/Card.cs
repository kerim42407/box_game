using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CardUI))]
public class Card : MonoBehaviour
{
    #region Fields and Properties

    [field: SerializeField] public ScriptableCard CardData { get; private set; }
    public CardAnimation cardAnimation;
    public int count;
    public UnityEvent<PlayerObjectController> playCardEvent = new();
    private UnityEvent<PlayerObjectController> destroyCardEvent = new();

    #endregion

    #region Methods

    public void SetUp(ScriptableCard data, bool shouldHide)
    {
        CardData = data;
        SetPlayCardEvent();
        GetComponent<CardUI>().SetCardUI(shouldHide);
    }

    public void PlayCard(PlayerObjectController player)
    {
        StartCoroutine(cardAnimation.MoveToPlayArea(player, 2f));
        //playCardEvent?.Invoke(player);
    }

    public void DestroyCard(PlayerObjectController player)
    {
        destroyCardEvent?.Invoke(player);
    }

    public void SetPlayCardEvent()
    {
        if (CardData.Category == CardCategory.Market)
        {
            playCardEvent.AddListener(PlayMarketCard);
            destroyCardEvent.AddListener(DestroyMarketCard);
        }
    }

    public void PlayMarketCard(PlayerObjectController player)
    {
        foreach (LocationController locationController in PlaygroundController.Instance.allFactories)
        {
            if (locationController.productionType == CardData.ProductionType)
            {
                locationController.activeCards.Add(this);
                locationController.UpdateProductivity();
            }
        }
        gameObject.SetActive(false);
        
    }

    public void DestroyMarketCard(PlayerObjectController player)
    {
        foreach (LocationController locationController in PlaygroundController.Instance.allFactories)
        {
            if (locationController.productionType == CardData.ProductionType)
            {
                locationController.activeCards.Remove(this);
                locationController.UpdateProductivity();
            }
        }
        Destroy(gameObject);
    }
    #endregion
}
