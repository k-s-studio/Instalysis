using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.EventSystems;

[ExecuteAlways]
[DisallowMultipleComponent]
public class ExampleClass : UIBehaviour {
    public Rect windowRect = new Rect(20, 20, Screen.width - 40, Screen.height - 40);
    protected override void Awake() {
        useGUILayout = true;
        Debug.Log($"{Screen.width} x {Screen.height}");
    }
    void OnGUI()
    {
        // Register the window. Notice the 3rd parameter
        windowRect = GUI.Window(0, windowRect, DoMyWindow, "My Window");
    }

    // Make the contents of the window
    void DoMyWindow(int windowID)
    {
        if (GUI.Button(new Rect(10, 20, windowRect.width - 20, 50), "Hello World")) {
            print("Got a click");
            useGUILayout = false;
        }
    }
}
