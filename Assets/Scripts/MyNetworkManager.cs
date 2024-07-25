using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Steamworks;

public class MyNetworkManager : NetworkManager
{
    [SerializeField] private PlayerObjectController gamePlayerPrefab;
    public List<PlayerObjectController> gamePlayers { get; } = new List<PlayerObjectController>();

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        if(SceneManager.GetActiveScene().name == "Lobby")
        {
            PlayerObjectController gamePlayerInstance = Instantiate(gamePlayerPrefab);
            gamePlayerInstance.connectionID = conn.connectionId;
            gamePlayerInstance.playerIDNumber = gamePlayers.Count + 1;
            gamePlayerInstance.playerSteamID = (ulong)SteamMatchmaking.GetLobbyMemberByIndex((CSteamID)SteamLobby.instance.currentLobbyID, gamePlayers.Count);

            NetworkServer.AddPlayerForConnection(conn, gamePlayerInstance.gameObject);
        }
    }
    public void StartGame(string sceneName)
    {
        ServerChangeScene(sceneName);
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
    }

    //public override void OnClientConnect()
    //{
    //    base.OnClientConnect();
    //    clientCount++;
    //    Debug.Log(clientCount);
    //}

    //public override void OnClientDisconnect()
    //{
    //    base.OnClientDisconnect();
    //    clientCount--;
    //    Debug.Log(clientCount);
    //}

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    public void InstantiatePlayground()
    {
        //foreach(PlayerObjectController playerObjectController in gamePlayers)
        //{
        //    playerObjectController.playgroundController = currentPlayground.GetComponent<PlaygroundController>();
        //    playerObjectController.playerMoveController.SetStartPosition();
        //}
    }

}
