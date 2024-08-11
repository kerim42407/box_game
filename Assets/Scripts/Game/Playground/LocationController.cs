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

    public FactoryController factoryController;
    public ResourceController resourceController;
    private PlaygroundController playgroundController;

    public bool isBuyable;

    [HideInInspector] public GameObject sellLocationToggle;

    public float rentRate;
    public PlayerObjectController ownerPlayer;

    private TextMeshPro rentRateText;

    [Header("References")]
    [HideInInspector] public GameObject locationInfoPanel;

    [Header("Factory Variables")]
    public float productivity;

    private Vector2 positionOnScreen;




    // Start is called before the first frame update
    void Start()
    {
        playgroundController = GetComponentInParent<PlaygroundController>();
        if (locationType == LocationType.RegularFactory || locationType == LocationType.BigFactory || locationType == LocationType.GoldenFactory || locationType == LocationType.Resource)
        {
            playerColorMaterial = GetComponent<MeshRenderer>().materials[1];
            isBuyable = true;
        }
        CheckLocationType();
        positionOnScreen = Camera.main.WorldToScreenPoint(transform.position);
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
    private void CheckLocationType()
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
            locationInfoPanel = playgroundController.uiManager.locationInfoPanel;
            //SpawnLocationInfoPanelPrefab();
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

    void OnMouseOver()
    {
        if (locationInfoPanel)
        {
            LocationInfoPanelController locationInfoPanelController = locationInfoPanel.GetComponent<LocationInfoPanelController>();
            locationInfoPanel.transform.position = positionOnScreen;
            locationInfoPanelController.locationNameText.text = locationName;
            locationInfoPanelController.productivityText.text = $"%{productivity}";
            locationInfoPanel.SetActive(true);
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
