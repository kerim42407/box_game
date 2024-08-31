using Mirror;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private PlaygroundController playgroundController;
    [HideInInspector] public GameManager gameManager;

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
                if (destinationTransforms[destinationIndex] == playgroundController.locations[0].transform)
                {
                    playerObjectController.CmdUpdatePlayerMoney(playerObjectController.playerMoney + gameManager.startingPointIncome);
                }
                if (destinationIndex == destinationTransforms.Count - 1)
                {
                    playgroundController.CmdDeactivateEmissionOnLocation(playerObjectController.playerLocation);
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
        NetworkConnectionToClient target = gameManager.Manager.gamePlayers[gameManager.turnIndex].connectionToClient;
        LocationController locationController = playgroundController.locations[playerObjectController.playerLocation].GetComponent<LocationController>();
        LocationType locationType = locationController.locationType;

        if(locationType == LocationType.RegularFactory || locationType == LocationType.BigFactory || locationType == LocationType.GoldenFactory)
        {
            FactoryController factoryController = playgroundController.locations[playerObjectController.playerLocation].GetComponent<FactoryController>();

            // Factory has owner
            if (factoryController.s_OwnerPlayer)
            {
                PlayerObjectController ownerPlayer = factoryController.s_OwnerPlayer;
                int factoryLevel = factoryController.s_FactoryLevel;
                int maxFactoryLevel = factoryController.maxFactoryLevel;

                // Factory owned by local player
                if(ownerPlayer == playerObjectController)
                {
                    // Factory level is smaller than its max level
                    if(factoryLevel < maxFactoryLevel)
                    {
                        float upgradePrice = gameManager.CalculateFactoryUpgradePrice(factoryController);

                        // Player has enough money to upgrade factory
                        if(playerObjectController.playerMoney >= upgradePrice)
                        {
                            RpcSetFactoryUpgradePanelData(target, locationController.locationName, upgradePrice);
                            Debug.Log("You have enough money to upgrade the factory");
                        }

                        // Player does not have enough money to upgrade factory
                        else
                        {
                            Debug.Log("You do not have enough money to upgrade the factory");
                            gameManager.CmdUpdateTurnIndex();
                        }
                    }

                    // Factory level is at its max level
                    else
                    {
                        gameManager.CmdUpdateTurnIndex();
                        Debug.Log("Factory level is at its max level");
                    }
                }

                // Factory owned by another player
                else
                {
                    float rentRate = factoryController.s_RentRate;

                    // Player have enough money to pay rent
                    if (playerObjectController.playerMoney >= rentRate)
                    {
                        playerObjectController.CmdUpdatePlayerMoney(playerObjectController.playerMoney - rentRate);
                        factoryController.s_OwnerPlayer.CmdUpdatePlayerMoney(factoryController.s_OwnerPlayer.playerMoney + rentRate);
                        float buyPrice = gameManager.CalculateFactorySellToAnotherPlayerPrice(factoryController);
                        float _playerMoney = playerObjectController.playerMoney - rentRate;

                        // Factory level is smaller than 3
                        if (factoryController.s_FactoryLevel < 3)
                        {

                            // Player have enough money to buy factory
                            if (_playerMoney >= buyPrice)
                            {
                                RpcSetFactoryBuyPanelData(target, locationController.locationName, buyPrice);
                                Debug.Log($"You have enough money to buy factory");
                            }

                            // Player does not have enough money to buy factory
                            else
                            {
                                gameManager.CmdUpdateTurnIndex();
                                Debug.Log("You do not have enough money to buy factory");
                            }
                        }

                        // Factory level is bigger than 2
                        else
                        {
                            gameManager.CmdUpdateTurnIndex();
                            Debug.Log("Factory level is bigger than 2. You can't buy this factory");
                        }
                    }

                    // Player does not have enough money to pay rent
                    else
                    {
                        float playerMoneyAfterSell = 0;
                        foreach (LocationController locationController1 in playerObjectController.s_PlayerOwnedLocations)
                        {
                            playerMoneyAfterSell += locationController1.GetCalculateSellToBankPrice();
                        }

                        // Player have enough money to pay rent after sales
                        if (playerObjectController.playerMoney + playerMoneyAfterSell >= rentRate)
                        {
                            RpcSetSellLocationsPanelData(target, rentRate);
                            Debug.Log("You have enough money to pay rent after sales");
                        }

                        // Player does not have enough money to pay rent after sales. Bankruptcy
                        else
                        {
                            foreach (LocationController locationController1 in playerObjectController.s_PlayerOwnedLocations)
                            {
                                playgroundController.CmdSellLocationToTheBank(locationController1.locationIndex, playerObjectController);
                            }
                            playgroundController.CmdBankruptcy(playerObjectController);
                            gameManager.CmdUpdateTurnIndex();
                            Debug.Log("You do not have enough money to pay rent after sales. Bankruptcy");
                        }
                    }
                }
            }

            // Factory has no owner
            else
            {
                float buyPrice = gameManager.CalculateFactoryBuyFromBankPrice(factoryController);

                // Player has enough money to buy factory
                if (playerObjectController.playerMoney >= buyPrice)
                {
                    RpcSetFactoryBuyPanelData(target, locationController.locationName, buyPrice);
                    Debug.Log("You have enough money to buy factory");
                }

                // Player does not have enough money to buy factory
                else
                {
                    gameManager.CmdUpdateTurnIndex();
                    Debug.Log("You do not have enough money to buy factory");
                }
            }
        }
        else if(locationType == LocationType.Resource)
        {
            ResourceController resourceController = playgroundController.locations[playerObjectController.playerLocation].GetComponent<ResourceController>();

            // Resource has owner
            if (resourceController.s_OwnerPlayer)
            {
                PlayerObjectController ownerPlayer = resourceController.s_OwnerPlayer;

                // Resource owned by local player
                if(ownerPlayer == playerObjectController)
                {
                    gameManager.CmdUpdateTurnIndex();
                }
                else
                {
                    float rentRate = resourceController.s_RentRate;

                    // Player has enough money to pay rent
                    if (playerObjectController.playerMoney >= rentRate)
                    {
                        playerObjectController.CmdUpdatePlayerMoney(playerObjectController.playerMoney - rentRate);
                        ownerPlayer.CmdUpdatePlayerMoney(ownerPlayer.playerMoney + rentRate);
                        gameManager.CmdUpdateTurnIndex();
                    }

                    // Player does not have enough money to pay rent
                    else
                    {
                        float playerMoneyAfterSell = 0;
                        foreach (LocationController locationController1 in playerObjectController.s_PlayerOwnedLocations)
                        {
                            playerMoneyAfterSell += locationController1.GetCalculateSellToBankPrice();
                        }

                        // Player have enough money to pay rent after sales
                        if (playerObjectController.playerMoney + playerMoneyAfterSell >= rentRate)
                        {
                            RpcSetSellLocationsPanelData(target, rentRate);
                            Debug.Log("You have enough money to pay rent after sales");
                        }

                        // Player does not have enough money to pay rent after sales. Bankruptcy
                        else
                        {
                            foreach (LocationController locationController1 in playerObjectController.s_PlayerOwnedLocations)
                            {
                                playgroundController.CmdSellLocationToTheBank(locationController1.locationIndex, playerObjectController);
                            }
                            playgroundController.CmdBankruptcy(playerObjectController);
                            gameManager.CmdUpdateTurnIndex();
                            Debug.Log("You do not have enough money to pay rent after sales. Bankruptcy");
                        }
                    }
                }
            }

            // Resource has no owner
            else
            {
                float buyPrice = gameManager.CalculateResourceBuyFromBankPrice();

                // Player has enough money to buy resource
                if (playerObjectController.playerMoney >= buyPrice)
                {
                    RpcSetResourceBuyPanelData(target, locationController.locationName, buyPrice);
                }

                // Player does not have enough money to buy resource
                else
                {
                    gameManager.CmdUpdateTurnIndex();
                }
            }
        }
        else if (locationType == LocationType.Card)
        {
            DeckController deckController = playgroundController.locations[playerObjectController.playerLocation].GetComponent<DeckController>();
            gameManager.CmdDrawCardForPlayer(playerObjectController, deckController);
        }
        else
        {
            gameManager.CmdUpdateTurnIndex();
        }
    }

    [TargetRpc]
    private void RpcSetFactoryBuyPanelData(NetworkConnectionToClient target, string locationName, float buyPrice)
    {
        // Get references
        GameObject playerLocation = playgroundController.locations[playerObjectController.playerLocation];
        FactoryController factoryController = playerLocation.GetComponent<FactoryController>();
        UIManager uiManager = gameManager.uiManager;
        GameObject factoryBuyPanel = Instantiate(uiManager.factoryBuyPanelPrefab, uiManager.mainCanvas.transform);
        FactoryBuyPanelData factoryBuyPanelData = factoryBuyPanel.GetComponent<FactoryBuyPanelData>();

        // Set window frame
        if (factoryController.locationType == LocationType.GoldenFactory)
        {
            factoryBuyPanelData.windowFrame.sprite = uiManager.goldenFrame;
        }
        else
        {
            factoryBuyPanelData.windowFrame.sprite = uiManager.normalFrame;
        }

        // Set location name text
        factoryBuyPanelData.locationNameText.text = locationName;

        // Set factory level text
        if (factoryController.s_FactoryLevel == 0)
        {
            factoryBuyPanelData.factoryLevelText.text = "1";
        }
        else
        {
            factoryBuyPanelData.factoryLevelText.text = factoryController.s_FactoryLevel.ToString();
        }

        #region Golden Factory Functions
        // Set golden factory functions
        if (factoryController.locationType == LocationType.GoldenFactory)
        {
            factoryBuyPanelData.previousButton.gameObject.SetActive(true);
            factoryBuyPanelData.nextButton.gameObject.SetActive(true);
            factoryBuyPanelData.productionTypeText.text = factoryController.s_ProductionType.ToString();
            for (int i = 0; i < factoryBuyPanelData.productionType.Length; i++)
            {
                if (factoryBuyPanelData.productionType[i] == factoryController.s_ProductionType.ToString())
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
                factoryBuyPanelData.productivityText.text = $"%{gameManager.CalculateProductivityByProductionType(playerObjectController, factoryBuyPanelData.productionTypeText.text)}";
                factoryBuyPanelData.rentRateText.text = "$" + string.Format(CultureInfo.InvariantCulture, "{0:N0}", gameManager.CalculateFactoryRentRate(factoryController.s_FactoryLevel,factoryController.factoryPriceCoef,gameManager.CalculateProductivityByProductionType(playerObjectController, factoryBuyPanelData.productionTypeText.text)));
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
                factoryBuyPanelData.productivityText.text = $"%{gameManager.CalculateProductivityByProductionType(playerObjectController, factoryBuyPanelData.productionTypeText.text)}";
                factoryBuyPanelData.rentRateText.text = "$" + string.Format(CultureInfo.InvariantCulture, "{0:N0}", gameManager.CalculateFactoryRentRate(factoryController.s_FactoryLevel, factoryController.factoryPriceCoef, gameManager.CalculateProductivityByProductionType(playerObjectController, factoryBuyPanelData.productionTypeText.text)));
            });
            factoryBuyPanelData.productionTypeText.text = factoryBuyPanelData.productionType[factoryBuyPanelData.productionTypeIndex];
            factoryBuyPanelData.productivityText.text = $"%{gameManager.CalculateProductivityByProductionType(playerObjectController, factoryBuyPanelData.productionTypeText.text)}";
            factoryBuyPanelData.rentRateText.text = "$" + string.Format(CultureInfo.InvariantCulture, "{0:N0}", gameManager.CalculateFactoryRentRate(factoryController.s_FactoryLevel, factoryController.factoryPriceCoef, gameManager.CalculateProductivityByProductionType(playerObjectController, factoryBuyPanelData.productionTypeText.text)));
        }

        // Other factory functions
        else
        {
            factoryBuyPanelData.previousButton.gameObject.SetActive(false);
            factoryBuyPanelData.nextButton.gameObject.SetActive(false);
            factoryBuyPanelData.productionTypeText.text = factoryController.s_ProductionType.ToString();
            factoryBuyPanelData.productivityText.text = $"%{factoryController.s_Productivity}";
            factoryBuyPanelData.rentRateText.text = "$" + string.Format(CultureInfo.InvariantCulture, "{0:N0}", gameManager.CalculateFactoryRentRate(factoryController.s_FactoryLevel,factoryController.factoryPriceCoef,factoryController.s_Productivity));
        }
        #endregion

        // Set buy price text
        factoryBuyPanelData.buyPriceText.text = "$" + string.Format(CultureInfo.InvariantCulture, "{0:N0}", buyPrice);

        // Set owner player text
        if (factoryController.s_OwnerPlayer)
        {
            factoryBuyPanelData.ownerNameText.text = factoryController.s_OwnerPlayer.playerName;
        }
        else
        {
            factoryBuyPanelData.ownerNameText.text = "No owner";
        }


        // Add listener to buy button
        factoryBuyPanelData.buyButton.onClick.AddListener(() =>
        {
            playerObjectController.CmdUpdatePlayerMoney(playerObjectController.playerMoney - buyPrice);
            if (factoryController.s_OwnerPlayer)
            {
                factoryController.s_OwnerPlayer.CmdUpdatePlayerMoney(factoryController.s_OwnerPlayer.playerMoney + buyPrice);
            }
            playgroundController.CmdBuyFactory(playerObjectController.playerLocation, playerObjectController, factoryBuyPanelData.productionType[factoryBuyPanelData.productionTypeIndex]);
            gameManager.CmdUpdateTurnIndex();
            Destroy(factoryBuyPanel);
        });

        // Add listener to cancel button
        factoryBuyPanelData.cancelButton.onClick.AddListener(() =>
        {
            gameManager.CmdUpdateTurnIndex();
            Destroy(factoryBuyPanel);
        });

    }
    [TargetRpc]
    private void RpcSetFactoryUpgradePanelData(NetworkConnectionToClient target, string locationName, float upgradePrice)
    {
        // Get references
        GameObject playerLocation = playgroundController.locations[playerObjectController.playerLocation];
        FactoryController factoryController = playerLocation.GetComponent<FactoryController>();
        UIManager uiManager = gameManager.uiManager;
        GameObject factoryUpgradePanel = Instantiate(uiManager.factoryUpgradePanelPrefab, uiManager.mainCanvas.transform);
        FactoryUpgradePanelData factoryUpgradePanelData = factoryUpgradePanel.GetComponent<FactoryUpgradePanelData>();

        // Set window frame
        if (factoryController.locationType == LocationType.GoldenFactory)
        {
            
            factoryUpgradePanelData.windowFrame.sprite = uiManager.goldenFrame;
        }
        else
        {
            factoryUpgradePanelData.windowFrame.sprite = uiManager.normalFrame;
        }

        // Set location name text
        factoryUpgradePanelData.locationNameText.text = locationName;

        // Set factory level text
        factoryUpgradePanelData.factoryLevelText.text = factoryController.s_FactoryLevel.ToString();

        // Set production type text
        factoryUpgradePanelData.productionTypeText.text = factoryController.s_ProductionType.ToString();

        // Set productivity text
        factoryUpgradePanelData.productivityText.text = $"%{factoryController.s_Productivity}";

        // Set upgrade price text
        factoryUpgradePanelData.upgradePriceHeader.text = $"Upgrade Price For Level {factoryController.s_FactoryLevel + 1}";
        factoryUpgradePanelData.upgradePriceText.text = "$" + string.Format(CultureInfo.InvariantCulture, "{0:N0}", upgradePrice);

        // Set rent rate text
        factoryUpgradePanelData.currentRentRateText.text = "$" + string.Format(CultureInfo.InvariantCulture, "{0:N0}", gameManager.CalculateFactoryRentRate(factoryController.s_FactoryLevel, factoryController.factoryPriceCoef, factoryController.s_Productivity));
        factoryUpgradePanelData.nextRentRateText.text = "$" + string.Format(CultureInfo.InvariantCulture, "{0:N0}", gameManager.CalculateFactoryRentRate(factoryController.s_FactoryLevel +1, factoryController.factoryPriceCoef, factoryController.s_Productivity));

        // Add listener to upgrade button
        factoryUpgradePanelData.upgradeButton.onClick.AddListener(() =>
        {
            playerObjectController.CmdUpdatePlayerMoney(playerObjectController.playerMoney - upgradePrice);
            playgroundController.CmdUpgradeFactory(playerObjectController.playerLocation);
            gameManager.CmdUpdateTurnIndex();
            Destroy(factoryUpgradePanel);
        });

        // Add listener to cancel button
        factoryUpgradePanelData.cancelButton.onClick.AddListener(() =>
        {
            gameManager.CmdUpdateTurnIndex();
            Destroy(factoryUpgradePanel);
        });
    }
    [TargetRpc]
    private void RpcSetResourceBuyPanelData(NetworkConnectionToClient target, string locationName, float buyPrice)
    {
        // Get references
        GameObject playerLocation = playgroundController.locations[playerObjectController.playerLocation];
        ResourceController resourceController = playerLocation.GetComponent<ResourceController>();
        UIManager uiManager = gameManager.uiManager;
        GameObject resourceBuyPanel = Instantiate(uiManager.resourceBuyPanelPrefab, uiManager.mainCanvas.transform);
        ResourceBuyPanelData resourceBuyPanelData = resourceBuyPanel.GetComponent<ResourceBuyPanelData>();

        // Set panel data
        resourceBuyPanelData.windowFrame.sprite = uiManager.normalFrame;
        resourceBuyPanelData.locationNameText.text = locationName;
        resourceBuyPanelData.productionTypeText.text = resourceController.s_ProductionType.ToString();
        resourceBuyPanelData.buyPriceText.text = "$" + string.Format(CultureInfo.InvariantCulture, "{0:N0}", buyPrice);
        resourceBuyPanelData.rentRateText.text = "$" + string.Format(CultureInfo.InvariantCulture, "{0:N0}", gameManager.resourceRentRate);

        //Add listener to buy button
        resourceBuyPanelData.buyButton.onClick.AddListener(() =>
        {
            playerObjectController.CmdUpdatePlayerMoney(playerObjectController.playerMoney - buyPrice);
            playgroundController.CmdBuyResource(playerObjectController.playerLocation, playerObjectController);
            gameManager.CmdUpdateTurnIndex();
            Destroy(resourceBuyPanel);
        });

        // Add listener to cancel button
        resourceBuyPanelData.cancelButton.onClick.AddListener(() =>
        {
            gameManager.CmdUpdateTurnIndex();
            Destroy(resourceBuyPanel);
        });
    }
    [TargetRpc]
    public void RpcSetSellLocationsPanelData(NetworkConnectionToClient target, float rentRate)
    {
        playerObjectController.canSell = true;
        playerObjectController.locationsToBeSold = new();
        UIManager uiManager = gameManager.uiManager;
        foreach (LocationController locationController in playerObjectController.s_PlayerOwnedLocations)
        {
            SellLocationInfoPanelData sellLocationInfoPanelData = Instantiate(playgroundController.sellLocationInfoPanelPrefab, gameManager.canvas.transform).GetComponent<SellLocationInfoPanelData>();
            locationController.sellLocationInfoPanelData = sellLocationInfoPanelData;
            sellLocationInfoPanelData.transform.position = Camera.main.WorldToScreenPoint(locationController.transform.position);
            sellLocationInfoPanelData.locationNameText.text = locationController.locationName;
            sellLocationInfoPanelData.productivityText.text = $"%{locationController.GetProductivity()}";
            sellLocationInfoPanelData.sellPriceText.text = "$" + string.Format(CultureInfo.InvariantCulture, "{0:N0}", locationController.GetCalculateSellToBankPrice());

            // Add listener to toggle button
            sellLocationInfoPanelData.toggleButton.onClick.AddListener(() =>
            {
                sellLocationInfoPanelData.toggleButtonState = !sellLocationInfoPanelData.toggleButtonState;
                if (sellLocationInfoPanelData.toggleButtonState)
                {
                    sellLocationInfoPanelData.icon.sprite = sellLocationInfoPanelData.checkedImage;
                    playerObjectController.locationsToBeSold.Add(locationController);
                }
                else
                {
                    sellLocationInfoPanelData.icon.sprite = sellLocationInfoPanelData.uncheckedImage;
                    playerObjectController.locationsToBeSold.Remove(locationController);
                }
                SetSellLocationsPanelButtonData();
            });
        }

        GameObject sellLocationsPanel = Instantiate(uiManager.sellLocationsPanelPrefab, uiManager.mainCanvas.transform);
        SellLocationsPanelData sellLocationsPanelData = sellLocationsPanel.GetComponent<SellLocationsPanelData>();
        playerObjectController.sellLocationsPanelData = sellLocationsPanelData;
        sellLocationsPanelData.rentRateText.text = $"Rent rate: {rentRate}";
        sellLocationsPanelData.playerMoneyText.text = $"Your money: {playerObjectController.playerMoney}";
        sellLocationsPanelData.sellButton.Interactable(false);
    }

    public void Test()
    {
        foreach (LocationController locationController1 in playerObjectController.s_PlayerOwnedLocations)
        {
            playgroundController.CmdSellLocationToTheBank(locationController1.locationIndex, playerObjectController);
        }
        playgroundController.CmdBankruptcy(playerObjectController);
    }

    public void SellTest(NetworkConnectionToClient target, float rentRate)
    {
        RpcSetSellLocationsPanelData(target, rentRate);
    }

    public void SetSellLocationsPanelButtonData()
    {
        LocationController _locationController = playgroundController.locations[playerObjectController.playerLocation].GetComponent<LocationController>(); // The location stopped on.
        float rentRate = _locationController.GetRentRate();
        float value = 0;
        foreach (LocationController locationController in playerObjectController.locationsToBeSold)
        {
            value += locationController.GetCalculateSellToBankPrice();
        }
        playerObjectController.sellLocationsPanelData.playerMoneyText.text = $"Your money: {playerObjectController.playerMoney + value}";
        if (playerObjectController.playerMoney + value >= rentRate)
        {
            playerObjectController.sellLocationsPanelData.sellButton.onClick.RemoveAllListeners();
            playerObjectController.sellLocationsPanelData.sellButton.onClick.AddListener(() =>
            {
                foreach (LocationController _locationController in playerObjectController.s_PlayerOwnedLocations)
                {
                    Destroy(_locationController.sellLocationInfoPanelData.gameObject);
                    _locationController.sellLocationInfoPanelData = null;
                }
                playerObjectController.CmdUpdatePlayerMoney(playerObjectController.playerMoney + value - rentRate);
                _locationController.s_OwnerPlayer.CmdUpdatePlayerMoney(_locationController.s_OwnerPlayer.playerMoney + rentRate);
                foreach (LocationController locationController in playerObjectController.locationsToBeSold)
                {
                    playgroundController.CmdSellLocationToTheBank(locationController.locationIndex, playerObjectController);
                }
                gameManager.CmdUpdateTurnIndex();
                playerObjectController.canSell = false;

                Destroy(playerObjectController.sellLocationsPanelData.gameObject);
            });
            playerObjectController.sellLocationsPanelData.sellButton.Interactable(true);
        }
        else
        {
            playerObjectController.sellLocationsPanelData.sellButton.Interactable(false);
        }
    }

    public void MovePlayer(int locationIndex)
    {
        if (!playgroundController)
        {
            playgroundController = PlaygroundController.Instance;
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
        playgroundController.CmdActivateEmissionOnLocation(playerObjectController.playerLocation);
        shouldMove = true;
        startTime = Time.time;
    }

    public void SetStartPosition()
    {
        if (!playgroundController)
        {
            playgroundController = PlaygroundController.Instance;
        }
        transform.position = playgroundController.locations[0].transform.position;
        playerObjectController.playerLocation = 0;
    }

    public void PlayerCosmeticsSetup()
    {
        playerMesh.material.color = playerObjectController.playerColor;
        playerObjectController.playerTurnIndicator.GetComponent<MeshRenderer>().material.color = playerObjectController.playerColor;
    }
}
