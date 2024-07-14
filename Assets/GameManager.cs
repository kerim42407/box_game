using Mirror;
using Steamworks;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject gamePlayerListPanel;
    public GameObject gamePlayerListItemPrefab;

    public int turnIndex = -1;

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
        InstantiateGamePlayerListItems();
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void InstantiateGamePlayerListItems()
    {
        for(int i = 0; i < Manager.gamePlayers.Count; i++)
        {
            GameObject qwe = Instantiate(gamePlayerListItemPrefab, gamePlayerListPanel.transform);
            qwe.GetComponent<GamePlayerListItem>().playerName = Manager.gamePlayers[i].playerName;
            qwe.GetComponent<GamePlayerListItem>().playerSteamID = Manager.gamePlayers[i].playerSteamID;
            Manager.gamePlayers[i].gamePlayerListItem = qwe.GetComponent<GamePlayerListItem>();
            qwe.GetComponent<GamePlayerListItem>().SetPlayerValues();
        }
        PassTurn();
    }

    public void PassTurn()
    {
        if(turnIndex != -1)
        {
            Manager.gamePlayers[turnIndex].gamePlayerListItem.playerTurnIcon.color = Color.red;
            Manager.gamePlayers[turnIndex].canPlay = false;
        }
        
        if(turnIndex == Manager.gamePlayers.Count - 1)
        {
            turnIndex = 0;
        }
        else
        {
            turnIndex++;
        }
        Manager.gamePlayers[turnIndex].canPlay = true;
        Manager.gamePlayers[turnIndex].gamePlayerListItem.playerTurnIcon.color = Color.green;
        Manager.gamePlayers[turnIndex].playerInputController.canThrow = true;
        Debug.Log($"Current turn: {turnIndex}");
    }

    public void UpdateDiceResultText(string result)
    {
        Debug.Log(Manager.gamePlayers[turnIndex].gamePlayerListItem.playerDiceResultText.text);
        Manager.gamePlayers[turnIndex].gamePlayerListItem.playerDiceResultText.text = result;
    }

}
