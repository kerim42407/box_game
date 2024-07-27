using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LocationController))]
public class FactoryController : MonoBehaviour
{
    private GameManager gameManager;

    public LocationController locationController;
    private LocationController.ProductionType productionType;

    public PlayerObjectController ownerPlayer;

    public float priceMultiplier;
    public float rentRate;
    public int factoryLevel;
    public int maxFactoryLevel;

    // Start is called before the first frame update
    void Start()
    {
        locationController = GetComponent<LocationController>();
        gameManager = locationController.transform.parent.GetComponent<PlaygroundController>().gameManager;
        productionType = locationController.productionType;
    }

    public float CalculateRentRate(int _factoryLevel)
    {
        return gameManager.factoryRentRatePerLevel[_factoryLevel] * priceMultiplier;
    }

    public void UpdateRentRate()
    {
        rentRate = gameManager.factoryRentRatePerLevel[factoryLevel] * priceMultiplier;
        locationController.UpdateRentRate(rentRate);
    }

    public void UpdateLocationValue()
    {
        locationController.locationValue = gameManager.factoryRentRatePerLevel[factoryLevel];
    }

    public void UpdateOwnerPlayer()
    {
        locationController.ownerPlayer = ownerPlayer;
    }
}
