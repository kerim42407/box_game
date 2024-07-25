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
        factoryController.ownerPlayer = newOwner;
        if(factoryController.factoryLevel == 0)
        {
            factoryController.factoryLevel = 1;
            factoryController.UpdateRentRate();
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
    }

    public void CmdBuyResource(int locationIndex, PlayerObjectController newOwner)
    {
        RpcBuyResource(locationIndex, newOwner);
    }

    [ClientRpc]
    private void RpcBuyResource(int locationIndex, PlayerObjectController newOwner)
    {
        LocationController locationController = locations[locationIndex].GetComponent<LocationController>();
        ResourceController resourceController = locationController.GetComponent<ResourceController>();

        resourceController.ownerPlayer = newOwner;
        resourceController.UpdateRentRate();
        locationController.playerColorMaterial.color = gameManager.playerColors[newOwner.playerColor].color;

    }

}
