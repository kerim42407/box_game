using Mirror;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    [Header("References")]
    public GameObject playgroundPrefab;
    public GameObject canvas;
    public PlayerObjectController localPlayerController;
    public GameObject gamePlayerListPanel;
    public GameObject gamePlayerListItemPrefab;
    [HideInInspector]public PlaygroundController playgroundController;
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

    [Header("Game Variables")]
    public float resourceBuyPrice; // Resource location purchase price
    public float resourceRentRate;
    public float startingPointIncome;

    public float baseFactoryPrice;
    public float regularFactoryPriceCoef;
    public float bigFactoryPriceCoef;
    public float goldenFactoryPriceCoef;
    public float[] factoryPriceCoefPerLevel;

    public float resourceProductivityCoef;
    public float bonus;

    [Header("Effects")]
    public EventBase resourcePositiveEvent;
    public EventBase resourceNegativeEvent;


    [Header("Debug References")]
    public TMP_InputField inputField;
    public GameObject customDiceButton;

    // Start is called before the first frame update
    void Start()
    {
        if (isServer)
        {
            InstantiatePlayground();
            inputField.gameObject.SetActive(true);
            customDiceButton.gameObject.SetActive(true);
        }
        else
        {
            inputField.gameObject.SetActive(false);
            customDiceButton.gameObject.SetActive(false);
        }
        if (isClient)
        {
            SetReferences();
        }
        InstantiateGamePlayerListItems();
        foreach (PlayerObjectController playerObjectController in Manager.gamePlayers)
        {
            playerObjectController.UpdatePlayerMoney(0, playerObjectController.playerMoney);
            playerObjectController.playerMoveController.mainCamera = Camera.main;
        }
        startingPointIncome = Manager.gamePlayers[0].playerMoney / 2; 
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
        if(result % 2 == 0)
        {
            Manager.gamePlayers[turnIndex].playerMoveController.isEven = !Manager.gamePlayers[turnIndex].playerMoveController.isEven;
        }
        else
        {
            if (Manager.gamePlayers[turnIndex].playerMoveController.isEven)
            {
                Manager.gamePlayers[turnIndex].playerMoveController.isEven = false;
            }
        }
        Manager.gamePlayers[turnIndex].playerMoveController.MovePlayer(result);
    }

    public void CustomDiceResult(int result)
    {
        Manager.gamePlayers[turnIndex].playerMoveController.MovePlayer(int.Parse(inputField.text));
    }

    public void Test()
    {
        Manager.gamePlayers[turnIndex].playerMoveController.Test();
    }

    private void InstantiateGamePlayerListItems()
    {
        for (int i = 0; i < Manager.gamePlayers.Count; i++)
        {
            GameObject qwe = Instantiate(gamePlayerListItemPrefab, gamePlayerListPanel.transform);
            qwe.GetComponent<GamePlayerListItem>().playerName = Manager.gamePlayers[i].playerName;
            qwe.GetComponent<GamePlayerListItem>().playerSteamID = Manager.gamePlayers[i].playerSteamID;
            qwe.GetComponent<GamePlayerListItem>().background.color = Manager.gamePlayers[i].playerColor;
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
        if (Manager.gamePlayers[turnIndex].GetComponent<PlayerMoveController>().isEven)
        {
            TurnIndexUpdate(turnIndex, turnIndex);
        }
        else
        {
            TurnIndexUpdate(turnIndex, turnIndex + 1);
        }
        
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
