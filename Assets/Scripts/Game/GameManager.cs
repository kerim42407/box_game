using Michsky.MUIP;
using Mirror;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;



public class GameManager : NetworkBehaviour
{
    [Header("References")]
    public UIManager uiManager;
    public GameObject playgroundPrefab;
    public GameObject canvas;
    public PlayerObjectController localPlayerController;
    [HideInInspector] public PlaygroundController playgroundController;
    [HideInInspector] public Material[] playerColors;

    [SyncVar] public int turnIndex;
    [SyncVar] public int turnCount;

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
        if (isClient)
        {
            CmdSendSetupGameRequest(GetComponent<NetworkIdentity>().connectionToClient);
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
        Manager.gamePlayers[turnIndex].turnCount++;
    }
    [Command(requiresAuthority = false)]
    public void CmdSendSetupGameRequest(NetworkConnectionToClient target)
    {
        RpcSendSetupGameRequest(target);
    }
    [TargetRpc]
    public void RpcSendSetupGameRequest(NetworkConnectionToClient target)
    {
        playgroundController = GameObject.FindGameObjectWithTag("Playground").GetComponent<PlaygroundController>();
        foreach (PlayerObjectController playerObjectController in Manager.gamePlayers)
        {
            playerObjectController.playgroundController = playgroundController;
            playerObjectController.playerMoveController.SetStartPosition();
        }
        for (int i = 0; i < Manager.gamePlayers.Count; i++)
        {
            GamePlayerListItem gamePlayerListItem;
            if (Manager.gamePlayers[i].isLocalPlayer)
            {
                gamePlayerListItem = Instantiate(uiManager.localPlayerListItemPrefab, uiManager.localPlayerListItemPanel.transform).GetComponent<GamePlayerListItem>();
            }
            else
            {
                gamePlayerListItem = Instantiate(uiManager.gamePlayerListItemPrefab, uiManager.gamePlayersListItemPanel.transform).GetComponent<GamePlayerListItem>();
            }
            gamePlayerListItem.playerName = Manager.gamePlayers[i].playerName;
            gamePlayerListItem.playerSteamID = Manager.gamePlayers[i].playerSteamID;
            gamePlayerListItem.background.color = Manager.gamePlayers[i].playerColor;
            gamePlayerListItem.playerMoneyText.text = "$" + string.Format(CultureInfo.InvariantCulture, "{0:N0}", Manager.gamePlayers[i].playerMoney);
            Manager.gamePlayers[i].gamePlayerListItem = gamePlayerListItem;
            gamePlayerListItem.playerNameText.GetComponent<AutoSizeController>().AdjustFontSize();
            gamePlayerListItem.playerMoneyText.GetComponent<AutoSizeController>().AdjustFontSize();
            gamePlayerListItem.SetPlayerValues();
        }
        for (int i = 0; i < Manager.gamePlayers.Count; i++)
        {
            if (i == turnIndex)
            {
                Manager.gamePlayers[i].canPlay = true;
                Manager.gamePlayers[i].playerInputController.canThrow = true;
            }
            else
            {
                Manager.gamePlayers[i].canPlay = false;
                Manager.gamePlayers[i].playerInputController.canThrow = false;
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
        bool shouldIncreaseTurnCount = false;
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
                shouldIncreaseTurnCount = true;
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
            shouldIncreaseTurnCount = true;
        }

        if (shouldIncreaseTurnCount)
        {
            //foreach(Card card in Manager.gamePlayers[turnIndex].activeCards)
            //{
            //    card.count++;
            //    if(card.count == card.duration)
            //    {
            //        card.DestroyCard(Manager.gamePlayers[turnIndex]);
            //        Debug.Log("Destroy Card");
            //    }

            //}
            Manager.gamePlayers[turnIndex].turnCount++;
            
        }
        RpcUpdateTurnIndex(turnIndex);
        RpcShowNotification(Manager.gamePlayers[turnIndex].connectionToClient);

    }
    [ClientRpc]
    private void RpcUpdateTurnIndex(int index)
    {
        for (int i = 0; i < Manager.gamePlayers.Count; i++)
        {
            if (i == index)
            {
                Manager.gamePlayers[i].canPlay = true;
                Manager.gamePlayers[i].playerInputController.canThrow = true;
            }
            else
            {
                Manager.gamePlayers[i].canPlay = false;
                Manager.gamePlayers[i].playerInputController.canThrow = false;
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
