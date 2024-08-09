using Mirror;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class PlayerMoveController : NetworkBehaviour
{
    public GameObject playerModel;

    // Cosmetics
    public MeshRenderer playerMesh;

    [HideInInspector] public bool shouldMove;
    [HideInInspector] public int destinationIndex;
    [HideInInspector] public List<Transform> destinationTransforms;
    [HideInInspector] public Transform firstTransform;
    [HideInInspector] public float startTime;
    [HideInInspector] public bool isEven;
    public float journeyTime = .1f;

    private PlayerObjectController playerObjectController;
    private PlaygroundController playgroundController;
    [HideInInspector] public Camera mainCamera;

    public bool didCosmetic;



    // Start is called before the first frame update
    void Start()
    {
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
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
                if (destinationTransforms[destinationIndex] == playgroundController.locations[0].transform)
                {
                    playerObjectController.CmdUpdatePlayerMoney(playerObjectController.playerMoney + playerObjectController.gameManager.startingPointIncome);
                }
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

            if (factoryController.ownerPlayer) // Factory has owner
            {
                if (factoryController.ownerPlayer == playerObjectController) // Factory owned by local player
                {
                    if (factoryController.factoryLevel < factoryController.maxFactoryLevel) // Factory level smaller than it's max level
                    {
                        float upgradePrice = factoryController.CalculateUpgradePrice();
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
                    float rentRate = locationController.GetLocationRentRate();

                    if (playerObjectController.playerMoney >= rentRate)
                    {
                        playerObjectController.CmdUpdatePlayerMoney(playerObjectController.playerMoney - rentRate);
                        factoryController.ownerPlayer.CmdUpdatePlayerMoney(factoryController.ownerPlayer.playerMoney + rentRate);
                        if (factoryController.factoryLevel < 3)
                        {
                            float buyPrice = factoryController.CalculateSellToAnotherPrice(factoryController.factoryLevel);
                            if (playerObjectController.playerMoney >= buyPrice)
                            {
                                RpcSetFactoryBuyPanelData(target, locationController.locationName, buyPrice);
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
                            float buyPrice = factoryController.CalculateSellToAnotherPrice(factoryController.factoryLevel);
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
                        RpcSetSellOwnedLocationsPanelData(target, rentRate);
                        // TO DO Can't pay rent
                    }
                }
            }
            else // Factory has no owner
            {
                float buyPrice = factoryController.CalculateBuyFromBankPrice();
                if (playerObjectController.playerMoney >= buyPrice)
                {
                    Debug.Log("Factory has no owner and you have enough money to buy it.");
                    RpcSetFactoryBuyPanelData(target, locationController.locationName, buyPrice);
                }
                else
                {
                    Debug.Log("Factory has no owner but you don't have enough money to buy it.");
                    playerObjectController.gameManager.CmdUpdateTurnIndex();
                }

            }

        }
        else if (locationType == LocationController.LocationType.Resource)
        {
            float buyPrice = locationController.GetLocationBuyFromBankPrice();
            ResourceController resourceController = playerLocation.GetComponent<ResourceController>();
            NetworkConnectionToClient target = playerObjectController.Manager.gamePlayers[playerObjectController.gameManager.turnIndex].connectionToClient;

            if (resourceController.ownerPlayer) // Resource has owner
            {
                float rentRate = locationController.GetLocationRentRate();
                if (resourceController.ownerPlayer == playerObjectController) // Resource owned by local player
                {
                    playerObjectController.gameManager.CmdUpdateTurnIndex();
                }
                else // Resource owned by another player
                {
                    if (playerObjectController.playerMoney >= rentRate) // Player has enough money to pay rent
                    {
                        playerObjectController.CmdUpdatePlayerMoney(playerObjectController.playerMoney - rentRate);
                        resourceController.ownerPlayer.CmdUpdatePlayerMoney(resourceController.ownerPlayer.playerMoney + rentRate);
                        playerObjectController.gameManager.CmdUpdateTurnIndex();
                    }
                    else // Player doesn't have enough money to pay rent
                    {
                        RpcSetSellOwnedLocationsPanelData(target, rentRate);
                    }
                }
            }
            else // Resource has no owner
            {
                if (playerObjectController.playerMoney >= buyPrice) // Player has enough money to buy resource
                {
                    RpcSetResourceBuyPanelData(target, locationController.locationName, buyPrice);
                }
                else // Player doesn't have enough money to buy resource
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
    private void RpcSetFactoryBuyPanelData(NetworkConnectionToClient target, string locationName, float buyPrice)
    {
        // Get references
        GameObject playerLocation = playgroundController.locations[playerObjectController.playerLocation];
        FactoryController factoryController = playerLocation.GetComponent<FactoryController>();
        UIManager uiManager = playerObjectController.gameManager.uiManager;
        GameObject factoryBuyPanel = Instantiate(uiManager.factoryBuyPanelPrefab, uiManager.mainCanvas.transform);
        FactoryBuyPanelData factoryBuyPanelData = factoryBuyPanel.GetComponent<FactoryBuyPanelData>();

        // Set panel data
        if (factoryController.locationController.locationType == LocationController.LocationType.GoldenFactory)
        {
            factoryBuyPanelData.windowFrame.sprite = uiManager.goldenFrame;
        }
        else
        {
            factoryBuyPanelData.windowFrame.sprite = uiManager.normalFrame;
        }
        factoryBuyPanelData.locationNameText.text = locationName;

        factoryBuyPanelData.factoryLevelText.text = factoryController.factoryLevel.ToString();
        if (factoryController.locationController.locationType == LocationController.LocationType.GoldenFactory)
        {
            factoryBuyPanelData.previousButton.gameObject.SetActive(true);
            factoryBuyPanelData.nextButton.gameObject.SetActive(true);
            factoryBuyPanelData.productionTypeText.text = factoryController.locationController.productionType.ToString();
            for (int i = 0; i < factoryBuyPanelData.productionType.Length; i++)
            {
                if (factoryBuyPanelData.productionType[i] == factoryController.locationController.productionType.ToString())
                {
                    factoryBuyPanelData.productionTypeIndex = i;
                    break;
                }
            }
            factoryBuyPanelData.previousButton.onClick.AddListener(() =>
            {
                if (factoryBuyPanelData.productionTypeIndex == 0)
                {
                    factoryBuyPanelData.productionTypeIndex = 4;
                }
                else
                {
                    factoryBuyPanelData.productionTypeIndex--;
                }
                factoryBuyPanelData.productionTypeText.text = factoryBuyPanelData.productionType[factoryBuyPanelData.productionTypeIndex];
                factoryBuyPanelData.productivityText.text = $"%{factoryController.locationController.productivity + factoryController.locationController.CheckResource(factoryBuyPanelData.productionTypeText.text, playerObjectController) * 100}";
            });
            factoryBuyPanelData.nextButton.onClick.AddListener(() =>
            {
                if (factoryBuyPanelData.productionTypeIndex == 4)
                {
                    factoryBuyPanelData.productionTypeIndex = 0;
                }
                else
                {
                    factoryBuyPanelData.productionTypeIndex++;
                }
                factoryBuyPanelData.productionTypeText.text = factoryBuyPanelData.productionType[factoryBuyPanelData.productionTypeIndex];
                factoryBuyPanelData.productivityText.text = $"%{factoryController.locationController.productivity + factoryController.locationController.CheckResource(factoryBuyPanelData.productionTypeText.text, playerObjectController) * 100}";
            });
            factoryBuyPanelData.productionTypeText.text = factoryBuyPanelData.productionType[factoryBuyPanelData.productionTypeIndex];
            factoryBuyPanelData.productivityText.text = $"%{factoryController.locationController.productivity + factoryController.locationController.CheckResource(factoryBuyPanelData.productionTypeText.text, playerObjectController) * 100}";
        }
        else
        {
            factoryBuyPanelData.previousButton.gameObject.SetActive(false);
            factoryBuyPanelData.nextButton.gameObject.SetActive(false);
            factoryBuyPanelData.productionTypeText.text = factoryController.locationController.productionType.ToString();
            factoryBuyPanelData.productivityText.text = factoryController.locationController.productivity.ToString();
        }
        
        factoryBuyPanelData.buyPriceText.text = "$" + string.Format(CultureInfo.InvariantCulture, "{0:N0}", buyPrice);
        if (factoryController.factoryLevel == 0)
        {
            factoryBuyPanelData.rentRateText.text = "$" + string.Format(CultureInfo.InvariantCulture, "{0:N0}", factoryController.CalculateRentRate(1));
        }
        else
        {
            factoryBuyPanelData.rentRateText.text = "$" + string.Format(CultureInfo.InvariantCulture, "{0:N0}", factoryController.CalculateRentRate(factoryController.factoryLevel));
        }
        if (factoryController.ownerPlayer)
        {
            factoryBuyPanelData.ownerNameText.text = factoryController.ownerPlayer.playerName;
        }
        else
        {
            factoryBuyPanelData.ownerNameText.text = "No owner";
        }


        // Add listener to buy button
        factoryBuyPanelData.buyButton.onClick.AddListener(() =>
        {
            playerObjectController.CmdUpdatePlayerMoney(playerObjectController.playerMoney - buyPrice);
            if (factoryController.ownerPlayer)
            {
                factoryController.ownerPlayer.CmdUpdatePlayerMoney(factoryController.ownerPlayer.playerMoney + buyPrice);
            }
            playgroundController.CmdBuyFactory(playerObjectController.playerLocation, playerObjectController);

            // Golden factory functions
            if(factoryController.locationController.locationType == LocationController.LocationType.GoldenFactory)
            {
                playgroundController.CmdSetProductionType(playerObjectController.playerLocation, factoryBuyPanelData.productionTypeText.text);
                playgroundController.CmdSetProductivity(playerObjectController.playerLocation, playerObjectController.gameManager.turnIndex);
                //factoryController.locationController.SetProductionType(factoryBuyPanelData.productionTypeText.text);
                //factoryController.locationController.SetProductivity(playerObjectController);
            }
            playerObjectController.gameManager.CmdUpdateTurnIndex();
            Destroy(factoryBuyPanel);
        });

        // Add listener to cancel button
        factoryBuyPanelData.cancelButton.onClick.AddListener(() =>
        {
            playerObjectController.gameManager.CmdUpdateTurnIndex();
            Destroy(factoryBuyPanel);
        });

    }
    [TargetRpc]
    private void RpcSetFactoryUpgradePanelData(NetworkConnectionToClient target, string locationName, float upgradePrice)
    {
        // Get references
        GameObject playerLocation = playgroundController.locations[playerObjectController.playerLocation];
        FactoryController factoryController = playerLocation.GetComponent<FactoryController>();
        UIManager uiManager = playerObjectController.gameManager.uiManager;
        GameObject factoryUpgradePanel = Instantiate(uiManager.factoryUpgradePanelPrefab, uiManager.mainCanvas.transform);
        FactoryUpgradePanelData factoryUpgradePanelData = factoryUpgradePanel.GetComponent<FactoryUpgradePanelData>();

        // Set panel data
        if (factoryController.locationController.locationType == LocationController.LocationType.GoldenFactory)
        {
            factoryUpgradePanelData.windowFrame.sprite = uiManager.goldenFrame;
        }
        else
        {
            factoryUpgradePanelData.windowFrame.sprite = uiManager.normalFrame;
        }
        factoryUpgradePanelData.locationNameText.text = locationName;
        factoryUpgradePanelData.productionTypeText.text = factoryController.locationController.productionType.ToString();
        factoryUpgradePanelData.factoryLevelText.text = factoryController.factoryLevel.ToString();
        factoryUpgradePanelData.productivityText.text = factoryController.locationController.productivity.ToString();
        factoryUpgradePanelData.upgradePriceHeader.text = $"Upgrade Price For Level {factoryController.factoryLevel + 1}";
        factoryUpgradePanelData.upgradePriceText.text = "$" + string.Format(CultureInfo.InvariantCulture, "{0:N0}", upgradePrice);
        factoryUpgradePanelData.currentRentRateText.text = "$" + string.Format(CultureInfo.InvariantCulture, "{0:N0}", factoryController.CalculateRentRate(factoryController.factoryLevel));
        factoryUpgradePanelData.nextRentRateText.text = "$" + string.Format(CultureInfo.InvariantCulture, "{0:N0}", factoryController.CalculateRentRate(factoryController.factoryLevel + 1));

        // Add listener to upgrade button
        factoryUpgradePanelData.upgradeButton.onClick.AddListener(() =>
        {
            playerObjectController.CmdUpdatePlayerMoney(playerObjectController.playerMoney - upgradePrice);
            playgroundController.CmdUpgradeFactory(playerObjectController.playerLocation);
            playerObjectController.gameManager.CmdUpdateTurnIndex();
            Destroy(factoryUpgradePanel);
        });

        // Add listener to cancel button
        factoryUpgradePanelData.cancelButton.onClick.AddListener(() =>
        {
            playerObjectController.gameManager.CmdUpdateTurnIndex();
            Destroy(factoryUpgradePanel);
        });
    }
    [TargetRpc]
    private void RpcSetResourceBuyPanelData(NetworkConnectionToClient target, string locationName, float buyPrice)
    {
        // Get references
        GameObject playerLocation = playgroundController.locations[playerObjectController.playerLocation];
        ResourceController resourceController = playerLocation.GetComponent<ResourceController>();
        UIManager uiManager = playerObjectController.gameManager.uiManager;
        GameObject resourceBuyPanel = Instantiate(uiManager.resourceBuyPanelPrefab, uiManager.mainCanvas.transform);
        ResourceBuyPanelData resourceBuyPanelData = resourceBuyPanel.GetComponent<ResourceBuyPanelData>();

        // Set panel data
        resourceBuyPanelData.windowFrame.sprite = uiManager.normalFrame;
        resourceBuyPanelData.locationNameText.text = locationName;
        resourceBuyPanelData.productionTypeText.text = resourceController.locationController.productionType.ToString();
        resourceBuyPanelData.buyPriceText.text = "$" + string.Format(CultureInfo.InvariantCulture, "{0:N0}", buyPrice);
        resourceBuyPanelData.rentRateText.text = "$" + string.Format(CultureInfo.InvariantCulture, "{0:N0}", resourceController.CalculateRentRate());

        //Add listener to buy button
        resourceBuyPanelData.buyButton.onClick.AddListener(() =>
        {
            playerObjectController.CmdUpdatePlayerMoney(playerObjectController.playerMoney - buyPrice);
            playgroundController.CmdBuyResource(playerObjectController.playerLocation, playerObjectController);
            playerObjectController.gameManager.CmdUpdateTurnIndex();
            Destroy(resourceBuyPanel);
        });

        // Add listener to cancel button
        resourceBuyPanelData.cancelButton.onClick.AddListener(() =>
        {
            playerObjectController.gameManager.CmdUpdateTurnIndex();
            Destroy(resourceBuyPanel);
        });
    }
    [TargetRpc]
    public void RpcSetSellOwnedLocationsPanelData(NetworkConnectionToClient target, float rentRate)
    {
        playerObjectController.canSell = true;
        playerObjectController.locationsToBeSold = new();
        UIManager uiManager = playerObjectController.gameManager.uiManager;
        foreach (LocationController locationController in playerObjectController.ownedLocations)
        {
            GameObject sellLocationToggle = Instantiate(playgroundController.sellLocationTogglePrefab, locationController.transform);
            sellLocationToggle.transform.localPosition = new Vector3(1, .1f, 0);
            locationController.tag = "Saleable Location";
            locationController.sellLocationToggle = sellLocationToggle;
        }
        GameObject sellLocationsPanel = Instantiate(uiManager.sellLocationsPanelPrefab, uiManager.mainCanvas.transform);
        SellLocationsPanelData sellLocationsPanelData = sellLocationsPanel.GetComponent<SellLocationsPanelData>();
        playerObjectController.sellLocationsPanelData = sellLocationsPanelData;
        sellLocationsPanelData.theMoneyYouNeedText.text = $"You need: {rentRate}";
        sellLocationsPanelData.yourMoneyAfterSalesText.text = $"Your money: {playerObjectController.playerMoney}";
        sellLocationsPanelData.confirmButton.interactable = false;
    }

    public void SetSellLocationsPanelButtonData()
    {
        LocationController _locationController = playgroundController.locations[playerObjectController.playerLocation].GetComponent<LocationController>(); // The location stopped on.
        float rentRate = _locationController.GetLocationRentRate();
        float value = 0;
        foreach (LocationController locationController in playerObjectController.locationsToBeSold)
        {
            value += locationController.GetLocationRentRate();
        }
        playerObjectController.sellLocationsPanelData.yourMoneyAfterSalesText.text = $"Your money: {playerObjectController.playerMoney + value}";
        if (playerObjectController.playerMoney + value >= rentRate)
        {
            playerObjectController.sellLocationsPanelData.confirmButton.onClick.RemoveAllListeners();
            playerObjectController.sellLocationsPanelData.confirmButton.onClick.AddListener(() =>
            {
                foreach (LocationController _locationController in playerObjectController.ownedLocations)
                {
                    Destroy(_locationController.sellLocationToggle);
                    _locationController.sellLocationToggle = null;
                    _locationController.tag = "Untagged";
                }
                playerObjectController.CmdUpdatePlayerMoney(playerObjectController.playerMoney + value - rentRate);
                _locationController.ownerPlayer.CmdUpdatePlayerMoney(_locationController.ownerPlayer.playerMoney + rentRate);
                foreach (LocationController locationController in playerObjectController.locationsToBeSold)
                {
                    playgroundController.CmdSellLocationToTheBank(locationController.locationIndex, playerObjectController);
                }
                playerObjectController.gameManager.CmdUpdateTurnIndex();
                playerObjectController.canSell = false;

                Destroy(playerObjectController.sellLocationsPanelData.gameObject);
            });
            playerObjectController.sellLocationsPanelData.confirmButton.interactable = true;
        }
        else
        {
            playerObjectController.sellLocationsPanelData.confirmButton.interactable = false;
        }
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
        playerMesh.material.color = playerObjectController.playerColor;
    }
}
