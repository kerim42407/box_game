using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LocationController : MonoBehaviour
{
    public enum ProductionType { Clay, Copper, Iron, Cotton, Coal }
    public ProductionType productionType;
    public enum LocationType { Starting, Special, Card, RegularFactory, BigFactory, GoldenFactory, Resource }
    public LocationType locationType;
    public int locationIndex;
    public string locationName;

    public Material playerColorMaterial;

    

    public float rentRate;
    public PlayerObjectController ownerPlayer;

    private TextMeshPro rentRateText;

    [Header("References")]
    [HideInInspector] public PlaygroundController playgroundController;
    [HideInInspector] public FactoryController factoryController;
    [HideInInspector] public ResourceController resourceController;
    [HideInInspector] public Deck deck;
    [HideInInspector] public EmissionController emissionController;
    [HideInInspector] public GameObject locationInfoPanel;

    [Header("Factory Variables")]
    public float productivity;

    [Header("Events")]
    public List<EventBase> events;

    [Header("Active Cards")]
    public List<Card> activeCards;

    [HideInInspector] public SellLocationInfoPanelData sellLocationInfoPanelData;

    // Start is called before the first frame update
    void Start()
    {
        if (locationType == LocationType.RegularFactory || locationType == LocationType.BigFactory || locationType == LocationType.GoldenFactory || locationType == LocationType.Resource)
        {
            playerColorMaterial = GetComponent<MeshRenderer>().materials[1];
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    #region Productivity Functions
    public float CheckResource(string productionType, PlayerObjectController playerObjectController)
    {
        foreach (LocationController locationController1 in playgroundController.resources)
        {
            if (locationController1.productionType.ToString() == productionType)
            {
                if (locationController1.ownerPlayer == null)
                {
                    return 0;
                }
                else if (locationController1.ownerPlayer == playerObjectController)
                {
                    return playgroundController.gameManager.resourceProductivityCoef;
                }
                else
                {
                    return -playgroundController.gameManager.resourceProductivityCoef;
                }
            }
        }
        return 0;
    }

    public void UpdateProductivity(float value)
    {
        productivity += value;
    }
    #endregion

    #region Calculate Functions
    public float GetLocationBuyFromBankPrice()
    {
        if (factoryController)
        {
            return factoryController.CalculateBuyFromBankPrice();
        }
        else
        {
            return resourceController.CalculateBuyFromBankPrice();
        }
    }
    public float GetLocationSellPriceToTheBank()
    {
        if (factoryController)
        {
            return factoryController.CalculateSellToBankPrice();
        }
        else
        {
            return resourceController.CalculateSellToBankPrice();
        }
    }
    public float GetLocationRentRate()
    {
        if (factoryController)
        {
            return factoryController.CalculateRentRate(factoryController.factoryLevel);
        }
        else
        {
            return resourceController.CalculateRentRate();
        }
    }
    public void UpdateRentRate()
    {
        rentRate = GetLocationRentRate();
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

    /// <summary> asd </summary>///
    public void CheckLocationType()
    {
        if (locationType == LocationType.Starting)
        {
            SpawnLocationNameTextPrefab();
        }
        else if (locationType == LocationType.Special)
        {
            SpawnLocationNameTextPrefab();
        }
        else if (locationType == LocationType.Card)
        {
            SpawnLocationNameTextPrefab();
            deck = GetComponent<Deck>();
            deck.playerCardContainer = playgroundController.uiManager.playerCardContainer;
        }
        else if (locationType == LocationType.RegularFactory)
        {
            productivity = 100;
            factoryController = GetComponent<FactoryController>();
            factoryController.factoryPriceCoef = playgroundController.gameManager.regularFactoryPriceCoef;
            factoryController.maxFactoryLevel = 3;
            Instantiate(SpawnFactory(1), transform);
            SpawnRentRateTextPrefab();
            SpawnLocationNameTextPrefab();
            playgroundController.allFactories.Add(this);
            locationInfoPanel = playgroundController.uiManager.locationInfoPanel;
        }
        else if (locationType == LocationType.BigFactory)
        {
            productivity = 100;
            factoryController = GetComponent<FactoryController>();
            factoryController.factoryPriceCoef = playgroundController.gameManager.bigFactoryPriceCoef;
            factoryController.maxFactoryLevel = 3;
            Instantiate(SpawnFactory(1), transform);
            SpawnRentRateTextPrefab();
            SpawnLocationNameTextPrefab();
            playgroundController.allFactories.Add(this);
            locationInfoPanel = playgroundController.uiManager.locationInfoPanel;
            //SpawnLocationInfoPanelPrefab();
        }
        else if (locationType == LocationType.GoldenFactory)
        {
            productivity = 100;
            factoryController = GetComponent<FactoryController>();
            playgroundController.goldenFactories.Add(this);
            factoryController.factoryPriceCoef = playgroundController.gameManager.goldenFactoryPriceCoef;
            factoryController.maxFactoryLevel = 4;
            Instantiate(SpawnFactory(1), transform);
            SpawnRentRateTextPrefab();
            SpawnLocationNameTextPrefab();
            playgroundController.allFactories.Add(this);
            locationInfoPanel = playgroundController.uiManager.locationInfoPanel;
            //SpawnLocationInfoPanelPrefab();
        }
        else if (locationType == LocationType.Resource)
        {
            resourceController = GetComponent<ResourceController>();
            playgroundController.resources.Add(this);
            Instantiate(SpawnResource(), transform);
            SpawnRentRateTextPrefab();
            SpawnLocationNameTextPrefab();
        }
        SetEmissionControllerVariables();
    }
    private GameObject SpawnFactory(int factoryLevel)
    {
        if (factoryLevel == 1)
        {
            return playgroundController.level1FactoryPrefab;
        }
        else if (factoryLevel == 2)
        {
            return playgroundController.level2FactoryPrefab;
        }
        else if (factoryLevel == 3)
        {
            return playgroundController.level3FactoryPrefab;
        }
        else
        {
            return playgroundController.level4FactoryPrefab;
        }
    }
    private GameObject SpawnResource()
    {
        if (productionType == ProductionType.Clay)
        {
            return playgroundController.clayResourcePrefab;
        }
        else if (productionType == ProductionType.Copper)
        {
            return playgroundController.copperResourcePrefab;
        }
        else if (productionType == ProductionType.Iron)
        {
            return playgroundController.ironResourcePrefab;
        }
        else if (productionType == ProductionType.Cotton)
        {
            return playgroundController.cottonResourcePrefab;
        }
        else
        {
            return playgroundController.coalResourcePrefab;

        }

        //public void UpdatePlayerColorMaterial()
        //{
        //    GetComponent<MeshRenderer>().materials[1].color = playerColorMaterial.color;
        //}
    }
    private void SpawnRentRateTextPrefab()
    {
        if (productionType == ProductionType.Clay)
        {
            rentRateText = Instantiate(playgroundController.rentRateTextPrefab, transform).GetComponent<TextMeshPro>();
        }
        else if (productionType == ProductionType.Copper)
        {
            rentRateText = Instantiate(playgroundController.rentRateTextPrefab, transform).GetComponent<TextMeshPro>();
        }
        else if (productionType == ProductionType.Iron)
        {
            rentRateText = Instantiate(playgroundController.rentRateTextPrefab, transform).GetComponent<TextMeshPro>();
        }
        else if (productionType == ProductionType.Cotton)
        {
            rentRateText = Instantiate(playgroundController.rentRateTextPrefab, transform).GetComponent<TextMeshPro>();
        }
        else
        {
            rentRateText = Instantiate(playgroundController.rentRateTextPrefab, transform).GetComponent<TextMeshPro>();
        }
        rentRateText.transform.localPosition = new Vector3(.675f, 0, 0);
        rentRateText.transform.localEulerAngles = GetRentRateTextRotation();
    }
    private void SpawnLocationNameTextPrefab()
    {
        GameObject locationNameText;
        if (productionType == ProductionType.Clay)
        {
            locationNameText = Instantiate(playgroundController.locationNameTextPrefab, transform);
            locationNameText.transform.localPosition = new Vector3(-0.625f, 0.0065f, 0);
        }
        else if (productionType == ProductionType.Copper)
        {
            locationNameText = Instantiate(playgroundController.locationNameTextPrefab, transform);
            locationNameText.transform.localPosition = new Vector3(-0.64f, 0.135f, 0);
        }
        else if (productionType == ProductionType.Iron)
        {
            locationNameText = Instantiate(playgroundController.locationNameTextPrefab, transform);
            locationNameText.transform.localPosition = new Vector3(-0.64f, 0.135f, 0);
        }
        else if (productionType == ProductionType.Cotton)
        {
            locationNameText = Instantiate(playgroundController.locationNameTextPrefab, transform);
            locationNameText.transform.localPosition = new Vector3(-0.64f, 0.135f, 0);
        }
        else
        {
            locationNameText = Instantiate(playgroundController.locationNameTextPrefab, transform);
            locationNameText.transform.localPosition = new Vector3(-0.625f, 0.0065f, 0);
        }

        locationNameText.transform.localEulerAngles = GetLocationNameTextRotation();
        locationNameText.GetComponent<TextMeshPro>().text = locationName;
    }
    private Vector3 GetRentRateTextRotation()
    {
        if (productionType == ProductionType.Clay)
        {
            return new Vector3(90, 90, 0);
        }
        else if (productionType == ProductionType.Copper)
        {
            return new Vector3(90, -90, 0);
        }
        else if (productionType == ProductionType.Iron)
        {
            return new Vector3(90, -90, 0);
        }
        else if (productionType == ProductionType.Cotton)
        {
            return new Vector3(90, -90, 0);
        }
        else
        {
            return new Vector3(90, 90, 0);

        }
    }
    private Vector3 GetLocationNameTextRotation()
    {
        if (productionType == ProductionType.Clay)
        {
            return new Vector3(75, 90, 0);
        }
        else if (productionType == ProductionType.Copper)
        {
            return new Vector3(50, -90, 0);
        }
        else if (productionType == ProductionType.Iron)
        {
            return new Vector3(50, -90, 0);
        }
        else if (productionType == ProductionType.Cotton)
        {
            return new Vector3(50, -90, 0);
        }
        else
        {
            return new Vector3(75, 90, 0);
        }
    }
    public void SetProductionType(string _productionType)
    {
        if (_productionType == "Clay")
        {
            productionType = ProductionType.Clay;
        }
        else if (_productionType == "Copper")
        {
            productionType = ProductionType.Copper;
        }
        else if (_productionType == "Iron")
        {
            productionType = ProductionType.Iron;
        }
        else if (_productionType == "Cotton")
        {
            productionType = ProductionType.Cotton;
        }
        else
        {
            productionType = ProductionType.Coal;
        }
    }
    private void SetEmissionControllerVariables()
    {
        emissionController = GetComponent<EmissionController>();

        if(locationType == LocationType.Starting || locationType == LocationType.Special)
        {
            emissionController.material = GetComponent<MeshRenderer>().materials[1];
        }
        else
        {
            emissionController.material = GetComponent<MeshRenderer>().materials[2];
        }
        emissionController.initialEmissionColor = emissionController.material.GetColor("_EmissionColor");

    }
    void OnMouseOver()
    {
        if (playgroundController.gameManager.localPlayerController)
        {
            if (locationInfoPanel && !playgroundController.gameManager.localPlayerController.canSell)
            {
                Vector2 positionOnScreen = Camera.main.WorldToScreenPoint(transform.position);
                LocationInfoPanelData locationInfoPanelData = locationInfoPanel.GetComponent<LocationInfoPanelData>();
                locationInfoPanel.transform.position = positionOnScreen;
                locationInfoPanelData.locationNameText.text = locationName;
                locationInfoPanelData.productivityText.text = $"%{productivity}";
                if (locationInfoPanelData.eventContainer.transform.childCount > 0)
                {
                    foreach (Transform transform in locationInfoPanelData.eventContainer.transform)
                    {
                        Destroy(transform.gameObject);
                    }
                }
                //foreach (EventBase eventBase in events)
                //{
                //    if (eventBase.eventType == EventType.Positive)
                //    {
                //        EventPanelData eventPanelData = Instantiate(locationInfoPanelData.positiveEventPrefab.GetComponent<EventPanelData>(), locationInfoPanelData.eventContainer.transform);
                //        eventPanelData.productivityText.text = $"{eventBase.value}";
                //        eventPanelData.eventNameText.text = eventBase.eventName;
                //    }
                //    else if (eventBase.eventType == EventType.Negative)
                //    {
                //        EventPanelData eventPanelData = Instantiate(locationInfoPanelData.negativeEventPrefab.GetComponent<EventPanelData>(), locationInfoPanelData.eventContainer.transform);
                //        eventPanelData.productivityText.text = $"{eventBase.value}";
                //        eventPanelData.eventNameText.text = eventBase.eventName;
                //    }
                //}

                foreach(Card card in activeCards)
                {
                    EventPanelData eventPanelData = Instantiate(locationInfoPanelData.positiveEventPrefab.GetComponent<EventPanelData>(), locationInfoPanelData.eventContainer.transform);
                    eventPanelData.productivityText.text = $"%{card.CardData.ProductivityValue}";
                    eventPanelData.eventNameText.text = card.CardData.CardName;
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
}
