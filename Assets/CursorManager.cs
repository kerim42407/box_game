using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public Texture2D normalCursorTexture;
    public Vector2 normalCursorHotspot;

    [Space(5)]
    public Texture2D hoverCursorTexture;
    public Vector2 hoverCursorHotspot;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnButtonCursorEnter()
    {
        Cursor.SetCursor(hoverCursorTexture, hoverCursorHotspot, CursorMode.Auto);
    }
    public void OnButtonCursorExit()
    {
        Cursor.SetCursor(normalCursorTexture, normalCursorHotspot, CursorMode.Auto);
    }
}
