using Mirror;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;



public class GameManager : NetworkBehaviour
{
    [Header("References")]
    public GameObject mainLight;
    public UIManager uiManager;
    public DiceThrower diceThrower;
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

    [Header("Debug References")]
    public TMP_InputField inputField;
    public GameObject customDiceButton;

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
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
        Manager.gamePlayers[turnIndex].canPlayCard = true;
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

    /// <summary>
    /// Calls CmdRollDice function if player can play
    /// </summary>
    public void RollDice()
    {
        if (localPlayerController.playerInputController.canThrow)
        {
            CmdRollDice();
            uiManager.yourTurnNotification.Close();
        }
    }

    /// <summary>
    /// Rolls dice
    /// </summary>
    [Command(requiresAuthority = false)]
    public void CmdRollDice()
    {
        diceThrower.RollDice();
    }

    /// <summary>
    /// Moves player on dice result
    /// </summary>
    /// <param name="result"></param>
    /// <param name="isEven"></param>
    public void OnDiceResult(int result, bool isEven)
    {
        RpcShowNotification(result.ToString());
        //uiManager.diceResultNotification.description = result.ToString();
        //uiManager.diceResultNotification.UpdateUI();
        //uiManager.diceResultNotification.Open();
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

    [ClientRpc]
    public void RpcShowNotification(string description)
    {
        uiManager.diceResultNotification.description = description;
        uiManager.diceResultNotification.UpdateUI();
        uiManager.diceResultNotification.Open();
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
        Manager.gamePlayers[turnIndex].playerMoveController.SellTest(Manager.gamePlayers[turnIndex].connectionToClient, 10000);
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
                if (card.count == card.CardData.CardDuration)
                {
                    CmdDestroyCard(Manager.gamePlayers[turnIndex], card);
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
                Manager.gamePlayers[i].playerTurnIndicator.SetActive(true);
                Manager.gamePlayers[i].canPlayCard = true;
                Manager.gamePlayers[i].canPlay = true;
                Manager.gamePlayers[i].playerInputController.canThrow = true;
            }
            else
            {
                Manager.gamePlayers[i].playerTurnIndicator.SetActive(false);
                Manager.gamePlayers[i].canPlayCard = false;
                Manager.gamePlayers[i].canPlay = false;
                Manager.gamePlayers[i].playerInputController.canThrow = false;
            }
        }
    }

    #region Notification Event Functions
    [TargetRpc]
    private void RpcShowNotification(NetworkConnectionToClient target)
    {
        uiManager.yourTurnNotification.Open();
    }
    #endregion

    public void Testttt()
    {
        uiManager.yourTurnNotification.Open();
    }

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
        // Return neutral if factory or resource has no owner
        if (factoryController.s_OwnerPlayer == null || resourceController.s_OwnerPlayer == null)
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
            if (factoryController.s_IsShuttedDown)
            {
                return 0;
            }
            else
            {
                return CalculateFactorySellToAnotherPlayerPrice(factoryController) / 3;
            }
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

    #region Market Card Functions

