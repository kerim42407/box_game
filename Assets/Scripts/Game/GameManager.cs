using Mirror;
using System.Globalization;
using TMPro;
using UnityEngine;



public class GameManager : NetworkBehaviour
{
    [Header("References")]
    public UIManager uiManager;
    public GameObject playgroundPrefab;
    public GameObject cardPrefab;
    public GameObject canvas;
    public PlayerObjectController localPlayerController;
    [HideInInspector] public PlaygroundController playgroundController;
    [HideInInspector] public Material[] playerColors;

    [SyncVar] public int turnIndex;
    [SyncVar] public int turnCount;
    public readonly SyncList<Card> s_ActiveCards = new();

    // Manager
    private MyNetworkManager manager;
    public MyNetworkManager Manager
    {
        get
        {
            if (manager != null)
            {
                return manager;
            }
            return manager = MyNetworkManager.singleton as MyNetworkManager;
        }
    }

    [Header("Game Variables")]
    public float resourceBuyPrice;
    public float resourceRentRate;
    [SyncVar] public float startingPointIncome;

    public float baseFactoryPrice;
    public float regularFactoryPriceCoef;
    public float bigFactoryPriceCoef;
    public float goldenFactoryPriceCoef;
    public float[] factoryPriceCoefPerLevel;

    public float resourceProductivityCoef;
    public float bonus;

    [Header("Notification Events")]
    public NotificationEventBase notificationEventBase;

