using Mirror;
using UnityEngine;

public class PlayerInputController : NetworkBehaviour
{
    public NetworkIdentity identity;
    public DiceThrower diceThrower;

    private PlayerObjectController localPlayerController;
    private Camera mainCamera;

    public bool canThrow;


    private void Awake()
    {
        identity = GetComponent<NetworkIdentity>();
        localPlayerController = GetComponent<PlayerObjectController>();

    }

    private void OnEnable()
    {
        GetReferences();
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (isLocalPlayer && diceThrower && localPlayerController.canPlay)
        {
            if (Input.GetKeyDown(KeyCode.Space) && canThrow)
            {
                canThrow = false;
                CmdRollDice();
            }
        }
    }

    [Command]
    private void CmdRollDice()
    {
        diceThrower.RollDice();
    }

    private void GetReferences()
    {
        if (GameObject.Find("Game Manager"))
        {
            GameObject gameManager = GameObject.Find("Game Manager");
            diceThrower = gameManager.GetComponent<DiceThrower>();
            mainCamera = Camera.main;
        }
        else
        {
            Invoke(nameof(GetReferences), 1f);
        }
    }
}
