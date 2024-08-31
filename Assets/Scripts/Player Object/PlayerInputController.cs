using Mirror;
using UnityEngine;

public class PlayerInputController : NetworkBehaviour
{
    #region Fields and Properties
    [HideInInspector]public NetworkIdentity identity;
    [HideInInspector]public DiceThrower diceThrower;
    private PlayerObjectController localPlayerController;
    public bool canThrow;
    #endregion

    #region Methods

    private void Awake()
    {
        identity = GetComponent<NetworkIdentity>();
        localPlayerController = GetComponent<PlayerObjectController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isLocalPlayer && diceThrower && localPlayerController.canPlay)
        {
            if (Input.GetKeyDown(KeyCode.Space) && canThrow)
            {
                canThrow = false;
                UIManager.Instance.yourTurnNotification.Close();
                GameManager.Instance.CmdRollDice();
            }
        }
    }

    private void OnEnable()
    {
        GetReferences();
    }

    private void GetReferences()
    {
        if (GameObject.Find("Game Manager"))
        {
            GameObject gameManager = GameObject.Find("Game Manager");
            diceThrower = gameManager.GetComponent<DiceThrower>();
        }
        else
        {
            Invoke(nameof(GetReferences), 1f);
        }
    }

    #endregion
}
