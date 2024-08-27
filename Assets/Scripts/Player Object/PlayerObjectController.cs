using Mirror;
using Steamworks;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerObjectController : NetworkBehaviour
{
    #region Fields and Properties


    // Player Data
    [SyncVar] public int connectionID;
    [SyncVar] public int playerIDNumber;
    [SyncVar] public ulong playerSteamID;
    [SyncVar] public int playerLobbyIndex;
    [SyncVar(hook = nameof(PlayerNameUpdate))] public string playerName;
    [SyncVar(hook = nameof(PlayerReadyUpdate))] public bool ready;

    public PlayerListItem playerListItem;

    [Header("Player Cosmetics")]
    public List<Color> playerColors;
    [SyncVar(hook = nameof(SetPlayerColor))] public Color playerColor;

    [SyncVar] public int turnCount;
    [SyncVar] public bool isBankrupt;
    [SyncVar] public bool canPlay;
    [SyncVar] public bool canSell;
    [SyncVar] public int playerLocation;
    [SyncVar(hook = nameof(UpdatePlayerMoney))] public float playerMoney;

    public GamePlayerListItem gamePlayerListItem;

    private MyNetworkManager manager;
    public MyNetworkManager Manager
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
    public GameManager gameManager;

    [Header("Sync Variables")]
    //[SyncVar] public List<LocationController> s_PlayerOwnedLocations;
    //[SyncVar] public List<LocationController> s_PlayerOwnedResources; 
    public readonly SyncList<LocationController> s_PlayerOwnedLocations = new ();
    public readonly SyncList<LocationController> s_PlayerOwnedResources = new();
    public readonly SyncList<Card> s_PlayerActiveCards = new ();
    [HideInInspector] public PlayerInputController playerInputController;
    [HideInInspector] public PlaygroundController playgroundController;
    [HideInInspector] public PlayerMoveController playerMoveController;
    [HideInInspector] public SellLocationsPanelData sellLocationsPanelData;
    [HideInInspector] public List<LocationController> locationsToBeSold = new();
    [HideInInspector] public float locationsToBeSoldValue;

    #endregion

    private void Start()
    {
        playerInputController = GetComponent<PlayerInputController>();
        playerMoveController = GetComponent<PlayerMoveController>();
        DontDestroyOnLoad(this.gameObject);
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "Game")
        {
            if (!playerInputController.enabled)
            {
                playerInputController.enabled = true;
            }
            if (!gameManager)
            {
                if (GameObject.Find("Game Manager"))
                {
                    gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
                    playerMoveController.gameManager = gameManager;
                    if (isLocalPlayer)
                    {
                        gameManager.localPlayerController = this;
                    }
                }
            }
            if (!playerMoveController.didCosmetic)
            {
                playerMoveController.PlayerCosmeticsSetup();
            }
        }



    }

    private void PlayerReadyUpdate(bool oldValue, bool newValue)
    {
        if (isServer)
        {
            ready = newValue;
        }
        if (isClient)
        {
            LobbyController.instance.UpdatePlayerList();
        }
    }

    [Command]
    private void CmdSetPlayerReady()
    {
        PlayerReadyUpdate(ready, !ready);
    }

    public void ChangeReady()
    {
        if (GetComponent<NetworkIdentity>().isOwned)
        {
            CmdSetPlayerReady();
        }
    }

    public override void OnStartAuthority()
    {
        CmdSetPlayerName(SteamFriends.GetPersonaName().ToString());
        gameObject.name = "LocalGamePlayer";
        LobbyController.instance.FindLocalPlayer();
        LobbyController.instance.UpdateLobbyName();
    }

    public override void OnStartClient()
    {
        Manager.gamePlayers.Add(this);
        LobbyController.instance.UpdateLobbyName();
        LobbyController.instance.UpdatePlayerList();
        if (isLocalPlayer)
        {
            CmdSetPlayerColor();
        }
        
    }

    public override void OnStopClient()
    {
        Manager.gamePlayers.Remove(this);
        if(SceneManager.GetActiveScene().name == "Lobby")
        {
            LobbyController.instance.UpdatePlayerList();
        }
    }

    [Command]
    private void CmdSetPlayerName(string PlayerName)
    {
        this.PlayerNameUpdate(this.playerName, PlayerName);
    }

    public void PlayerNameUpdate(string oldValue, string newValue)
    {
        if (isServer)
        {
            this.playerName = newValue;
        }

        if (isClient)
        {
            LobbyController.instance.UpdatePlayerList();
        }
    }

    // Start Game

    public void CanStartGame(string sceneName)
    {
        if (GetComponent<NetworkIdentity>().isOwned)
        {
            CmdCanStartGame(sceneName);
        }
    }


    [Command]
    public void CmdCanStartGame(string sceneName)
    {
        manager.StartGame(sceneName);

    }

    // Cosmetics
    //[Command]
    //public void CmdSendPlayerColor(Color newValue)
    //{
    //    SendPlayerColor(playerColor, newValue);
    //}

    //public void SendPlayerColor(Color oldValue, Color newValue)
    //{
    //    if (isServer)
    //    {
    //        playerColor = newValue;
    //    }
    //    if (isClient && (oldValue != newValue))
    //    {
    //        UpdateColor(newValue);
    //    }
    //}

    //private void UpdateColor(Color message)
    //{
    //    playerColor = message;
    //}

    // My Cosmetics
    [Command(requiresAuthority = false)]
    public void CmdSetPlayerColor()
    {
        List<Color> availibleColors = new List<Color>(playerColors);
        foreach(PlayerObjectController playerObjectController in Manager.gamePlayers)
        {
            availibleColors.Remove(playerObjectController.playerColor);
        }
        int randomColorIndex = Random.Range(0, availibleColors.Count);
        SetPlayerColor(playerColor, availibleColors[randomColorIndex]);
    }
    public void SetPlayerColor(Color oldValue, Color newValue)
    {
        if (isServer)
        {
            playerColor = newValue;
        }
        if (isClient && (oldValue != newValue))
        {
            UpdateColor(newValue);
        }
    }
    private void UpdateColor(Color message)
    {
        playerColor = message;
        if (playerListItem)
        {
            playerListItem.playerColorImage.color = playerColor;
        }
        else
        {
            Invoke(nameof(Test), .25f);
        }
        
    }
    private void Test()
    {
        if (playerListItem)
        {
            playerListItem.playerColorImage.color = playerColor;
        }
        else
        {
            Invoke(nameof(Test), .25f);
        }
    }

    // Player Money
    [Command(requiresAuthority = false)]
    public void CmdUpdatePlayerMoney(float newValue)
    {
        UpdatePlayerMoney(playerMoney, newValue);
    }

    public void UpdatePlayerMoney(float oldValue, float newValue)
    {
        if (isServer)
        {
            playerMoney = newValue;
        }
        if (isClient && (oldValue != newValue))
        {
            UpdateMoney(newValue);
        }
    }

    private void UpdateMoney(float value)
    {
        playerMoney = value;
        if (gamePlayerListItem)
        {
            gamePlayerListItem.playerMoneyText.text = "$" + string.Format(CultureInfo.InvariantCulture, "{0:N0}", playerMoney);
        }
        
    }
}
