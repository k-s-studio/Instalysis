using UnityEngine;
using UnityEditor;
using System;
using Newtonsoft.Json;

public class DataProcessing {
    public static Snapshot ss;

    [MenuItem("CustomTools/DataProcessing/Save")]
    public static void Save() {
        if (ss.Save()) Debug.Log($"TestData is saved to {App.DATAPATH}");
    }

    [MenuItem("CustomTools/DataProcessing/Log")]
    public static void Log() =>
        Debug.Log($"Id: {ss.Id}, Date: {ss.Date}, Followings= {ss.Followings.Count}, Followers= {ss.Followers.Count}, SerializeResult= {JsonConvert.SerializeObject(ss, App.SERIALIZERSETTING)}");

    [MenuItem("CustomTools/DataProcessing/New")]
    public static void New() {
        ss = new Snapshot("aaa") {
            Followings = { new Snapshot.UserRef("bbb", "友人B"), new Snapshot.UserRef("ccc", "友人C") },
            Followers = { new Snapshot.UserRef("bbb", "友人B"), new Snapshot.UserRef("ccc", "友人C") }
        };
        ss.Date = DateTime.Now;
    }

    [MenuItem("CustomTools/DataProcessing/AddDate")]
    public static void AddDate() => ss.Date =DateTime.Now;

    [MenuItem("CustomTools/DataProcessing/Deserialize")]
    public static void Deserialize() {
        ss = JsonConvert.DeserializeObject<Snapshot>(
            "{\"id\":\"aaa\",\"followers\":[{\"id\":\"bbb\",\"tag\":\"友人B\"},{\"id\":\"ccc\",\"tag\":\"友人C\"}],\"followings\":[{\"id\":\"bbb\",\"tag\":\"友人B\"},{\"id\":\"ccc\",\"tag\":\"友人C\"}]}",
            App.SERIALIZERSETTING
        );
    }

    public static void Clear() => ss = null;
}