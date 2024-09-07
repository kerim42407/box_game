using Mirror;
using UnityEngine;

public class FactoryController : LocationController
{
    #region Fields and Properties

    [Header("Factory Controller")]

    [Header("Sync Variables")]
    [SyncVar] public ProductionType s_ProductionType;
    [SyncVar] public float s_Productivity;
    [SyncVar] public int s_FactoryLevel;
    [SyncVar(hook = nameof(SetRentRate))] public float s_RentRate;
    [SyncVar] public ResourceState s_ResourceState;
    [SyncVar] public bool s_IsShuttedDown;


    private ProductionType defaultProductionType;
    [HideInInspector] public GameObject locationInfoPanel;
    #endregion

    [Header("Variables")]
    public int maxFactoryLevel;
    public float factoryPriceCoef;


    #region Methods

    #region Setup Functions

    public override void SetupEmissionController()
    {
        emissionController.material = GetComponent<MeshRenderer>().materials[2];
        emissionController.initialEmissionColor = emissionController.material.GetColor("_EmissionColor");
    }

    public override void SetupLocation()
    {
        base.SetupLocation();
        SpawnLocationNameTextPrefab();
        SpawnRentRateTextPrefab();
        SpawnFactoryPrefab();
        SetupFactoryVariables();
    }

    /// <summary>
    /// Set factory variables according to the Location Type
    /// </summary>
    private void SetupFactoryVariables()
    {
        switch (locationType)
        {
            case LocationType.RegularFactory:
                maxFactoryLevel = 3;
                factoryPriceCoef = gameManager.regularFactoryPriceCoef;
                break;
            case LocationType.BigFactory:
                maxFactoryLevel = 4;
                factoryPriceCoef = gameManager.bigFactoryPriceCoef;
                break;
            case LocationType.GoldenFactory:
                maxFactoryLevel = 4;
                factoryPriceCoef = gameManager.goldenFactoryPriceCoef;
                playgroundController.goldenFactories.Add(this);
                defaultProductionType = s_ProductionType;
                break;
        }
        playgroundController.allFactories.Add(this);
        playerColorMaterial = GetComponent<MeshRenderer>().materials[1];
        locationInfoPanel = UIManager.Instance.locationInfoPanel;
    }

    #endregion

    #region Get Functions

    public override float GetProductivity()
    {
        return s_Productivity;
    }

    public override float GetCalculateSellToBankPrice()
    {
        return gameManager.CalculateFactorySellToBankPrice(this);
    }

    public override float GetRentRate()
    {
        return s_RentRate;
    }

    public override void UpdateOwnerPlayer(PlayerObjectController newOwner)
    {
        base.UpdateOwnerPlayer(newOwner);
    }

    #endregion

    #region Set Functions

    public void SetRentRate(float oldValue, float newValue)
    {
        if (isServer)
        {
            s_RentRate = newValue;
        }
        if (isClient)
        {
            UpdateRentRate(newValue);
        }
    }

    public void UpdateRentRate(float rentRate)
    {
        if (rentRate != 0)
        {
            rentRateText.text = $"{rentRate / 1000}K";
        }
        else
        {
            rentRateText.text = "";
        }
    }
    #endregion

    /// <summary>
    /// Sets production type by input
    /// </summary>
    /// <param name="_productionType"></param>
    public void SetProductionType(string _productionType)
    {
        switch (_productionType)
        {
            case "Clay":
                s_ProductionType = ProductionType.Clay;
                break;
            case "Copper":
                s_ProductionType = ProductionType.Copper;
                break;
            case "Cotton":
                s_ProductionType = ProductionType.Cotton;
                break;
            case "Coal":
                s_ProductionType = ProductionType.Coal;
                break;
            case "Iron":
                s_ProductionType = ProductionType.Iron;
                break;
            case "Default":
                s_ProductionType = defaultProductionType;
                break;
        }
    }

    void OnMouseOver()
    {
        if (playgroundController.gameManager.localPlayerController)
        {
            if (!playgroundController.gameManager.localPlayerController.canSell)
            {
                Vector2 positionOnScreen = Camera.main.WorldToScreenPoint(transform.position);
                LocationInfoPanelData locationInfoPanelData = locationInfoPanel.GetComponent<LocationInfoPanelData>();
                locationInfoPanel.transform.position = positionOnScreen;
                locationInfoPanelData.locationNameText.text = locationName;
                locationInfoPanelData.productivityText.text = $"%{s_Productivity}";
                if (locationInfoPanelData.eventContainer.transform.childCount > 0)
                {
                    foreach (Transform transform in locationInfoPanelData.eventContainer.transform)
                    {
                        Destroy(transform.gameObject);
                    }
                }
                switch (s_ResourceState)
                {
                    case ResourceState.Positive:
                        EventPanelData eventPanelData1 = Instantiate(locationInfoPanelData.positiveEventPrefab.GetComponent<EventPanelData>(), locationInfoPanelData.eventContainer.transform);
                        eventPanelData1.productivityText.text = $"%{gameManager.resourceProductivityCoef}";
                        eventPanelData1.eventNameText.text = "Resource Positive";
                        break;
                    case ResourceState.Negative:
                        EventPanelData eventPanelData2 = Instantiate(locationInfoPanelData.negativeEventPrefab.GetComponent<EventPanelData>(), locationInfoPanelData.eventContainer.transform);
                        eventPanelData2.productivityText.text = $"%{gameManager.resourceProductivityCoef}";
                        eventPanelData2.eventNameText.text = "Resource Negative";
                        break;
                }
                foreach (Card card in s_ActiveCards)
                {
                    switch (card.CardData.EffectType)
                    {
                        case CardEffectType.Positive:
                            EventPanelData eventPanelData1 = Instantiate(locationInfoPanelData.positiveEventPrefab.GetComponent<EventPanelData>(), locationInfoPanelData.eventContainer.transform);
                            eventPanelData1.productivityText.text = $"%{card.CardData.ProductivityValue}";
                            eventPanelData1.eventNameText.text = $"{card.CardData.CardEffectDescription}";
                            break;
                        case CardEffectType.Negative:
                            EventPanelData eventPanelData2 = Instantiate(locationInfoPanelData.negativeEventPrefab.GetComponent<EventPanelData>(), locationInfoPanelData.eventContainer.transform);
                            eventPanelData2.productivityText.text = $"%{card.CardData.ProductivityValue}";
                            eventPanelData2.eventNameText.text = $"{card.CardData.CardEffectDescription}";
                            break;
                    }
                }
                locationInfoPanel.SetActive(true);
            }
        }

    }

    void OnMouseExit()
    {
        if (locationInfoPanel)
        {
            locationInfoPanel.SetActive(false);
        }

    }

    #endregion
}

public enum ResourceState
{
    Positive,
    Neutral,
    Negative
}
