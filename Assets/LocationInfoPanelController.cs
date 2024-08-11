using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LocationInfoPanelController : MonoBehaviour
{
    private GameObject mainCamera;
    [Header("References")]
    public TextMeshProUGUI locationNameText;
    public TextMeshProUGUI productivityText;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        //transform.LookAt(mainCamera.transform.position);
    }
}
