using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class FactoryController : LocationController
{
    #region Fields and Properties

    [Header("Factory Controller")]

    [Header("Sync Variables")]
    [SyncVar] public ProductionType s_ProductionType;
    [SyncVar] public float s_Productivity;
    [SyncVar(hook = nameof(SetFactoryLevel))] public int s_FactoryLevel;
    [SyncVar(hook = nameof(SetRentRate))] public float s_RentRate;
    [SyncVar] public ResourceState s_ResourceState;
    [SyncVar] public bool s_IsShuttedDown;

    [Header("References")]
    private GameObject factoryModel;
    private ProductionType defaultProductionType;
    [HideInInspector] public GameObject locationInfoPanel;
    [HideInInspector] private List<Material> factoryOwnerMaterials = new();
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
        locationOwnerMaterial = GetComponent<MeshRenderer>().materials[1];
        locationInfoPanel = UIManager.Instance.locationInfoPanel;
    }

    /// <summary>
    /// Spawns factory prefab
    /// </summary>
    public void SpawnFactoryPrefab()
    {
        factoryModel = Instantiate(playgroundController.level0FactoryPrefab, transform);
        factoryModel.transform.localPosition = new Vector3(.2f, .035f, 0);
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



    #endregion

    #region Set Functions

    public override void UpdateOwnerPlayer(PlayerObjectController newOwner)
    {
        base.UpdateOwnerPlayer(newOwner);
        UpdateFactoryOwnerPlayer(newOwner);
    }

    private void UpdateFactoryOwnerPlayer(PlayerObjectController newOwner)
    {
        if (newOwner != null)
        {
            if (factoryOwnerMaterials.Count > 0)
            {
                foreach (Material material in factoryOwnerMaterials)
                {
                    material.color = newOwner.playerColor;
                }
            }
        }

    }

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

    #endregion

    private void SetFactoryLevel(int oldValue, int newValue)
    {
        if (isServer)
        {
            s_FactoryLevel = newValue;
        }
        if (isClient && oldValue != newValue)
        {
            UpdateFactoryLevel(newValue);
        }
    }

    private void UpdateFactoryLevel(int newValue)
    {
        Destroy(factoryModel);
        factoryOwnerMaterials = new();

        switch (newValue)
        {
            case 0:
                factoryModel = Instantiate(playgroundController.level0FactoryPrefab, transform);
                factoryModel.transform.localPosition = new Vector3(.2f, .035f, 0);
                break;
            case 1:
                factoryModel = Instantiate(playgroundController.level1FactoryPrefab, transform);
                factoryModel.transform.localPosition = new Vector3(.25f, .035f, 0);
                factoryOwnerMaterials.Add(factoryModel.transform.GetChild(1).GetComponent<MeshRenderer>().materials[1]);
                factoryOwnerMaterials.Add(factoryModel.transform.GetChild(10).GetComponent<MeshRenderer>().materials[0]);
                factoryOwnerMaterials.Add(factoryModel.transform.GetChild(19).GetComponent<MeshRenderer>().materials[1]);
                break;
            case 2:
                factoryModel = Instantiate(playgroundController.level2FactoryPrefab, transform);
                factoryModel.transform.localPosition = new Vector3(.25f, .035f, 0);
                factoryOwnerMaterials.Add(factoryModel.transform.GetChild(1).GetComponent<MeshRenderer>().materials[1]);
                factoryOwnerMaterials.Add(factoryModel.transform.GetChild(3).GetComponent<MeshRenderer>().materials[1]);
                factoryOwnerMaterials.Add(factoryModel.transform.GetChild(10).GetComponent<MeshRenderer>().materials[0]);
                factoryOwnerMaterials.Add(factoryModel.transform.GetChild(20).GetComponent<MeshRenderer>().materials[1]);
                break;
            case 3:
                factoryModel = Instantiate(playgroundController.level3FactoryPrefab, transform);
                factoryModel.transform.localPosition = new Vector3(.2f, .035f, 0);
                factoryOwnerMaterials.Add(factoryModel.transform.GetChild(10).GetComponent<MeshRenderer>().materials[1]);
                factoryOwnerMaterials.Add(factoryModel.transform.GetChild(19).GetComponent<MeshRenderer>().materials[1]);
                factoryOwnerMaterials.Add(factoryModel.transform.GetChild(32).GetComponent<MeshRenderer>().materials[1]);
                factoryOwnerMaterials.Add(factoryModel.transform.GetChild(33).GetComponent<MeshRenderer>().materials[1]);
                factoryOwnerMaterials.Add(factoryModel.transform.GetChild(34).GetComponent<MeshRenderer>().materials[1]);
                break;
            case 4:
                factoryModel = Instantiate(playgroundController.level4FactoryPrefab, transform);
                factoryModel.transform.localPosition = new Vector3(.15f, .035f, 0);
                factoryOwnerMaterials.Add(factoryModel.transform.GetChild(3).GetComponent<MeshRenderer>().materials[1]);
                factoryOwnerMaterials.Add(factoryModel.transform.GetChild(4).GetComponent<MeshRenderer>().materials[0]);
                factoryOwnerMaterials.Add(factoryModel.transform.GetChild(7).GetComponent<MeshRenderer>().materials[1]);
                factoryOwnerMaterials.Add(factoryModel.transform.GetChild(28).GetComponent<MeshRenderer>().materials[1]);
                factoryOwnerMaterials.Add(factoryModel.transform.GetChild(29).GetComponent<MeshRenderer>().materials[1]);
                break;
        }
        FactoryModelController factoryModelController = factoryModel.GetComponent<FactoryModelController>();
        UpdateFactoryOwnerPlayer(s_OwnerPlayer);
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
                            eventPanelData1.eventNameText.text = $"{card.CardData.CardName}";
                            break;
                        case CardEffectType.Negative:
                            EventPanelData eventPanelData2 = Instantiate(locationInfoPanelData.negativeEventPrefab.GetComponent<EventPanelData>(), locationInfoPanelData.eventContainer.transform);
                            eventPanelData2.productivityText.text = $"%{card.CardData.ProductivityValue}";
                            eventPanelData2.eventNameText.text = $"{card.CardData.CardName}";
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
