using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;
using Mirror;
using TMPro;

public class CharacterCosmetics : MonoBehaviour
{
    public int currentColorIndex = 0;
    public Material[] playerColors;
    public Image currentColorImage;
    public TextMeshProUGUI currentColorText;
    public GameObject localPlayerObject;
    public PlayerObjectController localPlayerController;

    private void Start()
    {
        currentColorIndex = PlayerPrefs.GetInt("currentColorIndex", 0);
        currentColorImage.color = playerColors[currentColorIndex].color;
        currentColorText.text = playerColors[currentColorIndex].name;
        Invoke(nameof(FindLocalPlayer), .1f);
    }
    public void FindLocalPlayer()
    {
        if (GameObject.Find("LocalGamePlayer"))
        {
            localPlayerObject = GameObject.Find("LocalGamePlayer");
            localPlayerController = localPlayerObject.GetComponent<PlayerObjectController>();
            localPlayerController.CmdSendPlayerColor(currentColorIndex);
        }
        else
        {
            Invoke(nameof(FindLocalPlayer), .1f);
        }
        
    }

    public void NextColor()
    {
        if(currentColorIndex < playerColors.Length - 1)
        {
            currentColorIndex++;
            PlayerPrefs.SetInt("currentColorIndex", currentColorIndex);
            currentColorImage.color = playerColors[currentColorIndex].color;
            currentColorText.text = playerColors[currentColorIndex].name;
            localPlayerController.CmdSendPlayerColor(currentColorIndex);
        }
    }

    public void PreviousColor()
    {
        if (currentColorIndex > 0)
        {
            currentColorIndex--;
            PlayerPrefs.SetInt("currentColorIndex", currentColorIndex);
            currentColorImage.color = playerColors[currentColorIndex].color;
            currentColorText.text = playerColors[currentColorIndex].name;
            localPlayerController.CmdSendPlayerColor(currentColorIndex);
        }
    }
}
