using Mirror;
using UnityEngine;

public class MouseClickController : MonoBehaviour
{
    public Camera mainCamera;
    private PlayerObjectController localPlayerObjectController;
    // Start is called before the first frame update
    void Start()
    {
        GetReferences();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Input.mousePosition;

            Ray mRay = mainCamera.ScreenPointToRay(mousePosition);

            RaycastHit raycastHit;

            bool weHitSomething = Physics.Raycast(mRay, out raycastHit);

            if (weHitSomething)
            {
                NetworkConnectionToClient target = localPlayerObjectController.Manager.gamePlayers[localPlayerObjectController.gameManager.turnIndex].connectionToClient;
                if (!localPlayerObjectController.locationsToBeSold.Contains(raycastHit.transform.GetComponent<LocationController>()))
                {
                    localPlayerObjectController.locationsToBeSold.Add(raycastHit.transform.GetComponent<LocationController>());
                    //localPlayerObjectController.GetComponent<PlayerMoveController>().SellOwnedLocations(target,localPlayerObjectController.playgroundController.locations[localPlayerObjectController.playerLocation].GetComponent<LocationController>().rentRate);
                    raycastHit.transform.GetComponent<LocationController>().sellLocationToggle.GetComponent<MeshRenderer>().material.color = Color.green;
                }
                else
                {
                    localPlayerObjectController.locationsToBeSold.Remove(raycastHit.transform.GetComponent<LocationController>());
                    //localPlayerObjectController.GetComponent<PlayerMoveController>().SellOwnedLocations(target, localPlayerObjectController.playgroundController.locations[localPlayerObjectController.playerLocation].GetComponent<LocationController>().rentRate);
                    raycastHit.transform.GetComponent<LocationController>().sellLocationToggle.GetComponent<MeshRenderer>().material.color = Color.white;
                }

                Debug.Log(raycastHit.transform.name);
            }
            else
            {
                Debug.Log("We don't hit anything");
            }
        }
    }

    private void GetReferences()
    {
        if (GameObject.Find("LocalGamePlayer"))
        {
            localPlayerObjectController = GameObject.Find("LocalGamePlayer").GetComponent<PlayerObjectController>();
        }
        else
        {
            Invoke(nameof(GetReferences), 1f);
        }
    }
}
