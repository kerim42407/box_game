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
        if (isLocalPlayer && localPlayerController.canSell && Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Input.mousePosition;

            Ray mRay = mainCamera.ScreenPointToRay(mousePosition);

            RaycastHit raycastHit;

            bool weHitSomething = Physics.Raycast(mRay, out raycastHit);

            if (weHitSomething && raycastHit.transform.CompareTag("Saleable Location"))
            {
                if (!localPlayerController.locationsToBeSold.Contains(raycastHit.transform.GetComponent<LocationController>()))
                {
                    localPlayerController.locationsToBeSold.Add(raycastHit.transform.GetComponent<LocationController>());
                    raycastHit.transform.GetComponent<LocationController>().sellLocationToggle.GetComponent<MeshRenderer>().material.color = Color.green;
                    localPlayerController.playerMoveController.SetSellLocationsPanelButtonData();
                }
                else
                {
                    localPlayerController.locationsToBeSold.Remove(raycastHit.transform.GetComponent<LocationController>());
                    raycastHit.transform.GetComponent<LocationController>().sellLocationToggle.GetComponent<MeshRenderer>().material.color = Color.white;
                    localPlayerController.playerMoveController.SetSellLocationsPanelButtonData();
                }
            }
            else
            {

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
