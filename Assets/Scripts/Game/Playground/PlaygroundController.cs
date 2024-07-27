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

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        //GetComponent<NetworkIdentity>().AssignClientAuthority(GameObject.Find("LocalGamePlayer").GetComponent<PlayerObjectController>().connectionToClient);
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Buy Factory
    [Command(requiresAuthority = false)]
    public void CmdBuyFactory(int locationIndex, PlayerObjectController newOwner)
    {
        RpcBuyFactory(locationIndex, newOwner);
    }

    [ClientRpc]
    private void RpcBuyFactory(int locationIndex, PlayerObjectController newOwner)
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

        // If purchasing the factory for the first time, upgrade to level 1
        if (factoryController.factoryLevel == 0)
        {
            factoryController.factoryLevel = 1;
            factoryController.UpdateRentRate();
            factoryController.UpdateLocationValue();
            factoryController.UpdateOwnerPlayer();
        }
        locationController.playerColorMaterial.color = gameManager.playerColors[newOwner.playerColor].color;

    }

    // Upgrade Factory
    [Command(requiresAuthority = false)]
    public void CmdUpgradeFactory(int locationIndex)
    {
        RpcUpgradeFactory(locationIndex);
    }

    [ClientRpc]
    private void RpcUpgradeFactory(int locationIndex)
    {
        FactoryController factoryController = locations[locationIndex].GetComponent<FactoryController>();
        factoryController.factoryLevel++;
        factoryController.UpdateRentRate();
        factoryController.UpdateLocationValue();
    }

    // Buy Resource
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
        resourceController.UpdateRentRate();
        resourceController.UpdateLocationValue();
        resourceController.UpdateOwnerPlayer();
        locationController.playerColorMaterial.color = gameManager.playerColors[newOwner.playerColor].color;

    }

    // Sell Locations To The Bank
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
            factoryController.ownerPlayer = null;
            factoryController.ownerPlayer.ownedLocations.Remove(locationController);
            factoryController.factoryLevel = 0;
            factoryController.UpdateRentRate();
            factoryController.UpdateLocationValue();
            factoryController.UpdateOwnerPlayer();
        }
        else if (locationController.resourceController)
        {
            ResourceController resourceController = locationController.resourceController;
            resourceController.ownerPlayer = null;
            resourceController.ownerPlayer.ownedLocations.Remove(locationController);
            resourceController.UpdateRentRate();
            resourceController.UpdateLocationValue();
            resourceController.UpdateOwnerPlayer();
        }
        locationController.playerColorMaterial.color = new Color(1, 0, 0);
    }
}
