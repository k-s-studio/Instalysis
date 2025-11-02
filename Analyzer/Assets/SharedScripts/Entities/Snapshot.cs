using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using Assets.DataTable2col;

[Serializable]
[Tablizable(typeof(Snapshot))]
public class Snapshot {
    [JsonProperty("id")] public string Id { get; private set; }
    [JsonProperty("date")] public DateTime Date { get; set; }
    [JsonProperty("followers")] public List<UserRef> Followers { get; } = new List<UserRef>();
    [JsonProperty("followings")] public List<UserRef> Followings { get; } = new List<UserRef>();

    public Snapshot(string id) {
        Id = id;
        Followers = new List<UserRef>();
        Followings = new List<UserRef>();
    }

    //public bool Save(string name = null) => SLUtility.Save(this, name);
    //這樣就跟寫Extension一樣耶
    //不過又總覺得怪怪的，明明是Entity突然有功能(超出此資料類別的功能)

    [Serializable]
    public class UserRef {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("tag")] public string Tag { get; set; }

        public UserRef(string id, string tag) {
            Id = id;
            Tag = tag;
        }
    }
}