using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceController : MonoBehaviour
{
    private GameManager gameManager;
    private LocationController locationController;
    private LocationController.ProductionType productionType;

    public PlayerObjectController ownerPlayer;

    public float rentRate;

    // Start is called before the first frame update
    void Start()
    {
        locationController = GetComponent<LocationController>();
        productionType = locationController.productionType;
        gameManager = locationController.transform.parent.GetComponent<PlaygroundController>().gameManager;
    }

    public float CalculateRentRate()
    {
        return gameManager.resourceRentRate;
    }

    public void UpdateRentRate()
    {
        rentRate = gameManager.resourceRentRate;
        locationController.UpdateRentRate(rentRate);
    }

    public void UpdateLocationValue()
    {
        locationController.locationValue = gameManager.resourceRentRate;
    }

    public void UpdateOwnerPlayer()
    {
        locationController.ownerPlayer = ownerPlayer;
    }
}
