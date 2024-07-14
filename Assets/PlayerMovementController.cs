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

    private void Awake()
    {
        identity = GetComponent<NetworkIdentity>();
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
                SetPosition();
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

    public void SetPosition()
    {
        transform.position = new Vector3(Random.Range(-5, 5), 0.8f, Random.Range(-15, 7));
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
        playerMesh.material = playerColors[GetComponent<PlayerObjectController>().playerColor];
    }
}
