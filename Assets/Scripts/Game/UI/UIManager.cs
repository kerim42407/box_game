using Mirror;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    #region Fields and Properties
    public GameObject mainCanvas;

    [SerializeField] private TMP_Text diceOneText, diceTwoText;
    private int diceOneResult, diceTwoResult;
    private bool diceOne, diceTwo;

    [Header("Prefabs")]
    public GameObject localPlayerListItemPrefab;
    public GameObject gamePlayerListItemPrefab;

    [Header("Panel Prefabs")]
    public GameObject factoryBuyPanelPrefab;
    public GameObject factoryUpgradePanelPrefab;
    public GameObject resourceBuyPanelPrefab;
    public GameObject sellLocationsPanelPrefab;

    [Header("Panel Frames")]
    public Sprite normalFrame;
    public Sprite goldenFrame;

    [Header("References")]
    public GameManager gameManager;
    public GameObject infoCanvas;
    public GameObject playerCardContainer;
    public GameObject locationInfoPanel;
    public GameObject localPlayerListItemPanel;
    public GameObject gamePlayersListItemPanel;
    #endregion

    private void OnEnable()
    {
        Dice.OnDiceResult += SetText;
    }

    private void OnDisable()
    {
        Dice.OnDiceResult -= SetText;
    }

    private void SetText(int diceIndex, int diceResult)
    {
        if (diceIndex == 0)
        {
            diceOneResult = diceResult;
            diceOne = true;
            CheckDiceResult();
            //diceOneText.SetText($"Dice one: {diceResult}");
        }
        else
        {
            diceTwoResult = diceResult;
            diceTwo = true;
            CheckDiceResult();
            //diceTwoText.SetText($"Dice two: {diceResult}");
        }
    }

    private void CheckDiceResult()
    {
        if (diceOne && diceTwo)
        {
            bool isEven = false;
            //Debug.Log($"Dice result: {diceOneResult} + {diceTwoResult} = {diceOneResult + diceTwoResult}");
            if(diceOneResult == diceTwoResult)
            {
                isEven = true;
            }
            gameManager.OnDiceResult(diceOneResult + diceTwoResult, isEven);
            diceOne = false;
            diceTwo = false;
        }
        else
        {

        }
    }
}
