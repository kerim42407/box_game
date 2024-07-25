using Mirror;
using UnityEngine;

public class LocationController : MonoBehaviour
{
    public enum ProductionType { Clay, Copper, Iron, Cotton, Coal }
    public ProductionType productionType;
    public enum LocationType { Starting, Special, Card, RegularFactory, BigFactory, GoldenFactory, Resource }
    public LocationType locationType;

    public string locationName;

    public Material playerColorMaterial;

    private FactoryController factoryController;
    private PlaygroundController playgroundController;

    public bool isBuyable;

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

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void SetProductionType()
    {
        if (locationType == LocationType.RegularFactory || locationType == LocationType.BigFactory || locationType == LocationType.GoldenFactory)
        {
            factoryController = GetComponent<FactoryController>();
        }
    }

    private void CheckLocationType()
    {
        if(locationType == LocationType.Starting)
        {

        }
        else if(locationType == LocationType.Special)
        {

        }
        else if(locationType == LocationType.Card)
        {

        }
        else if(locationType == LocationType.RegularFactory)
        {
            factoryController = GetComponent<FactoryController>();
            factoryController.priceMultiplier = playgroundController.gameManager.regularFactoryPriceMultiplier;
            factoryController.maxFactoryLevel = 3;
            Instantiate(SpawnFactory(1),transform);
        }
        else if(locationType == LocationType.BigFactory)
        {
            factoryController = GetComponent<FactoryController>();
            factoryController.priceMultiplier = playgroundController.gameManager.bigFactoryPriceMultiplier;
            factoryController.maxFactoryLevel = 3;
            Instantiate(SpawnFactory(1), transform);
        }
        else if(locationType == LocationType.GoldenFactory)
        {
            factoryController = GetComponent<FactoryController>();
            factoryController.priceMultiplier = playgroundController.gameManager.goldenFactoryPriceMultiplier;
            factoryController.maxFactoryLevel = 4;
            Instantiate(SpawnFactory(1), transform);
        }
        else if(locationType == LocationType.Resource)
        {

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

    //public void UpdatePlayerColorMaterial()
    //{
    //    GetComponent<MeshRenderer>().materials[1].color = playerColorMaterial.color;
    //}
}
