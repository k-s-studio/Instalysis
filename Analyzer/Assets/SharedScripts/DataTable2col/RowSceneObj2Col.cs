using System;
using System.Linq.Expressions;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RowSceneObj2Col : MonoBehaviour {
    private static RowSceneObj2Col m_template = null;
    public Text NameText, DataText;
    public Image NameCell, DataCell;
    public HorizontalLayoutGroup HLayout;
    public /*volatile*/ static Action<RowSceneObj2Col> Config; //Table會初始化這個

    RowSceneObj2Col Init() {
        HLayout = gameObject.AddComponent<HorizontalLayoutGroup>();
        NameCell = this.NewChild("NameCell").AddComponent<Image>();
        DataCell = this.NewChild("DataCell").AddComponent<Image>();
        NameText = NameCell.NewChild("Text").AddComponent<Text>();
        DataText = DataCell.NewChild("Text").AddComponent<Text>();

        Config?.Invoke(this);
        return this;
        //Init()裡我就只想做Init，能不能New同時有set template又有回傳?
    }
    // RowSceneObj2Col Init(Action<RowSceneObj2Col> config) {
    //     if (config == null) {
    //         return Init();
    //     }
    //     HLayout = gameObject.AddComponent<HorizontalLayoutGroup>();
    //     NameCell = this.NewChild("NameCell").AddComponent<Image>();
    //     DataCell = this.NewChild("DataCell").AddComponent<Image>();
    //     NameText = NameCell.NewChild("Text").AddComponent<Text>();
    //     DataText = DataCell.NewChild("Text").AddComponent<Text>();
    //     config?.Invoke(this);
    //     return this;
    // }
    public static RowSceneObj2Col New() {
        if (m_template == null) {
            //Debug.Log("new template."); //有set到欸，好玩
            return m_template = new GameObject("Row").AddComponent<RowSceneObj2Col>().Init();
        }
        else return Instantiate(m_template);
    }
    void Awake() => useGUILayout = false;
}