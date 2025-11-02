using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEditor.Rendering;
using System.Linq;
using System;
using Newtonsoft.Json;
public class SLUtility {
    public static bool Save(string directory, string name, string content, bool createDir = true) {
        if (Path.GetInvalidFileNameChars().Any(c => name.Contains(c))) {
            Debug.LogWarning($"Failed to save. Invalid file name:{name}");
            return false;
        }
        if (Path.GetInvalidPathChars().Any(c => directory.Contains(c))) {
            Debug.LogWarning($"Failed to save. Invalid directory:{directory}");
            return false;
        }
        if (!Directory.Exists(directory)) {
            if (createDir)
                Directory.CreateDirectory(directory);
            else {
                Debug.LogWarning($"Failed to save. Directory unexists:{directory}");
                return false;
            }
        }
        try {
            File.WriteAllText(Path.Combine(directory, name), content);
            //如果資料夾不存在也不會自己創，笨==
            Debug.Log($"Successfully saved to {Path.Combine(directory, name)}");
            return true;
        }
        catch (Exception e) {
            Debug.LogError(e);
            return false;
        }
    }
    //無論誰存、什麼時候存、存什麼、存去哪，這裡只處理｢怎麼存｣
    //寫基本、寫穩(嚴謹)。

    // public static void Save(string path, string content, bool createDir = true) =>
    //     Save(Path.GetDirectoryName(path), Path.GetFileName(path), content, createDir);
    // 沒必要寫這條，很少有機會是完全由自己指定URI(又不是web)都是Application.xxxpath頂多多一層子目錄。畢竟Unity打包後根據平台，資源可能不用電腦路徑的形式存取，保證能行的目錄非常有限。
    // 既然都需要剪接路徑，檔案名稱和指定目錄分開參數也不用避諱吧？
    // 而且需要完全客製化指定路徑的場合目前幾乎想不到，算是很偏門了，用Path類別切分目錄和檔名也可以接受，畢竟是極端狀況。
    // 不可能為了測試多包一個根本用不到的功能==

    public static bool Save(Snapshot ss, string name = null) =>
        Save(App.DATAPATH, (name ?? DateTime.Now.ToString("yyyyMMddHHmmssfff")) + ".ss", JsonConvert.SerializeObject(ss, App.SERIALIZERSETTING));
    //無論誰存、何時存，處理「存什麼、存去哪」case by case 

    public static string Load(string path, string name) {
        //string path = "Assets/Resources/test.txt";
        StreamReader reader = new(path + "/" + name);
        string content = reader.ReadToEnd();
        reader.Close();
        return content;
    }

    public static List<Snapshot> LoadSS(string path) {
        List<Snapshot> snapshots = new();
        foreach (string p in Directory.GetFiles(path)) {
            if (p.EndsWith(".ss")) {
                snapshots.Add(JsonUtility.FromJson<Snapshot>(Load(p)));
            }
        }
        return snapshots;
    }
    public static string Load(string name) => Load(App.DATAPATH, name);
    //public static Snapshot Load(string path, string name) => JsonUtility.FromJson<Snapshot>(Load(string path, string name));
}