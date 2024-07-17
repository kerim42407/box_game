using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerMoveController : NetworkBehaviour
{
    public GameObject playerModel;

    // Cosmetics
    public MeshRenderer playerMesh;
    public Material[] playerColors;

    [HideInInspector] public bool shouldMove;
    [HideInInspector] public int destinationIndex;
    [HideInInspector] public List<Transform> destinationTransforms;
    [HideInInspector] public Transform firstTransform;
    [HideInInspector] public float startTime;
    [HideInInspector] public float journeyTime = 1.0f;

    private PlayerObjectController playerObjectController;
    private PlaygroundController playgroundController;

    public bool didCosmetic;
    

    // Start is called before the first frame update
    void Start()
    {
        playerObjectController = GetComponent<PlayerObjectController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (shouldMove)
        {
            if (transform.position == destinationTransforms[destinationIndex].position)
            {
                if (destinationIndex == destinationTransforms.Count - 1)
                {
                    destinationIndex = 0;
                    shouldMove = false;
                    playerObjectController.gameManager.turnIndex++;
                }
                else
                {
                    firstTransform = destinationTransforms[destinationIndex].transform;
                    startTime = Time.time;
                    destinationIndex++;
                }
            }
            else
            {
                Vector3 center = (firstTransform.position + destinationTransforms[destinationIndex].position) * 0.5f;

                center -= new Vector3(0, 1, 0);

                Vector3 firstPosCenter = firstTransform.position - center;
                Vector3 lastPosCenter = destinationTransforms[destinationIndex].position - center;

                float fracComplete = (Time.time - startTime) / journeyTime;

                transform.position = Vector3.Slerp(firstPosCenter, lastPosCenter, fracComplete);
                transform.position += center;
            }
        }
    }

    public void MovePlayer(int locationIndex)
    {
        if (!playgroundController)
        {
            playgroundController = playerObjectController.playgroundController;
        }
        destinationTransforms = new List<Transform>();
        firstTransform = playgroundController.locations[playerObjectController.playerLocation].transform;
        for (int i = playerObjectController.playerLocation; i < playerObjectController.playerLocation + locationIndex; i++)
        {
            if(i >= 39)
            {
                destinationTransforms.Add(playgroundController.locations[(i + 1) - 40].transform);
            }
            else
            {
                destinationTransforms.Add(playgroundController.locations[i + 1].transform);
            }
        }
        if (playerObjectController.playerLocation + locationIndex >= playgroundController.locations.Count)
        {
            playerObjectController.playerLocation = (playerObjectController.playerLocation + locationIndex) - playgroundController.locations.Count;
        }
        else
        {
            playerObjectController.playerLocation = playerObjectController.playerLocation + locationIndex;
        }
        
        shouldMove = true;
        startTime = Time.time;
    }

    public void SetStartPosition()
    {
        if (!playgroundController)
        {
            playgroundController = playerObjectController.playgroundController;
        }
        transform.position = playgroundController.locations[0].transform.position;
        playerObjectController.playerLocation = 0;
    }

    public void PlayerCosmeticsSetup()
    {
        playerMesh.material = playerColors[GetComponent<PlayerObjectController>().playerColor];
    }
}
