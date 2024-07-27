//using UnityEngine;
//using UnityEditor;

//public class EditModeFunctions : EditorWindow
//{
//    [MenuItem("Window/Edit Mode Functions")]
//    public static void ShowWindow()
//    {
//        GetWindow<EditModeFunctions>("Edit Mode Functions");
//    }

//    private void OnGUI()
//    {
//        if (GUILayout.Button("Sell Owned Locations"))
//        {
//            FunctionToRun();
//        }
//    }

//    private void FunctionToRun()
//    {
//        GameObject.Find("LocalGamePlayer").GetComponent<PlayerMoveController>().SellOwnedLocations(200000);
//    }
//}
