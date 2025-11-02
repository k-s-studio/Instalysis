using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
public class MyMenu {
    [MenuItem("CustomTools/Hello")]
    public static void Hello() => Debug.Log("Hello World");

    [MenuItem("CustomTools/ShowTime")]
    public static void ShowTime() {
        DateTime dateTime = DateTime.Now;
        Debug.Log(dateTime.ToString());
    }

    [MenuItem("CustomTools/V2asObj")]
    public static void V2asObj() {
        Vector2 v2 = new(0, 0);
        object o = v2;
        Debug.Log(o.GetType());
    }
    [MenuItem("CustomTools/TypeInfo/ABCTest")]
    public static void ABCTest() {
        A a = new();
        B b = new();
        C c = new();
        Debug.Log($"a: GetType()={a.GetType().Name}, (a is A)={a is A}, (a is C)={a is C}");
        Debug.Log($"b: GetType()={b.GetType().Name}, (b is A)={b is A}, (b is C)={b is C}");
        Debug.Log($"c: GetType()={c.GetType().Name}, (c is A)={c is A}, (c is C)={c is C}");
        A ac = c as A;
        C c_null = null;
        Debug.Log($"ac: GetType()={ac.GetType().Name}, (ac is A)={ac is A}, (ac is C)={ac is C}");
        //Debug.Log($"c_null: GetType()={c_null.GetType()}"); //不能對null GetType()
        Debug.Log($"c_null: (c_null is A)={c_null is A}, (c_null is C)={c_null is C}"); //null就是null，不是任何型別 
    }

    class A { }
    class B : A { }
    class C : B { }

    [MenuItem("CustomTools/TypeInfo/FilterAttribute")]
    public static void TypeInfo_FilterAttribute() {
        Debug.Log($"FilterAttribute = {Type.FilterAttribute}");
    }

    [MenuItem("CustomTools/TypeInfo/MonoBehaviour")]
    public static void TypeInfo_MonoBehaviour() {
        Type ttt = typeof(MonoBehaviour);
        //foreach(FieldInfo _fInfo in ttt.GetFields()) Debug.Log($"{_fInfo.Name} = {_fInfo.GetValue(ttt)}");
        Debug.Log($"Namespace = {ttt.Namespace}");
        Debug.Log($"FullName = {ttt.FullName}");
        Debug.Log($"Assembly = {ttt.Assembly.FullName}");
        Debug.Log($"CustomAttributes = {ttt.CustomAttributes}");
        Debug.Log($"CustomAttributes = {ttt.CustomAttributes.ToArray().Length}");
        foreach (CustomAttributeData t in ttt.CustomAttributes.ToList()) Debug.Log(t.AttributeType.Name);
    }

    [MenuItem("CustomTools/TypeInfo/SnapShot")]
    public static void TypeInfo_Snapshot() {
        Type ttt = typeof(Snapshot);
        //foreach(FieldInfo _fInfo in ttt.GetFields()) Debug.Log($"{_fInfo.Name} = {_fInfo.GetValue(ttt)}");
        Debug.Log($"Namespace = {ttt.Namespace}");
        Debug.Log($"FullName = {ttt.FullName}");
        Debug.Log($"Assembly = {ttt.Assembly.FullName}");
        Debug.Log($"CustomAttributes = {ttt.CustomAttributes}");
        Debug.Log($"CustomAttributes = {ttt.CustomAttributes.ToArray().Length}");
        foreach (CustomAttributeData t in ttt.CustomAttributes.ToList()) Debug.Log(t.AttributeType.Name);
    }

    [MenuItem("CustomTools/TypeInfo/GetAssembliesInfo")]
    public static void GetAssembliesInfo() {
        foreach (Assembly ass in AppDomain.CurrentDomain.GetAssemblies()) {
            Debug.Log($"{ass.GetName()} has {ass.GetTypes().Count()} types.");
        }
    }

    [MenuItem("CustomTools/TypeInfo/EqualsTest")]
    public static void EqualsTest() {
        object objA = a;
        object objB = b;
        object objC = c;
        Debug.Log($"(a==b)={a == b}, a.Equals(b)={a.Equals(b)} ,(a.GetHashCode() == b.GetHashCode())={a.GetHashCode() == b.GetHashCode()}");
        Debug.Log($"(c==b)={c == b}, c.Equals(b)={c.Equals(b)} ,(c.GetHashCode() == b.GetHashCode())={c.GetHashCode() == b.GetHashCode()}");
    }
    readonly static Type a = typeof(Snapshot);
    readonly static Type b = typeof(Snapshot);
    readonly static Type c = typeof(App);
}