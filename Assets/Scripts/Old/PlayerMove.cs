using Mirror;
using TMPro;
using UnityEngine;

public class PlayerMove : NetworkBehaviour
{
    public DiceThrower diceThrower;

    private void Awake()
    {
        diceThrower = GameObject.Find("Game Manager").GetComponent<DiceThrower>();
    }

    private void Start()
    {

    }

    private void Update()
    {
        if (isLocalPlayer)
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            Vector3 playerMovement = new Vector3(h * 0.25f, v * 0.25f, 0);

            transform.position = transform.position + playerMovement;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                CmdRollDice();
            }
        }


    }

    [Command]
    private void CmdRollDice()
    {
        diceThrower.RollDice();
    }

    private void OnDestroy()
    {

    }

}
