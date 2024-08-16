using Mirror;
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
    [HideInInspector] public PlaygroundController playgroundController;
    public UIManager uiManager;
    [HideInInspector] public Material[] playerColors;

    [SyncVar] public int turnIndex;
    [SyncVar] public bool isGameStarted;

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
    public float resourceBuyPrice;
    public float resourceRentRate;
    [SyncVar] public float startingPointIncome;

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

    [Header("Notification Events")]
    public NotificationEventBase notificationEventBase;

    [Header("Debug References")]
    public TMP_InputField inputField;
    public GameObject customDiceButton;

    // Start is called before the first frame update
    void Start()
    {
        if (isServer)
        {
            CmdSetupGame();
            inputField.gameObject.SetActive(true);
            customDiceButton.gameObject.SetActive(true);
        }
        else
        {
            inputField.gameObject.SetActive(false);
            customDiceButton.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    [Command(requiresAuthority = false)]
    public void CmdSetupGame()
    {
        GameObject playground = Instantiate(playgroundPrefab);
        NetworkServer.Spawn(playground);
        foreach (PlayerObjectController playerObjectController in Manager.gamePlayers)
        {
            playerObjectController.playerMoney = Manager.startingMoney * 1000;
        }
        startingPointIncome = (Manager.startingMoney * 1000) / 2;
        RpcShowNotification(Manager.gamePlayers[turnIndex].connectionToClient);
        RpcSetupGame();
    }

    [ClientRpc]
    private void RpcSetupGame()
    {
        Debug.Log("RpcSetupGame");
        playgroundController = GameObject.FindGameObjectWithTag("Playground").GetComponent<PlaygroundController>();
        foreach (PlayerObjectController playerObjectController in Manager.gamePlayers)
        {
            playerObjectController.playgroundController = playgroundController;
            playerObjectController.playerMoveController.SetStartPosition();
        }
        for (int i = 0; i < Manager.gamePlayers.Count; i++)
        {
            GameObject qwe = Instantiate(gamePlayerListItemPrefab, gamePlayerListPanel.transform);
            qwe.GetComponent<GamePlayerListItem>().playerName = Manager.gamePlayers[i].playerName;
            qwe.GetComponent<GamePlayerListItem>().playerSteamID = Manager.gamePlayers[i].playerSteamID;
            qwe.GetComponent<GamePlayerListItem>().background.color = Manager.gamePlayers[i].playerColor;
            qwe.GetComponent<GamePlayerListItem>().playerMoneyText.text = Manager.gamePlayers[i].playerMoney.ToString();
            Manager.gamePlayers[i].gamePlayerListItem = qwe.GetComponent<GamePlayerListItem>();
            qwe.GetComponent<GamePlayerListItem>().SetPlayerValues();
        }
        for (int i = 0; i < Manager.gamePlayers.Count; i++)
        {
            if (i == turnIndex)
            {
                Manager.gamePlayers[i].canPlay = true;
                Manager.gamePlayers[i].playerInputController.canThrow = true;
                Manager.gamePlayers[i].gamePlayerListItem.playerTurnIcon.color = Color.green;
            }
            else
            {
                Manager.gamePlayers[i].canPlay = false;
                Manager.gamePlayers[i].playerInputController.canThrow = false;
                Manager.gamePlayers[i].gamePlayerListItem.playerTurnIcon.color = Color.red;
            }
        }
    }

    public void OnDiceResult(int result, bool isEven)
    {
        Debug.Log($"Applying dice result: {result}");
        if (isEven)
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

    [Command(requiresAuthority = false)]
    public void CmdUpdateTurnIndex()
    {
        if (!Manager.gamePlayers[turnIndex].isBankrupt)
        {
            if (Manager.gamePlayers[turnIndex].GetComponent<PlayerMoveController>().isEven)
            {

            }
            else
            {
                turnIndex++;
                if (turnIndex == Manager.gamePlayers.Count)
                {
                    turnIndex = 0;
                }
                while (Manager.gamePlayers[turnIndex].isBankrupt)
                {
                    turnIndex++;
                    if (turnIndex == Manager.gamePlayers.Count)
                    {
                        turnIndex = 0;
                    }
                }
            }
        }
        else
        {
            while (Manager.gamePlayers[turnIndex].isBankrupt)
            {
                turnIndex++;
                if (turnIndex == Manager.gamePlayers.Count)
                {
                    turnIndex = 0;
                }
            }
        }
        
        RpcUpdateTurnIndex(turnIndex);
        RpcShowNotification(Manager.gamePlayers[turnIndex].connectionToClient);

    }
    [ClientRpc]
    private void RpcUpdateTurnIndex(int index)
    {
        Debug.Log(index);
        for (int i = 0; i < Manager.gamePlayers.Count; i++)
        {
            if (i == index)
            {
                Manager.gamePlayers[i].canPlay = true;
                Manager.gamePlayers[i].playerInputController.canThrow = true;
                Manager.gamePlayers[i].gamePlayerListItem.playerTurnIcon.color = Color.green;
            }
            else
            {
                Manager.gamePlayers[i].canPlay = false;
                Manager.gamePlayers[i].playerInputController.canThrow = false;
                Manager.gamePlayers[i].gamePlayerListItem.playerTurnIcon.color = Color.red;
            }
        }
    }

    #region Notification Event Functions
    [TargetRpc]
    private void RpcShowNotification(NetworkConnectionToClient target)
    {
        notificationEventBase.ShowNotification(uiManager.mainCanvas.transform);
    }
    #endregion

}
