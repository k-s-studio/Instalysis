using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine.UIElements;
using static DataTableUtility;

#region DataTable2ColEditor
[CustomEditor(typeof(TransformTester))]
public class TransformTesterEditor : Editor {
    TransformTester Target;
    public override VisualElement CreateInspectorGUI() {
        Target = target as TransformTester;
        return base.CreateInspectorGUI();
    }
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        if (GUILayout.Button("Rect")) Target.ShowRect();
    }
}
#endregion DataTable2ColEditor