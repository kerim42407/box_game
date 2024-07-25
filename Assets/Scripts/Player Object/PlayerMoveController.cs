using Mirror;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerMoveController : NetworkBehaviour
{
    public GameObject playerModel;

    // Cosmetics
    public MeshRenderer playerMesh;
    public Material[] playerColors;

    [HideInInspector] public bool shouldMove;
    [HideInInspector] public int destinationIndex;
    [HideInInspector] public List<Transform> destinationTransforms;
    [HideInInspector] public Transform firstTransform;
    [HideInInspector] public float startTime;
    public float journeyTime = .1f;

    private PlayerObjectController playerObjectController;
    private PlaygroundController playgroundController;

    public bool didCosmetic;


    // Start is called before the first frame update
    void Start()
    {
        playerObjectController = GetComponent<PlayerObjectController>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        if (shouldMove)
        {
            if (transform.position == destinationTransforms[destinationIndex].position)
            {
                if (destinationIndex == destinationTransforms.Count - 1)
                {
                    destinationIndex = 0;
                    shouldMove = false;
                    OnStopLocation();
                }
                else
                {
                    firstTransform = destinationTransforms[destinationIndex].transform;
                    startTime = Time.time;
                    destinationIndex++;
                }
            }
            else
            {
                Vector3 center = (firstTransform.position + destinationTransforms[destinationIndex].position) * 0.5f;

                center -= new Vector3(0, 1, 0);

                Vector3 firstPosCenter = firstTransform.position - center;
                Vector3 lastPosCenter = destinationTransforms[destinationIndex].position - center;

                float fracComplete = (Time.time - startTime) / journeyTime;

                transform.position = Vector3.Slerp(firstPosCenter, lastPosCenter, fracComplete);
                transform.position += center;
            }
        }
    }

    private void OnStopLocation()
    {
        GameObject playerLocation = playgroundController.locations[playerObjectController.playerLocation];
        LocationController locationController = playerLocation.GetComponent<LocationController>();
        LocationController.LocationType locationType = locationController.locationType;
        //Debug.Log($"Stopped on: {locationController.locationName}. Location type is: {locationController.locationType}.");

        if (locationType == LocationController.LocationType.RegularFactory || locationType == LocationController.LocationType.BigFactory || locationType == LocationController.LocationType.GoldenFactory)
        {
            FactoryController factoryController = playerLocation.GetComponent<FactoryController>();
            NetworkConnectionToClient target = playerObjectController.Manager.gamePlayers[playerObjectController.gameManager.turnIndex].connectionToClient;

            float buyPrice = playerObjectController.gameManager.factoryPricesPerLevel[factoryController.factoryLevel] * factoryController.priceMultiplier;

            if (factoryController.ownerPlayer) // Factory has owner
            {
                if (factoryController.ownerPlayer == playerObjectController) // Factory owned by local player
                {
                    if (factoryController.factoryLevel < factoryController.maxFactoryLevel) // Factory level smaller than it's max level
                    {
                        float upgradePrice = playerObjectController.gameManager.factoryPricesPerLevel[factoryController.factoryLevel] * factoryController.priceMultiplier;
                        if (playerObjectController.playerMoney >= upgradePrice) // Player has enough money to upgrade factory
                        {
                            Debug.Log("You already bought this factory, factory level is smaller than it's max level and you have enough money to upgrade it");
                            RpcSetFactoryUpgradePanelData(target, locationController.locationName, upgradePrice);
                        }
                        else // Player has not enough money to upgrade factory
                        {
                            Debug.Log("You already bought this factory, factory level is smaller than it's max level but you don't have enough money to upgrade it");
                            playerObjectController.gameManager.turnIndex++;
                        }
                    }
                    else
                    {
                        Debug.Log("You already bought this factory but factory level is at it's max level. You can't upgrade this factory.");
                        playerObjectController.gameManager.turnIndex++;
                    }
                }
                else // Factory owned by another player
                {
                    if (playerObjectController.playerMoney >= factoryController.rentRate)
                    {
                        playerObjectController.CmdUpdatePlayerMoney(playerObjectController.playerMoney - factoryController.rentRate);
                        factoryController.ownerPlayer.CmdUpdatePlayerMoney(factoryController.ownerPlayer.playerMoney + factoryController.rentRate);
                        if (factoryController.factoryLevel < 3)
                        {
                            buyPrice = playerObjectController.gameManager.factoryPricesPerLevel[factoryController.factoryLevel] * factoryController.priceMultiplier;
                            if (playerObjectController.playerMoney >= buyPrice)
                            {
                                RpcSetFactoryBuyPanelData(target, locationController.locationName, buyPrice, true, true);
                                Debug.Log($"{factoryController.ownerPlayer.playerName} own this factory. It's level is lower than 3. You have enough money to pay rent. You have enough money to buy this factory from him.");
                            }
                            else
                            {
                                playerObjectController.gameManager.CmdUpdateTurnIndex();
                                Debug.Log($"{factoryController.ownerPlayer.playerName} own this factory. It's level is lower than 3. You have enough money to pay rent. You don't have enough money to buy this factory from him.");
                            }
                        }
                        else
                        {
                            buyPrice = playerObjectController.gameManager.factoryPricesPerLevel[factoryController.factoryLevel] * factoryController.priceMultiplier;
                            if (playerObjectController.playerMoney >= buyPrice)
                            {
                                playerObjectController.gameManager.CmdUpdateTurnIndex();
                                Debug.Log($"{factoryController.ownerPlayer.playerName} own this factory. It's level is higher than 2. You have enough money to pay rent. You have enough money to buy this factory from him. You can't buy" +
                                $"this factory because of it's level.");
                            }
                            else
                            {
                                playerObjectController.gameManager.CmdUpdateTurnIndex();
                                Debug.Log($"{factoryController.ownerPlayer.playerName} own this factory. It's level is higher than 2. You have enough money to pay rent. You don't have enough money to buy this factory from him. You can't buy" +
                                $"this factory because of it's level.");
                            }
                        }
                    }
                    else
                    {
                        playerObjectController.CmdUpdatePlayerMoney(playerObjectController.playerMoney - factoryController.rentRate);
                        factoryController.ownerPlayer.CmdUpdatePlayerMoney(factoryController.ownerPlayer.playerMoney + factoryController.rentRate);
                        // TO DO Can't pay rent
                    }
                }
            }
            else // Factory has no owner
            {
                if (playerObjectController.playerMoney >= buyPrice)
                {
                    Debug.Log("Factory has no owner and you have enough money to buy it.");
                    RpcSetFactoryBuyPanelData(target, locationController.locationName, buyPrice, true, true);
                }
                else
                {
                    Debug.Log("Factory has no owner but you don't have enough money to buy it.");
                    playerObjectController.gameManager.CmdUpdateTurnIndex();
                }

            }

        }
        else if(locationType == LocationController.LocationType.Resource)
        {
            float buyPrice = playerObjectController.gameManager.resourceBuyPrice;
            ResourceController resourceController = playerLocation.GetComponent<ResourceController>();
            NetworkConnectionToClient target = playerObjectController.Manager.gamePlayers[playerObjectController.gameManager.turnIndex].connectionToClient;

            if (resourceController.ownerPlayer) // Resource has owner
            {
                if(resourceController.ownerPlayer == playerObjectController) // Resource owned by local player
                {
                    playerObjectController.gameManager.CmdUpdateTurnIndex();
                }
                else // Resource owned by another player
                {
                    playerObjectController.CmdUpdatePlayerMoney(playerObjectController.playerMoney - resourceController.rentRate);
                    resourceController.ownerPlayer.CmdUpdatePlayerMoney(resourceController.ownerPlayer.playerMoney + resourceController.rentRate);
                    playerObjectController.gameManager.CmdUpdateTurnIndex();
                }
            }
            else // Resource has no owner
            {
                if(playerObjectController.playerMoney >= buyPrice)
                {
                    RpcSetResourceBuyPanelData(target, locationController.locationName, buyPrice);
                }
                else
                {
                    playerObjectController.gameManager.CmdUpdateTurnIndex();
                }
                
            }
        }
        else
        {
            playerObjectController.gameManager.turnIndex++;
        }
    }

    [TargetRpc]
    private void Test(NetworkConnectionToClient target)
    {
        Debug.Log("Test");
    }


    [TargetRpc]
    private void RpcSetFactoryBuyPanelData(NetworkConnectionToClient target, string locationName, float buyPrice, bool canBuy, bool canCancel)
    {
        GameObject playerLocation = playgroundController.locations[playerObjectController.playerLocation];
        FactoryController factoryController = playerLocation.GetComponent<FactoryController>();
        UIManager uiManager = playerObjectController.gameManager.uiManager;
        GameObject factoryBuyPanel = Instantiate(uiManager.factoryBuyPanelPrefab, uiManager.mainCanvas.transform);
        FactoryBuyPanelData factoryBuyPanelData = factoryBuyPanel.GetComponent<FactoryBuyPanelData>();
        factoryBuyPanelData.locationNameText.text = locationName;

        factoryBuyPanelData.factoryLevelText.text = factoryController.factoryLevel.ToString();
        if (factoryController.factoryLevel == 0)
        {
            factoryBuyPanelData.rentRateText.text = $"Rent rate: {factoryController.CalculateRentRate(factoryController.factoryLevel)} => {factoryController.CalculateRentRate(factoryController.factoryLevel + 1)}";
        }
        else
        {
            factoryBuyPanelData.rentRateText.text = $"Rent rate: {factoryController.CalculateRentRate(factoryController.factoryLevel)}";
        }
        if (factoryController.ownerPlayer)
        {
            factoryBuyPanelData.ownerNameText.text = factoryController.ownerPlayer.playerName;
        }
        else
        {
            factoryBuyPanelData.ownerNameText.text = "No owner";
        }

        factoryBuyPanelData.buyButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Buy for: " + buyPrice.ToString();

        if (canBuy)
        {
            factoryBuyPanelData.buyButton.onClick.AddListener(() =>
            {
                playerObjectController.CmdUpdatePlayerMoney(playerObjectController.playerMoney - buyPrice);
                if (factoryController.ownerPlayer)
                {
                    factoryController.ownerPlayer.CmdUpdatePlayerMoney(factoryController.ownerPlayer.playerMoney + buyPrice);
                }
                playgroundController.CmdBuyFactory(playerObjectController.playerLocation, playerObjectController);
                playerObjectController.gameManager.CmdUpdateTurnIndex();
                Destroy(factoryBuyPanel);
            });
        }
        else
        {
            factoryBuyPanelData.buyButton.interactable = false;
        }

        if (canCancel)
        {
            factoryBuyPanelData.cancelButton.onClick.AddListener(() =>
            {
                playerObjectController.gameManager.CmdUpdateTurnIndex();
                Destroy(factoryBuyPanel);
            });
        }
        else
        {
            factoryBuyPanelData.cancelButton.gameObject.SetActive(false);
        }

    }

    [TargetRpc]
    private void RpcSetFactoryUpgradePanelData(NetworkConnectionToClient target, string locationName, float upgradePrice)
    {
        GameObject playerLocation = playgroundController.locations[playerObjectController.playerLocation];
        FactoryController factoryController = playerLocation.GetComponent<FactoryController>();
        UIManager uiManager = playerObjectController.gameManager.uiManager;
        GameObject factoryUpgradePanel = Instantiate(uiManager.factoryUpgradePanelPrefab, uiManager.mainCanvas.transform);
        FactoryUpgradePanelData factoryUpgradePanelData = factoryUpgradePanel.GetComponent<FactoryUpgradePanelData>();
        factoryUpgradePanelData.locationNameText.text = locationName;
        factoryUpgradePanelData.rentRateText.text = $"{factoryController.CalculateRentRate(factoryController.factoryLevel)} => {factoryController.CalculateRentRate(factoryController.factoryLevel + 1)}";
        factoryUpgradePanelData.factoryLevelText.text = $"{factoryController.factoryLevel} => {factoryController.factoryLevel + 1}";
        factoryUpgradePanelData.upgradeButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Upgrade for: " + upgradePrice.ToString();

        if (playerObjectController.playerMoney < upgradePrice)
        {
            factoryUpgradePanelData.upgradeButton.interactable = false;
        }
        else
        {
            factoryUpgradePanelData.upgradeButton.interactable = true;
            factoryUpgradePanelData.upgradeButton.onClick.AddListener(() =>
            {
                playerObjectController.CmdUpdatePlayerMoney(playerObjectController.playerMoney - upgradePrice);
                playgroundController.CmdUpgradeFactory(playerObjectController.playerLocation);
                playerObjectController.gameManager.CmdUpdateTurnIndex();
                Destroy(factoryUpgradePanel);
            });
        }

        factoryUpgradePanelData.cancelButton.onClick.AddListener(() =>
        {
            playerObjectController.gameManager.CmdUpdateTurnIndex();
            Destroy(factoryUpgradePanelData);
        });
    }
    [TargetRpc]
    private void RpcSetResourceBuyPanelData(NetworkConnectionToClient target, string locationName, float buyPrice)
    {
        GameObject playerLocation = playgroundController.locations[playerObjectController.playerLocation];
        ResourceController resourceController = playerLocation.GetComponent<ResourceController>();
        UIManager uiManager = playerObjectController.gameManager.uiManager;
        GameObject resourceBuyPanel = Instantiate(uiManager.resourceBuyPanelPrefab, uiManager.mainCanvas.transform);
        ResourceBuyPanelData resourceBuyPanelData = resourceBuyPanel.GetComponent<ResourceBuyPanelData>();

        resourceBuyPanelData.locationNameText.text = locationName;
        resourceBuyPanelData.rentRateText.text = $"Rent rate: 0 => {resourceController.CalculateRentRate()}";
        resourceBuyPanelData.ownerNameText.text = "No owner";
        resourceBuyPanelData.buyButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Buy for: " + buyPrice.ToString();
        resourceBuyPanelData.buyButton.onClick.AddListener(() =>
        {
            playerObjectController.CmdUpdatePlayerMoney(playerObjectController.playerMoney - buyPrice);
            playgroundController.CmdBuyResource(playerObjectController.playerLocation, playerObjectController);
            playerObjectController.gameManager.CmdUpdateTurnIndex();
            Destroy(resourceBuyPanel);
        });
        resourceBuyPanelData.cancelButton.onClick.AddListener(() =>
        {
            playerObjectController.gameManager.CmdUpdateTurnIndex();
            Destroy(resourceBuyPanelData);
        });
    }

    public void MovePlayer(int locationIndex)
    {
        if (!playgroundController)
        {
            playgroundController = playerObjectController.playgroundController;
        }
        destinationTransforms = new List<Transform>();
        firstTransform = playgroundController.locations[playerObjectController.playerLocation].transform;
        for (int i = playerObjectController.playerLocation; i < playerObjectController.playerLocation + locationIndex; i++)
        {
            if (i >= 39)
            {
                destinationTransforms.Add(playgroundController.locations[(i + 1) - 40].transform);
            }
            else
            {
                destinationTransforms.Add(playgroundController.locations[i + 1].transform);
            }
        }
        if (playerObjectController.playerLocation + locationIndex >= playgroundController.locations.Count)
        {
            playerObjectController.playerLocation = (playerObjectController.playerLocation + locationIndex) - playgroundController.locations.Count;
        }
        else
        {
            playerObjectController.playerLocation = playerObjectController.playerLocation + locationIndex;
        }

        shouldMove = true;
        startTime = Time.time;
    }

    public void SetStartPosition()
    {
        if (!playgroundController)
        {
            playgroundController = playerObjectController.playgroundController;
        }
        transform.position = playgroundController.locations[0].transform.position;
        playerObjectController.playerLocation = 0;
    }

    public void PlayerCosmeticsSetup()
    {
        playerMesh.material = playerColors[GetComponent<PlayerObjectController>().playerColor];
    }
}
