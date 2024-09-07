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
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }
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
            case CardCategory.Luck:
                switch (CardData.CardIndex)
                {
                    case 0:
                        break;
                    case 1:
                        break;
                    case 2:
                        break;
                    case 3:
                        playCardEvent.AddListener(GameManager.Instance.CmdPlayStrongStormCard);
                        break;
                    case 4:
                        break;
                    case 5:
                        playCardEvent.AddListener(GameManager.Instance.CmdPlayFertileSoilsCard);
                        destroyCardEvent.AddListener(GameManager.Instance.CmdDestroyFertileSoilsCard);
                        break;
                }
                break;
            case CardCategory.Market:
                playCardEvent.AddListener(GameManager.Instance.CmdPlayMarketCard);
                destroyCardEvent.AddListener(GameManager.Instance.CmdDestroyMarketCard);
                break;
            case CardCategory.Sabotage:
                switch (CardData.CardIndex)
                {
                    case 0:
                        break;
                    case 1:
                        break;
                    case 2:
                        break;
                    case 3:
                        playCardEvent.AddListener(GameManager.Instance.CmdPlaySuspiciousFireCard);
                        break;
                    case 4:
                        playCardEvent.AddListener(GameManager.Instance.CmdPlayResourceDisasterCard);
                        break;
                    case 5:
                        playCardEvent.AddListener(GameManager.Instance.CmdPlayFactoryShutdownCard);
                        destroyCardEvent.AddListener(GameManager.Instance.CmdDestroyFactoryShutdownCard);
                        break;
                }
                break;
        }
    }

    public bool CheckCardPlayable()
    {
        switch (CardData.Category)
        {
            case CardCategory.Luck:
                switch (CardData.CardIndex)
                {
                    case 0:
                        return true;
                    case 1:
                        return true;
                    case 2:
                        return true;
                    case 3:
                        return true;
                    case 4:
                        return true;
                }
                return true;
            case CardCategory.Market:
                return true;
            case CardCategory.Sabotage:
                switch (CardData.CardIndex)
                {
                    case 0:
                        return true;
                    case 1:
                        return true;
                    case 2:
                        return true;
                    case 3:
                        return GameManager.Instance.CheckSuspiciousFirePlayable(this);
                    case 4:
                        return GameManager.Instance.CheckResourceDisasterPlayable(this);
                    case 5:
                        return GameManager.Instance.CheckFactoryShutdownPlayable(this);
                }
                return true;
        }
        return true;
    }

    #endregion
}