    [Command(requiresAuthority = false)]
    public void CmdDrawCardForPlayer(PlayerObjectController player, DeckController deckController)
    {
        int cardIndex = Random.Range(0, deckController.cardCollection.CardsInCollection.Count);
        GameObject cardObject = Instantiate(cardPrefab);
        NetworkServer.Spawn(cardObject, player.connectionToClient);
        Card card = cardObject.GetComponent<Card>();
        card.s_OwnerPlayer = player;
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
            //card.s_OwnerPlayer = player;
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
        else
        {
            // If is local player and card type is holdable, update turn index
            if (card.isOwned)
            {
                CmdUpdateTurnIndex();
            }
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdPlayHoldableCard(PlayerObjectController player, Card card)
    {
        RpcPlayHoldableCard(player, card);
    }

    [ClientRpc]
    private void RpcPlayHoldableCard(PlayerObjectController player, Card card)
    {
        card.PlayCard();
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
        RpcDeactivateCard(card);
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

    #region Luck Card Functions

    #region Strong Storm Card Functions (index == 3)

    [Command(requiresAuthority = false)]
    public void CmdPlayStrongStormCard(Card card)
    {
        List<FactoryController> selectableFactories = new(); // Create list for selectable factories

        // Set list for selectable factories
        foreach (FactoryController factoryController in playgroundController.allFactories)
        {
            if(factoryController.s_OwnerPlayer == card.s_OwnerPlayer)
            {
                selectableFactories.Add(factoryController);
            }
        }

        // Apply card effect if list is not empty
        if(selectableFactories.Count != 0)
        {
            int index = Random.Range(0, selectableFactories.Count);
            FactoryController selectedFactory = selectableFactories[index];

            if (selectedFactory.s_FactoryLevel > 1)
            {
                selectedFactory.s_FactoryLevel--;
                CalculateFactoryRentRate(selectedFactory);
            }
            else
            {
                playgroundController.CmdSellLocationToTheBank(selectedFactory.locationIndex, selectedFactory.s_OwnerPlayer);
            }
        }
        Destroy(card.gameObject);
        CmdUpdateTurnIndex();
    }

    #endregion

    #region Fertile Soils Card Functions (index == 5)

    [Command(requiresAuthority = false)]
    public void CmdPlayFertileSoilsCard(Card card)
    {
        List<FactoryController> selectableFactories = new(); // Create list for selectable factories

        // Set list for selectable factories
        foreach (FactoryController factoryController in playgroundController.allFactories)
        {
            if (factoryController.s_OwnerPlayer == card.s_OwnerPlayer)
            {
                selectableFactories.Add(factoryController);
            }
        }

        // Apply card effect if list is not empty
        if (selectableFactories.Count != 0)
        {
            int index = Random.Range(0, selectableFactories.Count);
            FactoryController selectedFactory = selectableFactories[index];

            selectedFactory.s_ActiveCards.Add(card);
            selectedFactory.s_Productivity = playgroundController.ServerCalculateFactoryProductivity(selectedFactory);
            selectedFactory.s_RentRate = CalculateFactoryRentRate(selectedFactory);
        }
        RpcDeactivateCard(card);
    }

    [Command(requiresAuthority = false)]
    public void CmdDestroyFertileSoilsCard(Card card)
    {
        foreach (FactoryController factoryController in playgroundController.allFactories)
        {
            if (factoryController.s_ActiveCards.Contains(card))
            {
                CmdRemoveFertileSoilsCardEffect(factoryController, card);
            }
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdRemoveFertileSoilsCardEffect(FactoryController factoryController, Card card)
    {
        factoryController.s_ActiveCards.Remove(card);
        factoryController.s_Productivity = playgroundController.ServerCalculateFactoryProductivity(factoryController);
        factoryController.s_RentRate = CalculateFactoryRentRate(factoryController);
    }

    #endregion

    #endregion

    #region Sabotage Card Functions

    #region Suspicious Fire Card Functions (index == 3)

    /// <summary>
    /// Checks if player can play suspicious fire card
    /// </summary>
    /// <param name="card"></param>
    /// <returns></returns>
    public bool CheckSuspiciousFirePlayable(Card card)
    {
        foreach (LocationController locationController in playgroundController.allFactories)
        {
            if (locationController.s_OwnerPlayer != null && locationController.s_OwnerPlayer != card.s_OwnerPlayer)
            {
                return true;
            }
        }
        return false;
    }

    [Command(requiresAuthority = false)]
    public void CmdPlaySuspiciousFireCard(Card card)
    {
        TRpcPlaySuspiciousFireCard(card.s_OwnerPlayer.connectionToClient, card);
    }

    [TargetRpc]
    private void TRpcPlaySuspiciousFireCard(NetworkConnectionToClient target, Card card)
    {
        foreach (LocationController locationController in playgroundController.allLocations)
        {
            if(locationController.locationType == LocationType.RegularFactory || locationController.locationType == LocationType.BigFactory || locationController.locationType == LocationType.GoldenFactory)
            {
                if(locationController.s_OwnerPlayer != null && locationController.s_OwnerPlayer != card.s_OwnerPlayer)
                {
                    locationController.IndicateLocation(true);
                    locationController.playedCard = card;
                    locationController.onClickEvent.AddListener(CmdApplySuspiciousFireCardEffect);
                }
                else
                {
                    locationController.IndicateLocation(false);
                }
            }
            else
            {
                locationController.IndicateLocation(false);
            }
        }
        mainLight.SetActive(false);
    }

    /// <summary>
    /// Applies suspicious fire card effect to selected location
    /// </summary>
    /// <param name="locationController"></param>
    /// <param name="card"></param>
    [Command(requiresAuthority = false)]
    public void CmdApplySuspiciousFireCardEffect(LocationController locationController, Card card)
    {
        FactoryController factoryController = playgroundController.locations[locationController.locationIndex].GetComponent<FactoryController>();

        if(factoryController.s_FactoryLevel == 1)
        {
            playgroundController.CmdSellLocationToTheBank(locationController.locationIndex,locationController.s_OwnerPlayer);
            
        }
        else
        {
            factoryController.s_FactoryLevel--;
            factoryController.s_RentRate = CalculateFactoryRentRate(factoryController);
        }

        TRpcResetLocationIndicators(card.s_OwnerPlayer.connectionToClient);
        Destroy(card.gameObject);
    }

    #endregion

    #region Resource Disaster Card Functions (index == 4)

    /// <summary>
    /// Checks if player can play resource disaster card
    /// </summary>
    /// <param name="card"></param>
    /// <returns></returns>
    public bool CheckResourceDisasterPlayable(Card card)
    {
        foreach (LocationController locationController in playgroundController.resources)
        {
            if (locationController.s_OwnerPlayer != null && locationController.s_OwnerPlayer != card.s_OwnerPlayer)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Tells server to play resource disaster card
    /// </summary>
    /// <param name="card"></param>
    [Command(requiresAuthority = false)]
    public void CmdPlayResourceDisasterCard(Card card)
    {
        TRpcPlayResourceDisasterCard(card.s_OwnerPlayer.connectionToClient, card);
    }

    /// <summary>
    /// Activates UI and events on target player for resource disaster card
    /// </summary>
    /// <param name="target"></param>
    /// <param name="card"></param>
    [TargetRpc]
    private void TRpcPlayResourceDisasterCard(NetworkConnectionToClient target, Card card)
    {
        foreach (LocationController locationController in playgroundController.allLocations)
        {
            if (locationController.locationType != LocationType.Resource)
            {
                locationController.IndicateLocation(false);
            }
            else
            {
                if (locationController.s_OwnerPlayer != null && locationController.s_OwnerPlayer != card.s_OwnerPlayer)
                {
                    locationController.IndicateLocation(true);
                    locationController.playedCard = card;
                    locationController.onClickEvent.AddListener(CmdApplyResourceDisasterCardEffect);
                }
                else
                {
                    locationController.IndicateLocation(false);
                }
            }
        }
        mainLight.SetActive(false);
    }

    /// <summary>
    /// Applies resource disaster card effect to selected location
    /// </summary>
    /// <param name="locationController"></param>
    /// <param name="card"></param>
    [Command(requiresAuthority = false)]
    public void CmdApplyResourceDisasterCardEffect(LocationController locationController, Card card)
    {
        locationController.s_OwnerPlayer.s_PlayerOwnedLocations.Remove(locationController);
        locationController.s_OwnerPlayer.s_PlayerOwnedResources.Remove(locationController);

        locationController.s_OwnerPlayer = null;

        Debug.Log(locationController.locationIndex);
        ResourceController resourceController = playgroundController.locations[locationController.locationIndex].GetComponent<ResourceController>();
        foreach (FactoryController factoryController in playgroundController.goldenFactories)
        {
            if (factoryController.s_ProductionType == resourceController.s_ProductionType)
            {
                factoryController.s_ResourceState = SetGoldenFactoryResourceState(factoryController, resourceController);
                factoryController.s_Productivity = playgroundController.ServerCalculateFactoryProductivity(factoryController);
                factoryController.s_RentRate = CalculateFactoryRentRate(factoryController);
            }
        }
        resourceController.s_RentRate = CalculateResourceRentRate(resourceController);

        TRpcResetLocationIndicators(card.s_OwnerPlayer.connectionToClient);
        Destroy(card.gameObject);
    }

    #endregion

    #region Factory Shutdown Card Functions (index  == 5)

    public bool CheckFactoryShutdownPlayable(Card card)
    {
        foreach (LocationController locationController in playgroundController.allFactories)
        {
            if (locationController.s_OwnerPlayer != null && locationController.s_OwnerPlayer != card.s_OwnerPlayer)
            {
                return true;
            }
        }
        return false;
    }

    [Command(requiresAuthority = false)]
    public void CmdPlayFactoryShutdownCard(Card card)
    {
        TRpcPlayFactoryShutdownCard(card.s_OwnerPlayer.connectionToClient, card);
    }

    [TargetRpc]
    private void TRpcPlayFactoryShutdownCard(NetworkConnectionToClient target, Card card)
    {
        foreach(LocationController locationController in playgroundController.allLocations)
        {
            if(locationController.locationType == LocationType.RegularFactory || locationController.locationType == LocationType.BigFactory || locationController.locationType == LocationType.GoldenFactory)
            {
                if (locationController.s_OwnerPlayer != null && locationController.s_OwnerPlayer != card.s_OwnerPlayer)
                {
                    locationController.IndicateLocation(true);
                    locationController.playedCard = card;
                    locationController.onClickEvent.AddListener(CmdApplyFactoryShutdownCardEffect);
                }
                else
                {
                    locationController.IndicateLocation(false);
                }
            }
            else
            {
                locationController.IndicateLocation(false);
            }
        }
        mainLight.SetActive(false);
    }

    [Command(requiresAuthority = false)]
    public void CmdApplyFactoryShutdownCardEffect(LocationController locationController, Card card)
    {
        FactoryController factoryController = playgroundController.locations[locationController.locationIndex].GetComponent<FactoryController>();

        factoryController.s_ActiveCards.Add(card);
        factoryController.s_IsShuttedDown = true;
        factoryController.s_Productivity = playgroundController.ServerCalculateFactoryProductivity(factoryController);
        factoryController.s_RentRate = CalculateFactoryRentRate(factoryController);
        TRpcResetLocationIndicators(card.s_OwnerPlayer.connectionToClient);
        RpcDeactivateCard(card);
    }

    [Command(requiresAuthority = false)]
    public void CmdDestroyFactoryShutdownCard(Card card)
    {
        foreach (FactoryController factoryController in playgroundController.allFactories)
        {
            if (factoryController.s_ActiveCards.Contains(card))
            {
                CmdRemoveFactoryShutdownCardEffect(factoryController, card);
            }
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdRemoveFactoryShutdownCardEffect(FactoryController factoryController, Card card)
    {
        factoryController.s_ActiveCards.Remove(card);
        factoryController.s_IsShuttedDown = false;
        factoryController.s_Productivity = playgroundController.ServerCalculateFactoryProductivity(factoryController);
        factoryController.s_RentRate = CalculateFactoryRentRate(factoryController);
    }

    #endregion

    #endregion

    /// <summary>
    /// Deactivates UI and reset events on target player
    /// </summary>
    /// <param name="target"></param>
    [TargetRpc]
    private void TRpcResetLocationIndicators(NetworkConnectionToClient target)
    {
        foreach (LocationController locationController in playgroundController.allLocations)
        {
            locationController.onClickEvent.RemoveAllListeners();
            locationController.playedCard = null;
            locationController.ResetIndicateLocation();
        }
        mainLight.SetActive(true);
        localPlayerController.playerInputController.canThrow = true;
    }

    /// <summary>
    /// Deactivates card on all clients
    /// </summary>
    /// <param name="card"></param>
    [ClientRpc]
    private void RpcDeactivateCard(Card card)
    {
        card.gameObject.SetActive(false);
    }

    #endregion

}
