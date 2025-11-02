using UnityEngine;
using UnityEditor;
using System;

public class LogErrorTest {
    [MenuItem("CustomTools/LogErrorTest/LogError")]
    public static void LogError() {
        Debug.LogError($"This is {"LogError"} be like with {"string param"}.");
        Debug.LogError(new Exception($"This is {"LogError"} be like with {"exception"}.", new Exception("INNER EXCEPTION MSG", new Exception("INNER INNER EXCEPTION MSG"))));
    }

    [MenuItem("CustomTools/LogErrorTest/LogErrorFormat")]
    public static void LogErrorFormat() {
        Debug.LogErrorFormat("This is {0} be like with {1} and {2}.", "LogErrorFormat", "string", new Exception("EXCEPTION MSG", new Exception("INNER EXCEPTION MSG")));
    }

    [MenuItem("CustomTools/LogErrorTest/LogException")]
    public static void LogException() => Debug.LogException(new Exception("EXCEPTION MSG", new Exception("INNER EXCEPTION MSG", new Exception("INNER INNER EXCEPTION MSG"))));
}