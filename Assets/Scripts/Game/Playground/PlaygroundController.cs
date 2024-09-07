using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class PlaygroundController : NetworkBehaviour
{
    public GameManager gameManager;
    public List<GameObject> locations;
    public GameObject startPoint;

    [Header("Factory Prefabs")]
    public GameObject level0FactoryPrefab;
    public GameObject level1FactoryPrefab;
    public GameObject level2FactoryPrefab;
    public GameObject level3FactoryPrefab;
    public GameObject level4FactoryPrefab;

    [Header("Resource Prefabs")]
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
    public List<LocationController> allLocations;
    public List<ResourceController> resources;
    public List<FactoryController> allFactories;
    public List<FactoryController> goldenFactories;

    public static PlaygroundController Instance { get; private set; }

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
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        uiManager = GameObject.Find("UI Manager").GetComponent<UIManager>();
    }

    [Server]
    public float ServerCalculateFactoryProductivity(FactoryController factoryController)
    {
        float _productivity = 100;
        if (factoryController.s_IsShuttedDown)
        {
            _productivity = 0;
        }
        else
        {
            if (factoryController.locationType == LocationType.GoldenFactory)
            {
                _productivity += ServerCheckFactoryResourceState(factoryController);
            }

            _productivity += ServerCheckFactoryActiveCards(factoryController);
        }

        return _productivity;
    }

    /// <summary>
    /// Checks resource state and sets productivity
    /// </summary>
    /// <param name="factoryController"></param>
    /// <returns></returns>
    [Server]
    public float ServerCheckFactoryResourceState(FactoryController factoryController)
    {
        float _productivity = 0; // Set initial productivity

        // Set productivity by resource state
        switch (factoryController.s_ResourceState)
        {
            case ResourceState.Positive:
                _productivity += gameManager.resourceProductivityCoef;
                break;
            case ResourceState.Negative:
                _productivity -= gameManager.resourceProductivityCoef;
                break;
            case ResourceState.Neutral:
                break;
        }

        return _productivity; // Return calculated productivity
    }

    /// <summary>
    /// Check all active factory cards and sets productivity
    /// </summary>
    /// <param name="factoryController"></param>
    /// <returns></returns>
    [Server]
    public float ServerCheckFactoryActiveCards(FactoryController factoryController)
    {
        float _productivity = 0; // Set initial productivity

        // Set productivity by active cards
        foreach (Card card in factoryController.s_ActiveCards)
        {
            switch (card.CardData.Category)
            {
                case CardCategory.Luck:
                    switch (card.CardData.CardIndex)
                    {
                        case 5:
                            _productivity += card.CardData.ProductivityValue;
                            break;
                    }
                    break;
                case CardCategory.Market:
                    if (card.CardData.ProductionType == factoryController.s_ProductionType)
                    {
                        _productivity += card.CardData.ProductivityValue;
                    }
                    break;
            }
        }

        return _productivity; // Return calculated productivity
    }

    /// <summary>
    /// Clears active cards. Checks all active cards and add if they production type matches
    /// </summary>
    /// <param name="factoryController"></param>
    /// <returns></returns>
    [Server]
    public void ServerSetFactoryActiveCards(FactoryController factoryController)
    {
        foreach (Card card in factoryController.s_ActiveCards)
        {
            switch (card.CardData.Category)
            {
                case CardCategory.Market:
                    factoryController.s_ActiveCards.Remove(card);
                    break;
            }
        }

        foreach (Card card in gameManager.s_ActiveCards)
        {
            switch (card.CardData.Category)
            {
                case CardCategory.Market:
                    if (card.CardData.ProductionType == factoryController.s_ProductionType)
                    {
                        factoryController.s_ActiveCards.Add(card);
                    }
                    break;
            }
        }
    }

    #region Buy Factory Functions
    [Command(requiresAuthority = false)]
    public void CmdBuyFactory(int locationIndex, PlayerObjectController newOwner, string newProductionType)
    {
        // Get references
        LocationController locationController = locations[locationIndex].GetComponent<LocationController>();
        FactoryController factoryController = locations[locationIndex].GetComponent<FactoryController>();

        // Remove old owner
        if (factoryController.s_OwnerPlayer)
        {
            factoryController.s_OwnerPlayer.s_PlayerOwnedLocations.Remove(locationController);
        }

        // Set factory level to 1 if it is 0
        if (factoryController.s_FactoryLevel == 0)
        {
            factoryController.s_FactoryLevel = 1;
        }

        // Set new owner
        factoryController.s_OwnerPlayer = newOwner;
        newOwner.s_PlayerOwnedLocations.Add(locationController);

        // Set production type if golden factory
        if (factoryController.locationType == LocationType.GoldenFactory)
        {
            factoryController.SetProductionType(newProductionType);
            factoryController.s_ResourceState = gameManager.SetGoldenFactoryResourceState(factoryController);
            ServerSetFactoryActiveCards(factoryController);
            factoryController.s_Productivity = ServerCalculateFactoryProductivity(factoryController);
        }

        // Calculate productivity and rent rate
        factoryController.s_RentRate = gameManager.CalculateFactoryRentRate(factoryController);
    }

    #endregion

    #region Upgrade Factory Functions

    [Command(requiresAuthority = false)]
    public void CmdUpgradeFactory(int locationIndex)
    {
        FactoryController factoryController = locations[locationIndex].GetComponent<FactoryController>();
        factoryController.s_FactoryLevel++;
        factoryController.s_RentRate = gameManager.CalculateFactoryRentRate(factoryController);
    }

    #endregion

    #region Buy Resource Functions

    [Command(requiresAuthority = false)]
    public void CmdBuyResource(int locationIndex, PlayerObjectController newOwner)
    {
        ResourceController resourceController = locations[locationIndex].GetComponent<ResourceController>();
        resourceController.s_OwnerPlayer = newOwner;
        newOwner.s_PlayerOwnedLocations.Add(resourceController);
        newOwner.s_PlayerOwnedResources.Add(resourceController);

        foreach (FactoryController factoryController in goldenFactories)
        {
            if (factoryController.s_ProductionType == resourceController.s_ProductionType)
            {
                factoryController.s_ResourceState = gameManager.SetGoldenFactoryResourceState(factoryController, resourceController);
                factoryController.s_Productivity = ServerCalculateFactoryProductivity(factoryController);
                factoryController.s_RentRate = gameManager.CalculateFactoryRentRate(factoryController);
            }
        }

        resourceController.s_RentRate = gameManager.CalculateResourceRentRate(resourceController);
    }

    #endregion

    #region Sell Location To The Bank Functions
    [Command(requiresAuthority = false)]
    public void CmdSellLocationToTheBank(int locationIndex, PlayerObjectController owner)
    {
        LocationController locationController = locations[locationIndex].GetComponent<LocationController>();
        LocationType locationType = locationController.locationType;

        owner.s_PlayerOwnedLocations.Remove(locationController);

        locationController.s_OwnerPlayer = null;

        if (locationType == LocationType.Resource)
        {
            ResourceController resourceController = locations[locationIndex].GetComponent<ResourceController>();
            owner.s_PlayerOwnedResources.Remove(locationController);
            foreach (FactoryController factoryController in goldenFactories)
            {
                if (factoryController.s_ProductionType == resourceController.s_ProductionType)
                {
                    factoryController.s_ResourceState = gameManager.SetGoldenFactoryResourceState(factoryController, resourceController);
                    factoryController.s_Productivity = ServerCalculateFactoryProductivity(factoryController);
                    factoryController.s_RentRate = gameManager.CalculateFactoryRentRate(factoryController);
                }
            }
            resourceController.s_RentRate = gameManager.CalculateResourceRentRate(resourceController);
        }
        else
        {
            FactoryController factoryController = locations[locationIndex].GetComponent<FactoryController>();
            factoryController.s_FactoryLevel = 0;
            if (factoryController.locationType == LocationType.GoldenFactory)
            {
                factoryController.SetProductionType("Default");
                factoryController.s_ResourceState = ResourceState.Neutral;
                ServerSetFactoryActiveCards(factoryController);
                factoryController.s_Productivity = ServerCalculateFactoryProductivity(factoryController);
            }
            else
            {

            }
            factoryController.s_RentRate = gameManager.CalculateFactoryRentRate(factoryController);
        }
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
}

public enum RegionType
{
    Clay,
    Copper,
    Iron,
    Cotton,
    Coal
}

public enum ProductionType
{
    Clay,
    Copper,
    Iron,
    Cotton,
    Coal
}
public enum LocationType
{
    Starting,
    Special,
    Card,
    RegularFactory,
    BigFactory,
    GoldenFactory,
    Resource
}
