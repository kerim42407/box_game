using Mirror;
using UnityEngine;

public class PlayerInputController : NetworkBehaviour
{
    public NetworkIdentity identity;
    public DiceThrower diceThrower;

    private PlayerObjectController localPlayerController;

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
            diceThrower = GameObject.Find("Game Manager").GetComponent<DiceThrower>();
        }
        else
        {
            Invoke(nameof(GetReferences), 1f);
        }
    }
}
