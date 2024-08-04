using Mirror;
using Steamworks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MyNetworkManager : NetworkManager
{
    [SerializeField] private PlayerObjectController gamePlayerPrefab;
    public List<PlayerObjectController> gamePlayers { get; } = new List<PlayerObjectController>();

    public float startingMoney;

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            PlayerObjectController gamePlayerInstance = Instantiate(gamePlayerPrefab);
            gamePlayerInstance.connectionID = conn.connectionId;
            gamePlayerInstance.playerIDNumber = gamePlayers.Count + 1;
            gamePlayerInstance.playerSteamID = (ulong)SteamMatchmaking.GetLobbyMemberByIndex((CSteamID)SteamLobby.instance.currentLobbyID, gamePlayers.Count);
            if (gamePlayerInstance.connectionID == 0) // Host
            {
                gamePlayerInstance.playerLobbyIndex = 2;
            }
            else
            {
                for (int i = 0; i < 5; i++)
                {
                    if (gamePlayers.All(b => b.playerLobbyIndex != i))
                    {
                        gamePlayerInstance.playerLobbyIndex = i;
                        i = 10;
                    }
                }
            }

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
    public override void OnStartClient()
    {
        base.OnStartClient();
    }
}
