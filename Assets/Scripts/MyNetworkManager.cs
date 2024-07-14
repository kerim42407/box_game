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


    private int clientCount;

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
        Debug.Log("Server Started");
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
        Debug.Log("Client Connected");
    }

}
