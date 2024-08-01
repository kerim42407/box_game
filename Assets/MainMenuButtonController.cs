using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuButtonController : MonoBehaviour
{
    public GameObject cogwheelIcon;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetCogwheelIconPosition(Transform activeButton)
    {
        if (!cogwheelIcon.activeSelf)
        {
            cogwheelIcon.SetActive(true);
        }
        cogwheelIcon.transform.position = activeButton.position;
    }
}
