using Mirror;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerObjectController : NetworkBehaviour
{
    // Player Data
    [SyncVar] public int connectionID;
    [SyncVar] public int playerIDNumber;
    [SyncVar] public ulong playerSteamID;
    [SyncVar(hook = nameof(PlayerNameUpdate))] public string playerName;
    [SyncVar(hook = nameof(PlayerReadyUpdate))] public bool ready;

    // Cosmetics
    [SyncVar(hook = nameof(SendPlayerColor))] public int playerColor;

    [SyncVar] public bool canPlay;
    [SyncVar] public int playerLocation;
    [SyncVar(hook = nameof(UpdatePlayerMoney))] public float playerMoney = 200000;

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

    [HideInInspector] public PlayerInputController playerInputController;
    public PlaygroundController playgroundController;

    [HideInInspector] public PlayerMoveController playerMoveController;

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
    }

    public override void OnStopClient()
    {
        Manager.gamePlayers.Remove(this);
        LobbyController.instance.UpdatePlayerList();
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
    [Command]
    public void CmdSendPlayerColor(int newValue)
    {
        SendPlayerColor(playerColor, newValue);
    }

    public void SendPlayerColor(int oldValue, int newValue)
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

    private void UpdateColor(int message)
    {
        playerColor = message;
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
        gamePlayerListItem.playerMoneyText.text = playerMoney.ToString();
    }
}
