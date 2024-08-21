using UnityEngine;

[RequireComponent(typeof(LocationController))]
public class FactoryController : MonoBehaviour
{
    private GameManager gameManager;

    public LocationController locationController;
    private LocationController.ProductionType productionType;

    public PlayerObjectController ownerPlayer;

    public float factoryPriceCoef;
    public int factoryLevel;
    public int maxFactoryLevel;

    // Start is called before the first frame update
    void Start()
    {
        locationController = GetComponent<LocationController>();
        gameManager = locationController.transform.parent.GetComponent<PlaygroundController>().gameManager;
        productionType = locationController.productionType;
    }

    public float CalculateBuyFromBankPrice()
    {
        return gameManager.baseFactoryPrice * factoryPriceCoef;
    }

    public float CalculateSellToBankPrice()
    {
        return CalculateBuyFromBankPrice() / 2;
    }

    public float CalculateUpgradePrice()
    {
        return gameManager.baseFactoryPrice * gameManager.factoryPriceCoefPerLevel[factoryLevel + 1] * factoryPriceCoef;
    }

    public float CalculateSellToAnotherPrice(int _factoryLevel)
    {
        return gameManager.baseFactoryPrice * gameManager.factoryPriceCoefPerLevel[_factoryLevel] * factoryPriceCoef * (locationController.productivity / 100);
    }

    public float CalculateRentRate(int _factoryLevel)
    {
        return CalculateSellToAnotherPrice(_factoryLevel) / 3;
    }

    public float CalculateRentRateGoldenFactory(int _factoryLevel, float productivity)
    {
        return CalculateSellToAnotherPriceGoldenFactory(_factoryLevel, productivity) / 3;
    }

    public float CalculateSellToAnotherPriceGoldenFactory(int _factoryLevel, float productivity)
    {
        return gameManager.baseFactoryPrice * gameManager.factoryPriceCoefPerLevel[_factoryLevel] * factoryPriceCoef * (productivity / 100);
    }

    public void UpdateOwnerPlayer()
    {
        locationController.ownerPlayer = ownerPlayer;
    }
}
