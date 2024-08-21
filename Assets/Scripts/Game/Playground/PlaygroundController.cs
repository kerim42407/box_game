using Mirror;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PlaygroundController : NetworkBehaviour
{
    public GameManager gameManager;
    public List<GameObject> locations;
    public GameObject startPoint;
    public GameObject level1FactoryPrefab;
    public GameObject level2FactoryPrefab;
    public GameObject level3FactoryPrefab;
    public GameObject level4FactoryPrefab;
    public GameObject clayResourcePrefab;
    public GameObject copperResourcePrefab;
    public GameObject ironResourcePrefab;
    public GameObject cottonResourcePrefab;
    public GameObject coalResourcePrefab;
    public GameObject rentRateTextPrefab;
    public GameObject locationNameTextPrefab;

    [Header("Prefabs")]
    public GameObject locationInfoPanelPrefab;
    public GameObject sellLocationInfoPanelPrefab;

    public UIManager uiManager;

    [Header("Location Lists")]
    public List<LocationController> resources;
    public List<LocationController> allFactories;
    public List<LocationController> goldenFactories;

    public static PlaygroundController Instance { get; private set; }

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
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        uiManager = GameObject.Find("UI Manager").GetComponent<UIManager>();
        foreach (GameObject gameObject in locations)
        {
            gameObject.GetComponent<LocationController>().playgroundController = this;
            gameObject.GetComponent<LocationController>().CheckLocationType();
        }
        //GetComponent<NetworkIdentity>().AssignClientAuthority(GameObject.Find("LocalGamePlayer").GetComponent<PlayerObjectController>().connectionToClient);
    }

    #region Buy Factory Functions
    [Command(requiresAuthority = false)]
    public void CmdBuyFactory(int locationIndex, PlayerObjectController newOwner, string newProductionType)
    {
        RpcBuyFactory(locationIndex, newOwner, newProductionType);
    }

    [ClientRpc]
    private void RpcBuyFactory(int locationIndex, PlayerObjectController newOwner, string newProductionType)
    {
        FactoryController factoryController = locations[locationIndex].GetComponent<FactoryController>();
        LocationController locationController = locations[locationIndex].GetComponent<LocationController>();

        // Remove old owner
        if (factoryController.ownerPlayer)
        {
            factoryController.ownerPlayer.ownedLocations.Remove(locationController);
        }

        // Assign new owner
        factoryController.ownerPlayer = newOwner;
        newOwner.ownedLocations.Add(locationController);

        // Set production type if golden factory
        if (locationController.locationType == LocationController.LocationType.GoldenFactory)
        {
            locationController.SetProductionType(newProductionType);

            foreach(PlayerObjectController player in gameManager.Manager.gamePlayers)
            {
                foreach(Card card in player.playerCards)
                {
                    if(card.CardData.Category == CardCategory.Market)
                    {
                        if(card.CardData.ProductionType == locationController.productionType)
                        {
                            locationController.activeCards.Add(card);
                        }
                        else
                        {
                            if (locationController.activeCards.Contains(card))
                            {
                                locationController.activeCards.Remove(card);
                            }
                        }
                        
                    }
                }
            }
            locationController.UpdateProductivity();
        }

        // If purchasing the factory for the first time, upgrade to level 1
        if (factoryController.factoryLevel == 0)
        {
            factoryController.factoryLevel = 1;
        }

        locationController.UpdateRentRate();
        factoryController.UpdateOwnerPlayer();
        locationController.playerColorMaterial.color = newOwner.playerColor;
    }
    #endregion

    #region Upgrade Factory Functions
    [Command(requiresAuthority = false)]
    public void CmdUpgradeFactory(int locationIndex)
    {
        RpcUpgradeFactory(locationIndex);
    }

    [ClientRpc]
    private void RpcUpgradeFactory(int locationIndex)
    {
        LocationController locationController = locations[locationIndex].GetComponent<LocationController>();
        FactoryController factoryController = locations[locationIndex].GetComponent<FactoryController>();

        factoryController.factoryLevel++;
        locationController.UpdateRentRate();
    }
    #endregion

    #region Buy Resource Functions
    [Command(requiresAuthority = false)]
    public void CmdBuyResource(int locationIndex, PlayerObjectController newOwner)
    {
        RpcBuyResource(locationIndex, newOwner);
    }

    [ClientRpc]
    private void RpcBuyResource(int locationIndex, PlayerObjectController newOwner)
    {
        ResourceController resourceController = locations[locationIndex].GetComponent<ResourceController>();
        LocationController locationController = locations[locationIndex].GetComponent<LocationController>();

        resourceController.ownerPlayer = newOwner;
        newOwner.ownedLocations.Add(locationController);
        newOwner.ownedResources.Add(locationController);

        locationController.UpdateRentRate();
        resourceController.UpdateOwnerPlayer();
        locationController.playerColorMaterial.color = newOwner.playerColor;

    }
    #endregion

    #region Sell Location To The Bank Functions
    [Command(requiresAuthority = false)]
    public void CmdSellLocationToTheBank(int locationIndex, PlayerObjectController owner)
    {
        RpcSellLocationToTheBank(locationIndex, owner);
    }
    [ClientRpc]
    private void RpcSellLocationToTheBank(int locationIndex, PlayerObjectController owner)
    {
        LocationController locationController = locations[locationIndex].GetComponent<LocationController>();
        if (locationController.factoryController)
        {
            FactoryController factoryController = locationController.factoryController;

            factoryController.ownerPlayer.ownedLocations.Remove(locationController);
            factoryController.ownerPlayer = null;
            factoryController.factoryLevel = 0;

            locationController.UpdateRentRate();
            factoryController.UpdateOwnerPlayer();
        }
        else if (locationController.resourceController)
        {
            ResourceController resourceController = locationController.resourceController;

            resourceController.ownerPlayer.ownedLocations.Remove(locationController);
            resourceController.ownerPlayer.ownedResources.Remove(locationController);

            resourceController.ownerPlayer = null;
            locationController.UpdateRentRate();
            resourceController.UpdateOwnerPlayer();
        }
        locationController.playerColorMaterial.color = new Color(1, 0, 0);
    }
    #endregion

    #region Bankruptcy Functions
    [Command(requiresAuthority = false)]
    public void CmdBankruptcy(PlayerObjectController player)
    {
        player.isBankrupt = true;
        gameManager.CmdUpdateTurnIndex();
        RpcBankruptcy(player);
    }
    [ClientRpc]
    private void RpcBankruptcy(PlayerObjectController player)
    {
        player.gamePlayerListItem.playerMoneyText.text = "Bankrupt";
    }
    #endregion

    #region Emission On Location Functions

    [Command(requiresAuthority = false)]
    public void CmdActivateEmissionOnLocation(int locationIndex)
    {
        RpcActivateEmissionOnLocation(locationIndex);
    }

    [ClientRpc]
    private void RpcActivateEmissionOnLocation(int locationIndex)
    {
        LocationController locationController = locations[locationIndex].GetComponent<LocationController>();
        EmissionController emissionController = locationController.GetComponent<EmissionController>();

        emissionController.ActivateEmission();
    }

    [Command(requiresAuthority = false)]
    public void CmdDeactivateEmissionOnLocation(int locationIndex)
    {
        RpcDeactivateEmissionOnLocation(locationIndex);
    }

    [ClientRpc]
    private void RpcDeactivateEmissionOnLocation(int locationIndex)
    {
        LocationController locationController = locations[locationIndex].GetComponent<LocationController>();
        EmissionController emissionController = locationController.GetComponent<EmissionController>();

        emissionController.DeactivateEmission();
    }

    #endregion

    #region Card Functions

    [Command(requiresAuthority = false)]
    public void CmdDrawCard(PlayerObjectController player, int locationIndex, int cardCollectionCount)
    {
        int cardIndex = Random.Range(0, cardCollectionCount);
        RpcDrawCard(player, locationIndex, cardIndex);
    }

    [ClientRpc] 
    private void RpcDrawCard(PlayerObjectController player, int locationIndex, int cardIndex)
    {
        LocationController locationController = locations[locationIndex].GetComponent<LocationController>();
        Deck deck = locationController.deck;
        deck.DrawCard(player, cardIndex);
    }

    #region Market Card Functions

    [Command(requiresAuthority = false)]
    public void CmdPlayCard(PlayerObjectController player, int cardIndex)
    {
        RpcPlayCard(player, cardIndex);
        gameManager.CmdUpdateTurnIndex();
    }

    [ClientRpc]
    private void RpcPlayCard(PlayerObjectController player, int cardIndex)
    {
        player.playerCards[cardIndex].PlayCard(player);
        //player.playerCards[cardIndex]
    }

    [Command(requiresAuthority = false)]
    public void CmdDestroyCard(PlayerObjectController player, int cardIndex)
    {
        RpcDestroyCard(player, cardIndex);
    }

    [ClientRpc]
    public void RpcDestroyCard(PlayerObjectController player, int cardIndex)
    {
        player.playerCards[cardIndex].DestroyCard(player);
    }
    #endregion

    #endregion

    #region Market Card Functions
    //[Command(requiresAuthority = false)]
    //public void CmdApplyMarketCardEffect(PlayerObjectController player, MarketCard marketCard)
    //{
    //    RpcApplyMarketCardEffect(player, marketCard);
    //}
    //[ClientRpc]
    //private void RpcApplyMarketCardEffect(PlayerObjectController player, MarketCard marketCard)
    //{
    //    foreach (LocationController locationController in allFactories)
    //    {
    //        if (locationController.productionType == marketCard.productionType)
    //        {
    //            locationController.activeCards.Add(marketCard);
    //        }
    //    }
    //    gameManager.activeCards.Add(marketCard);
    //    player.activeCards.Add(marketCard);
    //}

    //[Command(requiresAuthority = false)]
    //public void CmdRemoveMarketCardEffect(PlayerObjectController player, MarketCard marketCard)
    //{
    //    RpcRemoveMarketCardEffect(player, marketCard);
    //}
    //[ClientRpc]
    //private void RpcRemoveMarketCardEffect(PlayerObjectController player, MarketCard marketCard)
    //{
    //    foreach(EventBase eventBase in marketCard.activeEvents)
    //    {
    //        //Debug.Log(eventBase.locationIndex);
    //        eventBase.RemoveEvent(locations[eventBase.locationIndex].GetComponent<LocationController>());
    //        Destroy(eventBase);
    //    }
    //    gameManager.activeCards.Remove(marketCard);
    //    player.activeCards.Remove(marketCard);
    //    //player.activeCards.Remove(marketCard);
    //}
    #endregion
}
