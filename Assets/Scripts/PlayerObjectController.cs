using Mirror;
using Steamworks;
using System.Collections.Generic;
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
    public GamePlayerListItem gamePlayerListItem;

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

    [HideInInspector] public PlayerInputController playerInputController;
    public PlaygroundController playgroundController;
    [SyncVar] public int playerLocation;

    private bool move;
    Vector3 startPosition;
    Vector3 target;
    private int destinationIndex;
    

    public List<Transform> targetTransforms = new List<Transform>();

    private Transform firstPos, lastPos;
    public float journeyTime = 10.0f;
    private float startTime;


    private void Start()
    {
        playerInputController = GetComponent<PlayerInputController>();
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
        }

        if (GetComponent<NetworkIdentity>().isOwned)
        {
            if (move)
            {
                if(transform.position == lastPos.position)
                {
                    SetDestination(targetTransforms[destinationIndex].position);
                }
                else
                {
                    Vector3 center = (firstPos.position + lastPos.position) * 0.5f;

                    center -= new Vector3(0, 1, 0);

                    Vector3 firstPosCenter = firstPos.position - center;
                    Vector3 lastPosCenter = lastPos.position - center;

                    float fracComplete = (Time.time - startTime) / journeyTime;

                    transform.position = Vector3.Slerp(firstPosCenter, lastPosCenter, fracComplete);
                    transform.position += center;
                }
            }
        }
        
    }

    public void SetStartPosition()
    {
        transform.position = playgroundController.locations[0].transform.position;
        playerLocation = 0;
    }
    public void MovePlayer(int locationIndex)
    {
        for (int i = playerLocation; i < locationIndex; i++)
        {
            targetTransforms.Add(playgroundController.locations[i + 1].transform);
        }
        if (playerLocation + locationIndex >= playgroundController.locations.Count)
        {
            playerLocation = (playerLocation + locationIndex) - playgroundController.locations.Count;
        }
        else
        {
            playerLocation = playerLocation + locationIndex;
        }
        //firstPos = transform;
        //lastPos = targetTransforms[5];
        //startTime = Time.time;
        //move = true;
        //Debug.Log(journeyTime);
        //transform.position = playgroundController.locations[playerLocation].transform.position;

        SetDestination(targetTransforms[destinationIndex].position);
    }

    public void MoveTest()
    {

    }

    public void SetDestination(Vector3 destination)
    {
        if (transform.position == destination)
        {
            Debug.Log("Reached target");
            if (destinationIndex == targetTransforms.Count)
            {

            }
            else
            {
                destinationIndex++;
                target = targetTransforms[destinationIndex].position;
            }

        }
        else
        {
            target = targetTransforms[destinationIndex].position;
        }
        firstPos = transform;
        lastPos = targetTransforms[destinationIndex];
        move = true;
        startTime = Time.time;
        Debug.Log(firstPos.position);
        Debug.Log(lastPos.position);
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
}
