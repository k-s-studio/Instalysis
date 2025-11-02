using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine.UIElements;
using static DataTableUtility;
using Assets.DataTable2col;

#region DataTable2ColEditor
[CustomEditor(typeof(DataTable2Col))]
public class DataTable2ColEditor : Editor {
    private readonly Type TABLIZABLE_ATTR = typeof(TablizableAttribute);
    private DataTable2Col Target;
    private TypeNameSyncSelector e_select; //對全域變數的struct操作不用值複製，但是宣告區域變數var e = e_select;就會是複本。
    public static Type[] ListingTypes { get; private set; } = null; //用於給其他class取得的

    public override VisualElement CreateInspectorGUI() {
        Target = target as DataTable2Col;

        // 獲取所有的 Entity 類型，然後選擇DataTable2Col對象已經選擇的選項
        e_select.SearchAsmblyByAttr("Assembly-CSharp", TABLIZABLE_ATTR).Select(Array.IndexOf(e_select.TypeList, Target.DataType));
        ListingTypes = e_select.TypeList;
        return base.CreateInspectorGUI();
    }

    public override void OnInspectorGUI() {
        // 顯示下拉選單以選擇 Entity 類型
        e_select.SelectedIndex = EditorGUILayout.Popup("Data Type", e_select.SelectedIndex, e_select.NameList);
        if (!(e_select.SelectedType == null || e_select.SelectedType == Target.DataType)) Target.DataType = e_select.SelectedType;
        base.OnInspectorGUI();
        if (Target.DataType == null) return;
        //if (GUILayout.Button("Check")) ; //Target.ValueCheckment();
        if (GUILayout.Button("Resize")) Target.UpdateTableSize();
        if (GUILayout.Button("Clear")) Target.DestroyAllChild();
        if (GUILayout.Button("Apply")) Target.ApplyTable();
    }
}
#endregion DataTable2ColEditor
#region RowInfoDrawerUIE
[CustomPropertyDrawer(typeof(RowInfo))]
public class RowInfoDrawerUIE : PropertyDrawer {
    private readonly DataTable2Col table = Selection.activeGameObject.GetComponent<DataTable2Col>();
    private Dictionary<Type, (MemberInfo[] infos, string[] names)> membersOf = null;

