using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovementController : NetworkBehaviour
{
    public float speed = 0.1f;
    public GameObject playerModel;

    // Cosmetics
    public MeshRenderer playerMesh;
    public Material[] playerColors;

    public NetworkIdentity identity;

    private PlaygroundController playgroundController;

    private void Awake()
    {
        identity = GetComponent<NetworkIdentity>();
    }

    private void OnEnable()
    {
        GetReferences();    
    }

    private void Start()
    {
        playerModel.SetActive(false);
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "Game")
        {
            if (playerModel.activeSelf == false)
            {
                playerModel.SetActive(true);
                PlayerCosmeticsSetup();
            }
            else
            {

            }
            if (identity.isOwned)
            {
                Movement();
            }
            else
            {

            }
        }
    }

    public void SetStartPosition()
    {
        transform.position = playgroundController.startPoint.transform.position;
    }

    public void Movement()
    {
        float xDirection = Input.GetAxis("Horizontal");
        float zDirection = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(xDirection, 0.0f, zDirection);

        transform.position += moveDirection * speed;
    }

    // Cosmetics
    public void PlayerCosmeticsSetup()
    {
        playerMesh.material.color = GetComponent<PlayerObjectController>().playerColor;
    }

    private void GetReferences()
    {
        if (GameObject.Find("Game Manager"))
        {
            playgroundController = GameObject.Find("Game Manager").GetComponent<GameManager>().playgroundController;
            SetStartPosition();
        }
        else
        {
            Invoke(nameof(GetReferences), 1f);
        }
    }
}
