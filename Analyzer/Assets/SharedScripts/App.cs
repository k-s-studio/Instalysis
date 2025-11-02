using Newtonsoft.Json;
using UnityEngine;
using static System.IO.Path;

public class App {
    public static readonly string NAME = Application.productName;
    public static readonly string DATAPATH = Combine(Application.streamingAssetsPath, "Database");
    //Application.persistentDataPath;
    //Application.datapath + "/Database"
    //依平台而定(例如Android上無法寫入streamingAssetsPath，須為persistentDataPath)，待測試windows
    public static readonly JsonSerializerSettings SERIALIZERSETTING = new() { NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore };
}
