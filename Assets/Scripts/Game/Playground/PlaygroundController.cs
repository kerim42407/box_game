using Mirror;
using System.Collections.Generic;
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
    public GameObject sellLocationTogglePrefab;

    [Header("Prefabs")]
    public GameObject locationInfoPanelPrefab;

    public UIManager uiManager;

    [Header("Location Lists")]
    public List<LocationController> resources;
    public List<LocationController> goldenFactories;



    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        uiManager = GameObject.Find("UI Manager").GetComponent<UIManager>();
        //GetComponent<NetworkIdentity>().AssignClientAuthority(GameObject.Find("LocalGamePlayer").GetComponent<PlayerObjectController>().connectionToClient);
    }

    // Update is called once per frame
    void Update()
    {

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
            foreach (LocationController locationController1 in resources)
            {
                if (locationController1.productionType == locationController.productionType)
                {
                    if (!locationController1.ownerPlayer)
                    {
                        Debug.Log(locationController1 + " has no owner");
                    }
                    else
                    {
                        if (locationController1.ownerPlayer == newOwner)
                        {
                            locationController.productivity += gameManager.resourceProductivityCoef * 100;
                        }
                        else
                        {
                            locationController.productivity -= gameManager.resourceProductivityCoef * 100;
                        }
                    }

                }
            }
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
        foreach (LocationController locationController1 in goldenFactories)
        {
            if (locationController1.productionType == locationController.productionType)
            {
                if (!locationController1.ownerPlayer)
                {
                    Debug.Log(locationController1 + " has no owner");
                }
                else
                {
                    if (locationController1.ownerPlayer == newOwner)
                    {
                        locationController1.productivity += gameManager.resourceProductivityCoef * 100;
                        locationController1.UpdateRentRate();
                    }
                    else
                    {
                        locationController1.productivity -= gameManager.resourceProductivityCoef * 100;
                        locationController1.UpdateRentRate();
                    }
                }
            }
        }
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
            locationController.productivity = 100;
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
            foreach (LocationController locationController1 in goldenFactories)
            {
                if (locationController1.productionType == locationController.productionType)
                {
                    if (!locationController1.ownerPlayer)
                    {
                        Debug.Log(locationController1 + " has no owner");
                    }
                    else
                    {
                        if (locationController1.ownerPlayer == owner)
                        {
                            locationController1.productivity -= gameManager.resourceProductivityCoef * 100;
                            locationController1.UpdateRentRate();
                        }
                        else
                        {
                            locationController1.productivity += gameManager.resourceProductivityCoef * 100;
                            locationController1.UpdateRentRate();
                        }
                    }
                }
            }

            resourceController.ownerPlayer = null;
            locationController.UpdateRentRate();
            resourceController.UpdateOwnerPlayer();
        }
        locationController.playerColorMaterial.color = new Color(1, 0, 0);
    }
    #endregion
}