    //初始執行1s，滑鼠進入Inspector範圍則持續執行
    //RowDrawerUIE只有唯一實例，對Inspector顯示每一個Row執行一次OnGUI()
    //而且每幀丟進來的property.GetHashCode()都不一樣，這是怎麼cache?
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        if (table == null || DataTable2ColEditor.ListingTypes == null) return;
        if (membersOf == null) membersOf = DataTable2ColEditor.ListingTypes.ToDictionary(t => t, t => GetMembersAndNames(t)); //Dictionary不會被序列化，每次開Inspector都執行一次ToDictionary()?
        var _nameStringProperty = property.FindPropertyRelative("Name");
        var _infoStringProperty = property.FindPropertyRelative("m_infoString"); //若為null代表邏輯有問題，需報錯，所以後面call without null checkment
        var _selectedMemInfo = GetMemberInfo(_infoStringProperty.stringValue); //未選擇為null
        var _selectedIndex = _selectedMemInfo == null ? -1 : Array.IndexOf(membersOf[_selectedMemInfo.ReflectedType].infos, _selectedMemInfo);
        var _dropdownType = _selectedMemInfo?.ReflectedType ?? table.DataType; //Row未選擇 && Table未選擇 => null
        var _dropdownOptions = _dropdownType == null ? Array.Empty<string>() : membersOf[_dropdownType].names;

        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        var colRects = ColRects(position, new float[2] { 0.33f, 0.66f }, 1.5f);
        //var colRects = ColRects(position, new float[3] { 0.33f, 0.44f, 0.22f }, 1.5f);
        EditorGUI.DelayedTextField(colRects[0], _nameStringProperty, GUIContent.none);
        int new_i = EditorGUI.Popup(colRects[1], _selectedIndex, _dropdownOptions);
        if (new_i != _selectedIndex && new_i >= 0) _infoStringProperty.stringValue = GetMemberInfoString(membersOf[_dropdownType].infos[new_i]);
        //if (GUI.Button(colRects[2], "btn")) Debug.Log(table.DataType.FullName);

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }
    private (MemberInfo[], string[]) GetMembersAndNames(Type t) {
        if (t == null) return (Array.Empty<MemberInfo>(), Array.Empty<string>());
        var memberInfos = t.GetMembers().Where(info => info is PropertyInfo || info is FieldInfo).ToArray();
        var names = memberInfos.Select(_memInfo => $"{_memInfo.ReflectedType.Name}.{_memInfo.Name}").ToArray();
        return (memberInfos, names);
    }

    private Rect[] ColRects(Rect position, float[] widthAllocation, float padding) {
        Rect[] rects = new Rect[widthAllocation.Length];
        float rectX = position.x;
        for (int i = 0; i < widthAllocation.Length; i++) {
            rects[i] = new Rect() {
                x = rectX,
                y = position.y + padding,
                width = position.width * widthAllocation[i] - padding * 2,
                height = position.height - padding * 2
            };
            rectX += rects[i].width + padding * 2;
        }
        return rects;
    }
}
#endregion RowDrawerUIE
#region TypeNameSyncSelector
/// <summary>
/// 包含 Type[] Types 與映射名稱的string[] Names。
/// index = 0 ~ 127 且不超過 Types.length，不合規則時指派為-1。
/// 取得陣列若為null則回傳空陣列，取得元素若不存在則回傳null。
/// </summary>
public struct TypeNameSyncSelector {
    private string[] _names;
    private Type[] _types;
    private sbyte _selectedIndex;
    public void Set(Type[] types) { //同建構子的動作，不過用指派會比較省效能
        _types = types ?? Array.Empty<Type>();
        _names = _types.Select(t => t.Name).ToArray();
        _selectedIndex = -1;
    }
    public (int index, Type type, string name) Select(int i) {
        if (i < 0 || i >= _names?.Length || i > sbyte.MaxValue) _selectedIndex = -1;
        else _selectedIndex = (sbyte)i;
        return Selected;
    }
    public (int index, Type type, string name) Select(string typeName) => Select(Array.IndexOf(_names, typeName));
    public (int index, Type type, string name) Select(Type type) => Select(Array.IndexOf(_types, type));
    public Type[] TypeList {
        set => Set(value);
        readonly get => _types ?? Array.Empty<Type>();
    }
    public readonly string[] NameList => _names ?? Array.Empty<string>();
    public readonly (int index, Type type, string name) Selected => (SelectedIndex, SelectedType, SelectedName);
    public int SelectedIndex { set => Select(value); readonly get => _selectedIndex; }
    public Type SelectedType { set => Select(value); readonly get => _types?.ElementAtOrDefault(_selectedIndex); }
    public string SelectedName { set => Select(value); readonly get => _names?.ElementAtOrDefault(_selectedIndex); }
    // public static TypeNameSyncSelector Init(Type[] types) {
    //     Debug.Log("Init.");
    //     return new(types);
    // } 
}

public static class TypeNameSyncSelectorExtensions {
    ///<summary>
    /// 寫struct的擴充方法時傳遞this param時必加"ref"，否則以深複製傳值，對param做修改、param呼叫自我初始化方法都不影響原本的實例，只能把修改過的副本回傳。
    /// </summary>
    public static ref TypeNameSyncSelector SearchAsmblyByAttr(this ref TypeNameSyncSelector es, string assemblyString, Type attrType) {
        var value = AppDomain.CurrentDomain.Load(assemblyString)
            .GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.CustomAttributes.Any(attr => attr.AttributeType == attrType))
            .ToArray();
        es.Set(value);
        return ref es;
        //如果要玩func串，回傳值也要是ref struct不然傳出去的改變了也改不到原本實例
    }
}
#endregion TypeNameSyncSelector

[CustomEditor(typeof(RowSceneObj2Col))]
public class RowSceneObjEditor : Editor {
    public override void OnInspectorGUI() {
        GUI.enabled = false;
        base.OnInspectorGUI();
        GUI.enabled = true;
    }
}