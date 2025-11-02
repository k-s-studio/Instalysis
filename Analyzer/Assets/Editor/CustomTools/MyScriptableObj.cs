using UnityEngine;
using UnityEditor;
using System;

[CreateAssetMenu(fileName = "NewScriptableObj", menuName = "ScriptableObj")]
[Serializable]
public class MyScriptableObj : ScriptableObject {
    public int param1;
    public string param2;
}

public class MyScriptableObjTest {
    private readonly static MyScriptableObj myScriptableObj;

    [MenuItem("CustomTools/MyScriptableObj/Param1")] public static void GetParam1() => Debug.Log(myScriptableObj.param1);
    [MenuItem("CustomTools/MyScriptableObj/Param2")] public static void GetParam2() => Debug.Log(myScriptableObj.param2);
}
