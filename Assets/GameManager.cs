using Mirror;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public GameObject gamePlayerListPanel;
    public GameObject gamePlayerListItemPrefab;
    public PlaygroundController playgroundController;

    [SyncVar(hook = nameof(TurnIndexUpdate))] public int turnIndex;
    private int _turnIndex;

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

    // Start is called before the first frame update
    void Start()
    {
        //Manager.gamePlayers[0].canPlay = true;
        //Manager.gamePlayers[0].playerInputController.canThrow = true;
        Manager.InstantiatePlayground();
        InstantiateGamePlayerListItems();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnDiceResult(int result)
    {
        Debug.Log("Applying dice result");
        Manager.gamePlayers[turnIndex].MovePlayer(result);
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
        Invoke(nameof(IncreaseTurnIndex), 5f);
    }

    private void TurnIndexUpdate(int oldValue, int newValue)
    {
        if (isServer)
        {
            if(newValue == Manager.gamePlayers.Count)
            {
                turnIndex = 0;
            }
            else
            {
                //turnIndex++;
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
        for(int i = 0; i < Manager.gamePlayers.Count; i++)
        {
            if(i == turnIndex)
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

    public void UpdateDiceResultText(string result)
    {
        Debug.Log(Manager.gamePlayers[turnIndex].gamePlayerListItem.playerDiceResultText.text);
        Manager.gamePlayers[turnIndex].gamePlayerListItem.playerDiceResultText.text = result;
    }

}
