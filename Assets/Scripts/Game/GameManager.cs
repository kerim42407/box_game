using Mirror;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public GameObject playgroundPrefab;


    public GameObject gamePlayerListPanel;
    public GameObject gamePlayerListItemPrefab;
    public PlaygroundController playgroundController;
    public UIManager uiManager;
    [HideInInspector] public Material[] playerColors;

    [SyncVar(hook = nameof(TurnIndexUpdate))] public int turnIndex;
    private int _turnIndex;

    // Manager
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

    // Game Variables
    public float regularFactoryPriceMultiplier;
    public float bigFactoryPriceMultiplier;
    public float goldenFactoryPriceMultiplier;
    public float[] factoryPricesPerLevel;
    public float[] factoryValuePerLevel;
    public float resourceBuyPrice; // Resource location purchase price
    public float resourceValue; 

    // Start is called before the first frame update
    void Start()
    {
        //Manager.InstantiatePlayground();
        if (isServer)
        {
            InstantiatePlayground();
        }
        if (isClient)
        {
            SetReferences();
        }
        InstantiateGamePlayerListItems();
        foreach (PlayerObjectController playerObjectController in Manager.gamePlayers)
        {
            playerObjectController.UpdatePlayerMoney(0, playerObjectController.playerMoney);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    [Server]
    public void InstantiatePlayground()
    {
        GameObject playground = Instantiate(playgroundPrefab);
        NetworkServer.Spawn(playground);
    }

    public void SetReferences()
    {
        playgroundController = GameObject.FindGameObjectWithTag("Playground").GetComponent<PlaygroundController>();
        foreach (PlayerObjectController playerObjectController in Manager.gamePlayers)
        {
            playerObjectController.playgroundController = playgroundController;
            playerObjectController.playerMoveController.SetStartPosition();
        }
    }

    public void OnDiceResult(int result)
    {
        Debug.Log($"Applying dice result: {result}");
        Manager.gamePlayers[turnIndex].playerMoveController.MovePlayer(result);
    }

    private void InstantiateGamePlayerListItems()
    {
        for (int i = 0; i < Manager.gamePlayers.Count; i++)
        {
            GameObject qwe = Instantiate(gamePlayerListItemPrefab, gamePlayerListPanel.transform);
            qwe.GetComponent<GamePlayerListItem>().playerName = Manager.gamePlayers[i].playerName;
            qwe.GetComponent<GamePlayerListItem>().playerSteamID = Manager.gamePlayers[i].playerSteamID;
            Manager.gamePlayers[i].gamePlayerListItem = qwe.GetComponent<GamePlayerListItem>();
            qwe.GetComponent<GamePlayerListItem>().SetPlayerValues();
        }
        StartTurn();
    }

    [Server]
    private void IncreaseTurnIndex()
    {
        turnIndex++;
    }

    [Command(requiresAuthority = false)]
    public void CmdUpdateTurnIndex()
    {
        TurnIndexUpdate(turnIndex, turnIndex + 1);
    }

    private void TurnIndexUpdate(int oldValue, int newValue)
    {
        if (isServer)
        {
            if (newValue == Manager.gamePlayers.Count)
            {
                turnIndex = 0;
            }
            else
            {
                turnIndex = newValue;
            }
        }
        if (isClient)
        {
            StartTurn();
        }

    }

    public void StartTurn()
    {
        //Debug.Log($"Current turn: {turnIndex}");
        UpdateUIOnStartTurn();
    }

    private void UpdateUIOnStartTurn()
    {
        for (int i = 0; i < Manager.gamePlayers.Count; i++)
        {
            if (i == turnIndex)
            {
                Manager.gamePlayers[i].canPlay = true;
                Manager.gamePlayers[i].gamePlayerListItem.playerTurnIcon.color = Color.green;
                Manager.gamePlayers[i].playerInputController.canThrow = true;
            }
            else
            {
                Manager.gamePlayers[i].canPlay = false;
                Manager.gamePlayers[i].gamePlayerListItem.playerTurnIcon.color = Color.red;
                Manager.gamePlayers[i].playerInputController.canThrow = false;
            }
        }
    }

}
