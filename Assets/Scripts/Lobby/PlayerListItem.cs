using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListItem : MonoBehaviour
{
    public string playerName;
    public int connectionID;
    public ulong playerSteamID;
    private bool avatarReceived;

    public GameObject playerItemParent;
    public GameObject inviteButton;
    public TextMeshProUGUI playerNameText;
    public RawImage playerIcon;
    public TextMeshProUGUI playerReadyText;
    public GameObject hostIcon;
    public Image playerColorImage;
    public GameObject playerColorChangeButton;
    public GameObject previousPawnButton;
    public GameObject nextPawnButton;
    public Image playerPawnImage;

    public bool ready;

    protected Callback<AvatarImageLoaded_t> imageLoaded;

    // Start is called before the first frame update
    void Start()
    {
        imageLoaded = Callback<AvatarImageLoaded_t>.Create(OnImageLoaded);
    }

    public void ChangeReadyStatus()
    {
        if (ready)
        {
            playerReadyText.text = "Ready";
            playerReadyText.color = Color.green;
        }
        else
        {
            playerReadyText.text = "Not Ready";
            playerReadyText.color = Color.red;
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

    public void SetPlayerValues()
    {
        playerNameText.text = playerName;
        ChangeReadyStatus();
        GetPlayerIcon();
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