    [Header("Debug References")]
    public TMP_InputField inputField;
    public GameObject customDiceButton;

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        if (isServer)
        {
            CmdSetupGame();
            inputField.gameObject.SetActive(true);
            customDiceButton.gameObject.SetActive(true);
        }
        else
        {
            inputField.gameObject.SetActive(false);
            customDiceButton.gameObject.SetActive(false);
        }
        if (isClient)
        {
            CmdSendSetupGameRequest(GetComponent<NetworkIdentity>().connectionToClient);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    [Command(requiresAuthority = false)]
    public void CmdSetupGame()
    {
        GameObject playground = Instantiate(playgroundPrefab);
        NetworkServer.Spawn(playground);
        foreach (PlayerObjectController playerObjectController in Manager.gamePlayers)
        {
            playerObjectController.playerMoney = Manager.startingMoney * 1000;
        }
        startingPointIncome = (Manager.startingMoney * 1000) / 2;
        RpcShowNotification(Manager.gamePlayers[turnIndex].connectionToClient);
        Manager.gamePlayers[turnIndex].turnCount++;
    }
    [Command(requiresAuthority = false)]
    public void CmdSendSetupGameRequest(NetworkConnectionToClient target)
    {
        RpcSendSetupGameRequest(target);
    }
    [TargetRpc]
    public void RpcSendSetupGameRequest(NetworkConnectionToClient target)
    {
        playgroundController = GameObject.FindGameObjectWithTag("Playground").GetComponent<PlaygroundController>();
        foreach (PlayerObjectController playerObjectController in Manager.gamePlayers)
        {
            playerObjectController.playgroundController = playgroundController;
            playerObjectController.playerMoveController.SetStartPosition();
        }
        for (int i = 0; i < Manager.gamePlayers.Count; i++)
        {
            GamePlayerListItem gamePlayerListItem;
            if (Manager.gamePlayers[i].isLocalPlayer)
            {
                gamePlayerListItem = Instantiate(uiManager.localPlayerListItemPrefab, uiManager.localPlayerListItemPanel.transform).GetComponent<GamePlayerListItem>();
            }
            else
            {
                gamePlayerListItem = Instantiate(uiManager.gamePlayerListItemPrefab, uiManager.gamePlayersListItemPanel.transform).GetComponent<GamePlayerListItem>();
            }
            gamePlayerListItem.playerName = Manager.gamePlayers[i].playerName;
            gamePlayerListItem.playerSteamID = Manager.gamePlayers[i].playerSteamID;
            gamePlayerListItem.background.color = Manager.gamePlayers[i].playerColor;
            gamePlayerListItem.playerMoneyText.text = "$" + string.Format(CultureInfo.InvariantCulture, "{0:N0}", Manager.gamePlayers[i].playerMoney);
            Manager.gamePlayers[i].gamePlayerListItem = gamePlayerListItem;
            gamePlayerListItem.playerNameText.GetComponent<AutoSizeController>().AdjustFontSize();
            gamePlayerListItem.playerMoneyText.GetComponent<AutoSizeController>().AdjustFontSize();
            gamePlayerListItem.SetPlayerValues();
        }
        for (int i = 0; i < Manager.gamePlayers.Count; i++)
        {
            if (i == turnIndex)
            {
                Manager.gamePlayers[i].canPlay = true;
                Manager.gamePlayers[i].playerInputController.canThrow = true;
            }
            else
            {
                Manager.gamePlayers[i].canPlay = false;
                Manager.gamePlayers[i].playerInputController.canThrow = false;
            }
        }
    }

    public void OnDiceResult(int result, bool isEven)
    {
        Debug.Log($"Applying dice result: {result}");
        if (isEven)
        {
            Manager.gamePlayers[turnIndex].playerMoveController.isEven = !Manager.gamePlayers[turnIndex].playerMoveController.isEven;
        }
        else
        {
            if (Manager.gamePlayers[turnIndex].playerMoveController.isEven)
            {
                Manager.gamePlayers[turnIndex].playerMoveController.isEven = false;
            }
        }
        Manager.gamePlayers[turnIndex].playerMoveController.MovePlayer(result);
    }

    public void CustomDiceResult(int result)
    {
        Manager.gamePlayers[turnIndex].playerMoveController.MovePlayer(int.Parse(inputField.text));
    }

    public void Test()
    {
        Manager.gamePlayers[turnIndex].playerMoveController.Test();
    }

    public void SellTest()
    {
        Manager.gamePlayers[turnIndex].playerMoveController.SellTest(Manager.gamePlayers[turnIndex].connectionToClient,10000);
    }

    [Command(requiresAuthority = false)]
    public void CmdUpdateTurnIndex()
    {
        bool shouldIncreaseTurnCount = false;
        if (!Manager.gamePlayers[turnIndex].isBankrupt)
        {
            if (Manager.gamePlayers[turnIndex].GetComponent<PlayerMoveController>().isEven)
            {

            }
            else
            {
                turnIndex++;
                if (turnIndex == Manager.gamePlayers.Count)
                {
                    turnIndex = 0;
                }
                while (Manager.gamePlayers[turnIndex].isBankrupt)
                {
                    turnIndex++;
                    if (turnIndex == Manager.gamePlayers.Count)
                    {
                        turnIndex = 0;
                    }
                }
                shouldIncreaseTurnCount = true;
            }
        }
        else
        {
            while (Manager.gamePlayers[turnIndex].isBankrupt)
            {
                turnIndex++;
                if (turnIndex == Manager.gamePlayers.Count)
                {
                    turnIndex = 0;
                }
            }
            shouldIncreaseTurnCount = true;
        }

        if (shouldIncreaseTurnCount)
        {
            foreach (Card card in Manager.gamePlayers[turnIndex].s_PlayerActiveCards)
            {
                card.count++;
                if (card.count == card.CardData.CardDuration + 3)
                {
                    CmdDestroyCard(Manager.gamePlayers[turnIndex], card);
                    //card.DestroyCard();
                    //playgroundController.RpcDestroyCard(Manager.gamePlayers[turnIndex], Manager.gamePlayers[turnIndex].playerCards.IndexOf(card));
                }

            }
            Manager.gamePlayers[turnIndex].turnCount++;

        }
        RpcUpdateTurnIndex(turnIndex);
        RpcShowNotification(Manager.gamePlayers[turnIndex].connectionToClient);

    }
    [ClientRpc]
    private void RpcUpdateTurnIndex(int index)
    {
        for (int i = 0; i < Manager.gamePlayers.Count; i++)
        {
            if (i == index)
            {
                Manager.gamePlayers[i].canPlay = true;
                Manager.gamePlayers[i].playerInputController.canThrow = true;
            }
            else
            {
                Manager.gamePlayers[i].canPlay = false;
                Manager.gamePlayers[i].playerInputController.canThrow = false;
            }
        }
    }

    #region Notification Event Functions
    [TargetRpc]
    private void RpcShowNotification(NetworkConnectionToClient target)
    {
        notificationEventBase.ShowNotification(uiManager.mainCanvas.transform);
    }
    #endregion

    [Command]
    public void CmdTargetDebugLog()
    {

    }

    [TargetRpc]
    public void RpcTargetDebugLog(NetworkConnectionToClient target, string log)
    {
        Debug.Log(log);
    }

    public ResourceState SetGoldenFactoryResourceState(FactoryController factoryController)
    {
        foreach (ResourceController resourceController in playgroundController.resources)
        {
            if (resourceController.s_ProductionType == factoryController.s_ProductionType)
            {
                if (resourceController.s_OwnerPlayer != null)
                {
                    if (resourceController.s_OwnerPlayer == factoryController.s_OwnerPlayer)
                    {
                        return ResourceState.Positive;
                    }
                    else
                    {
                        return ResourceState.Negative;
                    }
                }
            }
        }

        return ResourceState.Neutral;
    }

    public ResourceState SetGoldenFactoryResourceState(FactoryController factoryController, ResourceController resourceController)
    {
        if (factoryController.s_OwnerPlayer == null)
        {
            return ResourceState.Neutral;
        }
        else
        {
            if (resourceController.s_OwnerPlayer == factoryController.s_OwnerPlayer)
            {
                return ResourceState.Positive;
            }
            else
            {
                return ResourceState.Negative;
            }
        }
    }

    #region Calculation Functions

    /// <summary>
    /// Calculates and returns factory controller's buy from bank price
    /// </summary>
    /// <param name="factoryController"></param>
    /// <returns></returns>
    public float CalculateFactoryBuyFromBankPrice(FactoryController factoryController)
    {
        return baseFactoryPrice * factoryController.factoryPriceCoef;
    }

    /// <summary>
    /// Calculates and returns factory controller's sell to bank price
    /// </summary>
    /// <param name="factoryController"></param>
    /// <returns></returns>
    public float CalculateFactorySellToBankPrice(FactoryController factoryController)
    {
        return CalculateFactoryBuyFromBankPrice(factoryController) / 2;
    }

    /// <summary>
    /// Calculates and returns factory controller's rent rate
    /// </summary>
    /// <param name="factoryController"></param>
    /// <returns></returns>
    public float CalculateFactoryRentRate(FactoryController factoryController)
    {
        if (factoryController.s_OwnerPlayer)
        {
            return CalculateFactorySellToAnotherPlayerPrice(factoryController) / 3;
        }
        else
        {
            return 0;
        }
    }

    /// <summary>
    /// Calculates and returns factory controller's sell to another player price
    /// </summary>
    /// <param name="factoryController"></param>
    /// <returns></returns>
    public float CalculateFactorySellToAnotherPlayerPrice(FactoryController factoryController)
    {
        return baseFactoryPrice * factoryPriceCoefPerLevel[factoryController.s_FactoryLevel] * factoryController.factoryPriceCoef * (factoryController.s_Productivity / 100);
    }

    /// <summary>
    /// Calculates and returns resource controller's rent rate
    /// </summary>
    /// <param name="resourceController"></param>
    /// <returns></returns>
    public float CalculateResourceRentRate(ResourceController resourceController)
    {
        if (resourceController.s_OwnerPlayer)
        {
            return resourceRentRate;
        }
        else
        {
            return 0;
        }
    }

    /// <summary>
    /// Calculates and returns factory controller's rent rate
    /// </summary>
    /// <param name="_factoryLevel"></param>
    /// <param name="_factoryPriceCoef"></param>
    /// <param name="_productivity"></param>
    /// <returns></returns>
    public float CalculateFactoryRentRate(int _factoryLevel, float _factoryPriceCoef, float _productivity)
    {
        return CalculateFactorySellToAnotherPlayerPrice(_factoryLevel, _factoryPriceCoef, _productivity) / 3;
    }

    /// <summary>
    /// Calculates and returns factory controller's sell to another player price
    /// </summary>
    /// <param name="_factoryLevel"></param>
    /// <param name="_factoryPriceCoef"></param>
    /// <param name="_productivity"></param>
    /// <returns></returns>
    public float CalculateFactorySellToAnotherPlayerPrice(int _factoryLevel, float _factoryPriceCoef, float _productivity)
    {
        return baseFactoryPrice * factoryPriceCoefPerLevel[_factoryLevel] * _factoryPriceCoef * (_productivity / 100);
    }

    /// <summary>
    /// Calculates and returns productivity by player and production type
    /// </summary>
    /// <param name="buyer"></param>
    /// <param name="_productionType"></param>
    /// <returns></returns>
    public float CalculateProductivityByProductionType(PlayerObjectController buyer, string _productionType)
    {
        float _productivity = 100;
        foreach (ResourceController resourceController in playgroundController.resources)
        {
            if (resourceController.s_ProductionType.ToString() == _productionType)
            {
                if (resourceController.s_OwnerPlayer)
                {
                    if (resourceController.s_OwnerPlayer == buyer)
                    {
                        _productivity += resourceProductivityCoef;
                    }
                    else
                    {
                        _productivity -= resourceProductivityCoef;
                    }
                }
            }
        }
        foreach (Card card in s_ActiveCards)
        {
            switch (card.CardData.Category)
            {
                case CardCategory.Market:
                    if (card.CardData.ProductionType.ToString() == _productionType)
                    {
                        _productivity += card.CardData.ProductivityValue;
                    }
                    break;
            }
        }
        return _productivity;
    }

    /// <summary>
    /// Calculates and returns factory controller's upgrade price
    /// </summary>
    /// <param name="factoryController"></param>
    /// <returns></returns>
    public float CalculateFactoryUpgradePrice(FactoryController factoryController)
    {
        return baseFactoryPrice * factoryPriceCoefPerLevel[factoryController.s_FactoryLevel + 1] * factoryController.factoryPriceCoef;
    }

    /// <summary>
    /// Calculates and returns resource controller's buy from bank price
    /// </summary>
    /// <returns></returns>
    public float CalculateResourceBuyFromBankPrice()
    {
        return resourceBuyPrice;
    }

    /// <summary>
    /// Calculates and returns resource controller's sell to bank price
    /// </summary>
    /// <returns></returns>
    public float CalculateResourceSellToBankPrice()
    {
        return CalculateResourceBuyFromBankPrice() / 2;
    }

    #endregion

    #region Card Functions

    [Command(requiresAuthority = false)]
    public void CmdDrawCardForPlayer(PlayerObjectController player, DeckController deckController)
    {
        int cardIndex = Random.Range(0, deckController.cardCollection.CardsInCollection.Count);
        GameObject cardObject = Instantiate(cardPrefab);
        NetworkServer.Spawn(cardObject, player.connectionToClient);
        Card card = cardObject.GetComponent<Card>();
        //card.SetUpEvent();

        RpcDrawCardForPlayer(player, card, cardIndex, deckController);
    }

    [ClientRpc]
    private void RpcDrawCardForPlayer(PlayerObjectController player, Card card, int cardIndex, DeckController deckController)
    {
        card.transform.localScale = new Vector3(.25f, .25f, .25f);
        card.transform.position = Camera.main.WorldToScreenPoint(deckController.transform.position);
        if (card.isOwned)
        {
            card.s_OwnerPlayer = player;
            card.SetUpUI(deckController.cardCollection.CardsInCollection[cardIndex], false);
            card.SetUpEvent();
            if (card.CardData.Type == CardType.Holdable)
            {
                card.transform.SetParent(uiManager.playerCardContainer.transform);
            }
            else
            {
                card.transform.SetParent(uiManager.mainCanvas.transform);
            }
        }
        else
        {
            card.SetUpUI(deckController.cardCollection.CardsInCollection[cardIndex], true);
            card.SetUpEvent();
            card.transform.SetParent(uiManager.mainCanvas.transform);
            if (card.CardData.Type == CardType.Holdable)
            {
                StartCoroutine(card.cardAnimation.MoveToPlayerUI(player.gamePlayerListItem.transform.position));
            }
            else
            {
                card.transform.SetParent(uiManager.mainCanvas.transform);

            }
        }
        if (card.CardData.Type == CardType.NotHoldable)
        {
            card.PlayCard();
        }


    }

    [Command(requiresAuthority = false)]
    public void CmdPlayCard(PlayerObjectController player, Card card)
    {
        s_ActiveCards.Add(card);
        player.s_PlayerActiveCards.Add(card);
        card.playCardEvent?.Invoke(card);

    }

    [Command(requiresAuthority = false)]
    public void CmdDestroyCard(PlayerObjectController player, Card card)
    {
        s_ActiveCards.Remove(card);
        player.s_PlayerActiveCards.Remove(card);
        card.destroyCardEvent?.Invoke(card);
    }

    [Command(requiresAuthority = false)]
    public void CmdPlayMarketCard(Card card)
    {

        foreach (FactoryController factoryController in playgroundController.allFactories)
        {
            if (factoryController.s_ProductionType == card.CardData.ProductionType)
            {
                CmdApplyMarketCardEffect(factoryController, card);
            }
        }
        CmdUpdateTurnIndex();
        RpcPlayMarketCard(card);
    }

    [ClientRpc]
    private void RpcPlayMarketCard(Card card)
    {
        card.gameObject.SetActive(false);
    }

    [Command(requiresAuthority = false)]
    public void CmdDestroyMarketCard(Card card)
    {
        foreach (FactoryController factoryController in playgroundController.allFactories)
        {
            if (factoryController.s_ProductionType == card.CardData.ProductionType)
            {
                CmdRemoveMarketCardEffect(factoryController, card);
            }
        }
        //Destroy(card);
        //NetworkServer.Destroy(card.gameObject);
    }

    [Command(requiresAuthority = false)]
    public void CmdApplyMarketCardEffect(FactoryController factoryController, Card card)
    {
        factoryController.s_ActiveCards.Add(card);
        factoryController.s_Productivity += card.CardData.ProductivityValue;
        factoryController.s_RentRate = CalculateFactoryRentRate(factoryController);
    }

    [Command(requiresAuthority = false)]
    public void CmdRemoveMarketCardEffect(FactoryController factoryController, Card card)
    {
        factoryController.s_ActiveCards.Remove(card);
        factoryController.s_Productivity -= card.CardData.ProductivityValue;
        factoryController.s_RentRate = CalculateFactoryRentRate(factoryController);
    }



    #endregion

}
