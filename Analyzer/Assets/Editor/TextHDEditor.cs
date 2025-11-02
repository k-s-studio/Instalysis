using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Assets.TextHD.V1;
using System;

[CustomEditor(typeof(TextHD))]
public class TextHDEditor : Editor {
    private TextHD m_Target;
    private SerializedProperty m_Text;
    private readonly Action space = () => GUILayout.Space(12);
    public override VisualElement CreateInspectorGUI() {
        m_Target = target as TextHD;
        m_Text = serializedObject.FindProperty("m_Text");
        return base.CreateInspectorGUI();
    }
    public override void OnInspectorGUI() {
        if (m_Target.NoBindingText) {
            EditorGUILayout.PropertyField(m_Text, new GUIContent("Text"));

            space();
            EditorGUILayout.HelpBox(
                "The referenced Text component is missing due to 'unbind' button, GameObject mis-deleted or some IDE error. Select Text in scene to Rebind it."
            , MessageType.Info);

            serializedObject.ApplyModifiedProperties();
            //使用 SerializedProperty 來修改屬性的話，結尾須調用 ApplyModifiedProperties()。
        }
        else {
            DrawDefaultInspector();

            space();
            if (GUILayout.Button("Restore")) m_Target.Restore();
            if (GUILayout.Button("Unbind")) m_Target.Unbind();
        }
    }
}