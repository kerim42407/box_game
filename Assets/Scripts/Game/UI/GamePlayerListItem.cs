using Steamworks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayerListItem : MonoBehaviour
{
    public string playerName;
    //public int connectionID;
    public ulong playerSteamID;
    private bool avatarReceived;

    public Image background;
    public TextMeshProUGUI playerNameText;
    public RawImage playerIcon;
    public Image playerTurnIcon;
    public TextMeshProUGUI playerMoneyText;

    protected Callback<AvatarImageLoaded_t> imageLoaded;

    // Start is called before the first frame update
    void Start()
    {
        imageLoaded = Callback<AvatarImageLoaded_t>.Create(OnImageLoaded);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetPlayerValues()
    {
        playerNameText.text = playerName;
        //ChangeReadyStatus();
        if (!avatarReceived)
        {
            GetPlayerIcon();
        }

    }

    private void OnImageLoaded(AvatarImageLoaded_t callback)
    {
        if (callback.m_steamID.m_SteamID == playerSteamID)
        {
            playerIcon.texture = GetSteamImageAsTexture(callback.m_iImage);
        }
        else
        {
            return;
        }
    }

    void GetPlayerIcon()
    {
        int imageID = SteamFriends.GetLargeFriendAvatar((CSteamID)playerSteamID);
        if (imageID == -1)
        {
            return;
        }
        playerIcon.texture = GetSteamImageAsTexture(imageID);
    }

    private Texture2D GetSteamImageAsTexture(int iImage)
    {
        Texture2D texture = null;

        bool isValid = SteamUtils.GetImageSize(iImage, out uint width, out uint height);
        if (isValid)
        {
            byte[] image = new byte[width * height * 4];

            isValid = SteamUtils.GetImageRGBA(iImage, image, (int)(width * height * 4));

            if (isValid)
            {
                texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
                texture.LoadRawTextureData(image);
                texture.Apply();
            }
        }
        avatarReceived = true;
        return texture;
    }
}
