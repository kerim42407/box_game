using Mirror;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameManager gameManager;

    [SerializeField] private TMP_Text diceOneText, diceTwoText;
    private int diceOneResult, diceTwoResult;
    private bool diceOne, diceTwo;

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
            Debug.Log($"Dice result: {diceOneResult} + {diceTwoResult} = {diceOneResult + diceTwoResult}");
            gameManager.OnDiceResult(diceOneResult + diceTwoResult);
            diceOne = false;
            diceTwo = false;
        }
        else
        {

        }
    }
}
