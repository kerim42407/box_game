using Mirror;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CardUI))]
public class Card : NetworkBehaviour
{
    #region Fields and Properties

    [field: SerializeField] public ScriptableCard CardData { get; private set; }
    public CardAnimation cardAnimation;
    public int count;
    public UnityEvent<Card> playCardEvent = new();
    public UnityEvent<Card> destroyCardEvent = new();
    [SyncVar] public PlayerObjectController s_OwnerPlayer;

    #endregion

    #region Methods

    public void SetUpEvent()
    {
        SetPlayCardEvent();
    }

    public void SetUpUI(ScriptableCard data, bool shouldHide)
    {
        CardData = data;
        GetComponent<CardUI>().SetCardUI(shouldHide);
    }

    public void PlayCard()
    {
        StartCoroutine(cardAnimation.MoveToPlayArea());
        //playCardEvent?.Invoke(this);
    }

    public void DestroyCard()
    {
        destroyCardEvent?.Invoke(this);
    }

    public void SetPlayCardEvent()
    {
        switch (CardData.Category)
        {
            case CardCategory.Market:
                playCardEvent.AddListener(GameManager.Instance.CmdPlayMarketCard);
                destroyCardEvent.AddListener(GameManager.Instance.CmdDestroyMarketCard);
                break;
        }
        //if (CardData.Category == CardCategory.Market)
        //{
        //    playCardEvent.AddListener(GameManager.Instance.CmdPlayCard);
        //    destroyCardEvent.AddListener(GameManager.Instance.CmdDestroyCard);
        //}
    }

    #endregion
}
