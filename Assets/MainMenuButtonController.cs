using UnityEngine;

public class MainMenuButtonController : MonoBehaviour
{
    public GameObject cogwheelIcon;
    //public Texture2D cursorTexture;
    //public CursorMode cursorMode = CursorMode.Auto;
    //public Vector2 hotSpot = Vector2.zero;
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        //Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
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
