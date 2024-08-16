using Steamworks;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Michsky.MUIP;
using Mirror;

public class LobbyController : NetworkBehaviour
{
    public static LobbyController instance;

    // UI Elements
    public TextMeshProUGUI lobbyNameText;

    // Player Data
    //public GameObject playerListViewContent;
    public GameObject playerListItemPrefab;
    public GameObject localPlayerObject;

    //
    public GameObject playerListContainer;
    public PlayerObjectController localPlayerController;

    [Header("Player Colors")]
    public List<Color> playerColorList;

    [Header("Sprites")]
    public Sprite disabledButtonTexture;
    public Sprite startGameButtonNormalTexture;
    public ButtonManager startGameButton;

    // Other Data
    public ulong currentLobbyID;
    public bool playerItemCreated;
    private List<PlayerListItem> playerListItems = new List<PlayerListItem>();
    public ButtonManager readyButton;
    public ButtonManager settingsButton;

    [Header("Game Settings")]
    public HorizontalSelector startingMoneySelector;
    private int startingMoneyIndex = 1;
    private float startingMoney;

    // Manager
    private MyNetworkManager manager;
    private MyNetworkManager Manager
    {
        get
        {
            if (manager != null)
            {
                return manager;
            }
            return manager = MyNetworkManager.singleton as MyNetworkManager;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        startingMoneySelector.defaultIndex = startingMoneyIndex;
        startingMoneySelector.SetupSelector();
        startingMoney = float.Parse(startingMoneySelector.items[startingMoneyIndex].itemTitle);
    }

    public void InviteFriendsUI()
    {
        SteamFriends.ActivateGameOverlayInviteDialog(new CSteamID(currentLobbyID));
    }

    public void ReadyPlayer()
    {
        localPlayerController.ChangeReady();
    }

    public void UpdateButton()
    {
        if (localPlayerController.ready)
        {
            readyButton.normalText.text = "Not Ready";
            readyButton.highlightedText.text = "Not Ready";
        }
        else
        {
            readyButton.normalText.text = "Ready";
            readyButton.highlightedText.text = "Ready";
        }
    }

    public void CheckIfAllReady()
    {
        bool allReady = false;
        foreach (PlayerObjectController player in Manager.gamePlayers)
        {
            if (player.ready)
            {
                allReady = true;
            }
            else
            {
                allReady = false;
                break;
            }
        }

        if (allReady)
        {
            if (localPlayerController.playerIDNumber == 1)
            {
                startGameButton.isInteractable = true;
                startGameButton.transform.GetChild(1).GetChild(0).GetComponent<Image>().sprite = startGameButtonNormalTexture;
            }
            else
            {
                startGameButton.isInteractable = false;
                startGameButton.transform.GetChild(1).GetChild(0).GetComponent<Image>().sprite = disabledButtonTexture;
            }
        }
        else
        {
            startGameButton.isInteractable = false;
            startGameButton.transform.GetChild(1).GetChild(0).GetComponent<Image>().sprite = disabledButtonTexture;
        }
    }

    public void UpdateLobbyName()
    {
        currentLobbyID = Manager.GetComponent<SteamLobby>().currentLobbyID;
        lobbyNameText.text = SteamMatchmaking.GetLobbyData(new CSteamID(currentLobbyID), "name");
    }

    public void UpdatePlayerList()
    {
        // Host
        if (!playerItemCreated)
        {
            CreateHostPlayerItem();
        }

        if (playerListItems.Count < Manager.gamePlayers.Count)
        {
            CreateClientPlayerItem();
        }
        if (playerListItems.Count > Manager.gamePlayers.Count)
        {
            RemovePlayerItem();
        }
        if (playerListItems.Count == Manager.gamePlayers.Count)
        {
            UpdatePlayerItem();
        }
    }

    public void FindLocalPlayer()
    {
        localPlayerObject = GameObject.Find("LocalGamePlayer");
        localPlayerController = localPlayerObject.GetComponent<PlayerObjectController>();
        if(localPlayerController.playerLobbyIndex == 2)
        {
            readyButton.gameObject.SetActive(true);
            startGameButton.gameObject.SetActive(true);
            settingsButton.gameObject.SetActive(true);
        }
        else
        {
            readyButton.gameObject.SetActive(true);
            startGameButton.gameObject.SetActive(false);
            settingsButton.gameObject.SetActive(false);
        }
        PlayerListItem localPlayerListItem = playerListContainer.transform.GetChild(localPlayerController.playerLobbyIndex).GetComponent<PlayerListItem>();
        localPlayerListItem.playerColorChangeButton.SetActive(true);
        localPlayerListItem.playerColorChangeButton.GetComponent<ButtonManager>().onClick.AddListener(() => localPlayerController.CmdSetPlayerColor());
    }

    public void CreateHostPlayerItem()
    {
        foreach (PlayerObjectController player in Manager.gamePlayers)
        {
            //GameObject newPlayerItem = Instantiate(playerListItemPrefab) as GameObject;
            //PlayerListItem newPlayerItemScript = newPlayerItem.GetComponent<PlayerListItem>();

            GameObject newPlayerItem = playerListContainer.transform.GetChild(2).gameObject;
            PlayerListItem newPlayerItemScript = newPlayerItem.GetComponent<PlayerListItem>();

            newPlayerItemScript.playerName = player.playerName;
            newPlayerItemScript.connectionID = player.connectionID;
            newPlayerItemScript.playerSteamID = player.playerSteamID;
            newPlayerItemScript.ready = player.ready;
            newPlayerItemScript.hostIcon.SetActive(true);
            newPlayerItemScript.inviteButton.SetActive(false);

            newPlayerItemScript.SetPlayerValues();

            //newPlayerItem.transform.SetParent(playerListViewContent.transform);
            newPlayerItem.transform.localScale = Vector3.one;

            playerListItems.Add(newPlayerItemScript);
            player.playerListItem = newPlayerItemScript;
        }
        playerItemCreated = true;
    }

    public void CreateClientPlayerItem()
    {
        foreach (PlayerObjectController player in Manager.gamePlayers)
        {
            if (!playerListItems.Any(b => b.connectionID == player.connectionID))
            {
                
                GameObject newPlayerItem = playerListContainer.transform.GetChild(player.playerLobbyIndex).gameObject;
                PlayerListItem newPlayerItemScript = newPlayerItem.GetComponent<PlayerListItem>();

                newPlayerItemScript.playerName = player.playerName;
                newPlayerItemScript.connectionID = player.connectionID;
                newPlayerItemScript.playerSteamID = player.playerSteamID;
                newPlayerItemScript.ready = player.ready;
                if(player.playerLobbyIndex == 2)
                {
                    newPlayerItemScript.hostIcon.SetActive(true);
                }
                else
                {
                    newPlayerItemScript.hostIcon.SetActive(false);
                }
                if (!newPlayerItemScript.playerItemParent.activeSelf)
                {
                    newPlayerItemScript.playerItemParent.SetActive(true);
                }
                if (newPlayerItemScript.inviteButton.activeSelf)
                {
                    newPlayerItemScript.inviteButton.SetActive(false);
                }
                newPlayerItemScript.SetPlayerValues();

                //newPlayerItem.transform.SetParent(playerListViewContent.transform);
                newPlayerItem.transform.localScale = Vector3.one;

                playerListItems.Add(newPlayerItemScript);
                player.playerListItem = newPlayerItemScript;
            }
            
            
        }
    }

    public void UpdatePlayerItem()
    {
        foreach (PlayerObjectController player in Manager.gamePlayers)
        {
            foreach (PlayerListItem playerListItemScript in playerListItems)
            {
                if (playerListItemScript.connectionID == player.connectionID)
                {
                    playerListItemScript.playerName = player.playerName;
                    playerListItemScript.ready = player.ready;
                    playerListItemScript.SetPlayerValues();
                    if (player == localPlayerController)
                    {
                        UpdateButton();
                    }
                }
            }
        }
        CheckIfAllReady();
    }

    public void RemovePlayerItem()
    {
        List<PlayerListItem> playerListItemToRemove = new List<PlayerListItem>();

        foreach (PlayerListItem playerListItem in playerListItems)
        {
            if (!Manager.gamePlayers.Any(b => b.connectionID == playerListItem.connectionID))
            {
                playerListItemToRemove.Add(playerListItem);
            }
        }
        if (playerListItemToRemove.Count > 0)
        {
            foreach (PlayerListItem playerlistItemToRemove in playerListItemToRemove)
            {
                GameObject objectToRemove = playerlistItemToRemove.gameObject;
                playerListItems.Remove(playerlistItemToRemove);
                //Destroy(objectToRemove);
                ClearPlayerListItem(playerlistItemToRemove);
                objectToRemove = null;
            }
        }
    }

    private void ClearPlayerListItem(PlayerListItem objectToClear)
    {
        GameObject _gameObject = objectToClear.gameObject;
        Destroy(objectToClear);
        PlayerListItem _playerListItem = _gameObject.AddComponent<PlayerListItem>();
        _playerListItem.playerItemParent = _gameObject.transform.GetChild(0).gameObject;
        _playerListItem.playerNameText = _playerListItem.playerItemParent.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        _playerListItem.playerReadyText = _playerListItem.playerItemParent.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        _playerListItem.playerIcon = _playerListItem.playerItemParent.transform.GetChild(2).GetComponent<RawImage>();
        _playerListItem.hostIcon = _playerListItem.playerItemParent.transform.GetChild(3).gameObject;
        _playerListItem.inviteButton = _gameObject.transform.GetChild(1).gameObject;
        _playerListItem.playerColorImage = _playerListItem.playerItemParent.transform.GetChild(4).GetComponent<Image>();
        _playerListItem.playerColorChangeButton = _playerListItem.playerItemParent.transform.GetChild(5).gameObject;
        _playerListItem.playerItemParent.SetActive(false);
        _playerListItem.inviteButton.SetActive(true);
        _playerListItem.SetPlayerValues();
    }

    public void StartGame(string sceneName)
    {
        SetGameSettings();
        localPlayerController.CanStartGame(sceneName);
    }

    public void BackButton()
    {
        if (localPlayerController.isServer)
        {
            Manager.StopHost();
        }
        if (localPlayerController.isClient)
        {
            Manager.StopClient();
        }
    }

    public void OnSettingsPanelOpen()
    {
        startingMoneySelector.index = startingMoneyIndex;
        startingMoneySelector.UpdateUI();
    }
    public void OnSettingsPanelConfirm()
    {
        startingMoneyIndex = startingMoneySelector.index;
        startingMoney = float.Parse(startingMoneySelector.items[startingMoneyIndex].itemTitle);
    }

    public void SetGameSettings()
    {
        Manager.startingMoney = startingMoney;
    }
}
