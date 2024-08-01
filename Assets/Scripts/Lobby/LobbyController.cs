using Steamworks;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LobbyController : MonoBehaviour
{
    public static LobbyController instance;

    // UI Elements
    public TextMeshProUGUI lobbyNameText;

    // Player Data
    public GameObject playerListViewContent;
    public GameObject playerListItemPrefab;
    public GameObject localPlayerObject;

    // Other Data
    public ulong currentLobbyID;
    public bool playerItemCreated;
    private List<PlayerListItem> playerListItems = new List<PlayerListItem>();
    public PlayerObjectController localPlayerController;

    // Ready
    public Button startGameButton;
    public TextMeshProUGUI readyButtonText;

    //public int startingGameMoney;

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
            readyButtonText.text = "Not Ready";
        }
        else
        {
            readyButtonText.text = "Ready";
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
                startGameButton.interactable = true;
            }
            else
            {
                startGameButton.interactable = false;
            }
        }
        else
        {
            startGameButton.interactable = false;
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
    }

    public void CreateHostPlayerItem()
    {
        foreach (PlayerObjectController player in Manager.gamePlayers)
        {
            GameObject newPlayerItem = Instantiate(playerListItemPrefab) as GameObject;
            PlayerListItem newPlayerItemScript = newPlayerItem.GetComponent<PlayerListItem>();

            newPlayerItemScript.playerName = player.playerName;
            newPlayerItemScript.connectionID = player.connectionID;
            newPlayerItemScript.playerSteamID = player.playerSteamID;
            newPlayerItemScript.ready = player.ready;
            newPlayerItemScript.SetPlayerValues();

            newPlayerItem.transform.SetParent(playerListViewContent.transform);
            newPlayerItem.transform.localScale = Vector3.one;

            playerListItems.Add(newPlayerItemScript);
        }
        playerItemCreated = true;
    }

    public void CreateClientPlayerItem()
    {
        foreach (PlayerObjectController player in Manager.gamePlayers)
        {
            if (!playerListItems.Any(b => b.connectionID == player.connectionID))
            {
                GameObject newPlayerItem = Instantiate(playerListItemPrefab) as GameObject;
                PlayerListItem newPlayerItemScript = newPlayerItem.GetComponent<PlayerListItem>();

                newPlayerItemScript.playerName = player.playerName;
                newPlayerItemScript.connectionID = player.connectionID;
                newPlayerItemScript.playerSteamID = player.playerSteamID;
                newPlayerItemScript.ready = player.ready;
                newPlayerItemScript.SetPlayerValues();

                newPlayerItem.transform.SetParent(playerListViewContent.transform);
                newPlayerItem.transform.localScale = Vector3.one;

                playerListItems.Add(newPlayerItemScript);
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
                Destroy(objectToRemove);
                objectToRemove = null;
            }
        }
    }

    public void StartGame(string sceneName)
    {
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
            Manager.StartClient();
        }
    }
}
