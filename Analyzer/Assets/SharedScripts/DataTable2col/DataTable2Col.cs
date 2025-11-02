using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using static DataTableUtility;
using System.Linq;

namespace Assets.DataTable2col {
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(VerticalLayoutGroup), typeof(RectTransform))]
    public class DataTable2Col : UIBehaviour {
        #region DataTable2Col
        private RectTransform m_rectTransform = null;
        public RectTransform rectTransform {
            get {
                if (m_rectTransform == null) m_rectTransform = transform as RectTransform;
                return m_rectTransform;
            }
        }
        #region DataBinding 
        [SerializeField][HideInInspector] private string _dataTypeString = "";
        private Type _dataType = null;
        public Type DataType {
            get {
                if (_dataType == null) {
                    if (_dataTypeString == "") return null;
                    else _dataType = Type.GetType(_dataTypeString, false, false);
                }
                return _dataType;
            }
            set {
                _dataTypeString = value?.FullName ?? "";
                _dataType = null;
            }
        }
        public object DataInstance {
            set {
                if (value.GetType() == DataType) foreach (var r in m_Rows) r?.UpdateText(data: value);
                else Debugg.LogError(this, "Data rejected. (Wrong type)");
            }
        }
        /// <summary>
        /// 原本Row是struct時，從List取元素會拿到複製品，得assign回去。
        /// 其實還是List的機制問題，Unity實作Indexer時讓Row[i]回傳的是複本而不是變數參考。
        /// </summary>
        public List<RowInfo> Rows;
        #endregion DataBinding
        #region Styling
        const float init_x = 500, init_y = 300;
        [Range(8, 1000)] public float CellHeight = 80;
        [Range(0, 1)] public float WidthAllocation = 0.3f;
        [Range(0, 300)] public float VerticalSpacing = 6, HorizontalSpacing = 6;
        //TextConfig
        public Font NameFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"), DataFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        public FontStyle NameStyle = FontStyle.Bold, DataStyle = FontStyle.Normal;
        public int NameSize = 36, DataSize = 36;
        public TextAnchor NameAlign = TextAnchor.MiddleCenter, DataAlign = TextAnchor.MiddleLeft;
        public float NamePadding = 0, DataPadding = 0;
        //ColorConfig
        public Color NameColor = Color.white, NameBackgroundColor = Color.gray;
        public Color DataColor = Color.black, DataBackgroundColor = Color.white;
        //SpriteConfig
        public Sprite CellBg = Resources.GetBuiltinResource<Sprite>("unity_builtin_extra/UISprite");
        [SerializeField][HideInInspector] private VerticalLayoutGroup VLayout;
        static DataTable2Col() => RowSceneObj2Col.Config = (o) => {
            //靜態建構子會在第一次使用到類別時調用
            //TextConfigs
            o.NameText.alignment = TextAnchor.MiddleCenter;
            o.DataText.alignment = TextAnchor.MiddleLeft;
            // o.NameText.fontSize = o.DataText.fontSize = 300;
            // o.NameText.verticalOverflow = o.DataText.verticalOverflow = VerticalWrapMode.Overflow;
            // o.NameText.horizontalOverflow = o.DataText.horizontalOverflow = HorizontalWrapMode.Overflow;

            //ImageConfigs
            o.NameCell.type = o.DataCell.type = Image.Type.Sliced;

            //HLayoutConfig
            o.HLayout.childAlignment = TextAnchor.MiddleCenter;
            (o.HLayout.childControlWidth, o.HLayout.childControlHeight) = (false, true);
            (o.HLayout.childScaleWidth, o.HLayout.childScaleHeight) = (false, false);
            (o.HLayout.childForceExpandWidth, o.HLayout.childForceExpandHeight) = (false, true);
        };
        private Action<VerticalLayoutGroup> VLayoutConfig = (layout) => {
            layout.childAlignment = TextAnchor.MiddleCenter;
            (layout.childControlWidth, layout.childControlHeight) = (true, true);
            (layout.childScaleWidth, layout.childScaleHeight) = (false, false);
            (layout.childForceExpandWidth, layout.childForceExpandHeight) = (true, true);
        };
        #endregion Styling
        #region TableControl
        [SerializeField][HideInInspector] private List<Row> m_Rows = new();
        private bool m_DirtyFlag = false;
#if UNITY_EDITOR
        private sbyte m_LengCache = sbyte.MaxValue; //紀錄變動前的長度
        [SerializeField][HideInInspector] private bool m_InitFlag = false;
        public void ApplyTable() {
            if (DataType == null) {
                Debugg.LogWarning(this, "Select a data type.");
                return;
            }
            this.DestroyAllChild();
            m_Rows.Clear();
            m_Rows.AddRange(Rows.Where(r => r != null && r.IsValidAndIsDataOfType(DataType)).Select(r => new Row(r, this.NewChild(RowSceneObj2Col.New()))));
            foreach (var row in m_Rows) {
                row.UpdateText(null);
                row.SetSprite(Column.Name, CellBg);
                row.SetSprite(Column.Data, CellBg);
                row.ConfigureColor(Column.Name, NameColor, NameBackgroundColor);
                row.ConfigureColor(Column.Data, DataColor, DataBackgroundColor);
                row.ConfigureFont(Column.Name, NameFont, NameStyle, NameAlign, NameSize, NamePadding);
                row.ConfigureFont(Column.Data, DataFont, DataStyle, DataAlign, DataSize, DataPadding);
                row.AdjustSize(
                    tableWidth: rectTransform.rect.width,
                    allocation: WidthAllocation
                );
                //使用event用'+'優化?
            }
            Debugg.Log(this, $"Create {m_Rows.Count} valid row(s).");
        }
        public void UpdateTableSize() {
            rectTransform.sizeDelta = new Vector2(
                x: rectTransform.rect.width,
                y: CellHeight * m_Rows.Count + VerticalSpacing * (m_Rows.Count - 1)
            );
            Debugg.Log(this, $"Resize to {rectTransform.sizeDelta}");
        }
        protected override void Awake() {
            Debugg.Log(this, "Awake()");
            //init once at very first
            if (!m_InitFlag) {
                m_InitFlag = true;
                Debugg.Log(this, "init");
                //CellBg = DefaultSprite;
                rectTransform.sizeDelta = new(init_x, init_y);
                VLayout = GetComponent<VerticalLayoutGroup>();
                VLayoutConfig.Invoke(VLayout);
            }
            useGUILayout = false;
        }
        protected override void OnValidate() {
            //Editor-only function that Unity calls when the script is loaded or a value changes in the Inspector.
            //Revise list behaviour
            if (Rows != null) {
                if (Rows.Count > m_LengCache && m_LengCache != 0)
                    for (int i = m_LengCache; i < Rows.Count; i++) Rows[i].Reset(); //如果Row增加就做點事情
                m_LengCache = (sbyte)Rows.Count;

                //Reflect value to where they should be
                foreach (Row row in m_Rows) {
                    if (row == null) continue;
                    row.UpdateText(null);
                    row.SetSprite(Column.Name, CellBg);
                    row.SetSprite(Column.Data, CellBg);
                    row.ConfigureColor(Column.Name, NameColor, NameBackgroundColor);
                    row.ConfigureColor(Column.Data, DataColor, DataBackgroundColor);
                }
            }
            VLayout.spacing = VerticalSpacing;
            m_DirtyFlag = true;
        }
        void Update() {
            if (Application.isPlaying) return;
            if (m_DirtyFlag)
                m_DirtyFlag = false;
            foreach (Row row in m_Rows) {
                row.ConfigureFont(Column.Name, NameFont, NameStyle, NameAlign, NameSize, NamePadding);
                row.ConfigureFont(Column.Data, DataFont, DataStyle, DataAlign, DataSize, DataPadding);
                row.AdjustSize(
                    tableWidth: rectTransform.rect.width,
                    allocation: WidthAllocation
                );
            }
        }
        protected override void OnRectTransformDimensionsChange() {
            //同步修改子物件尺寸(寬度)與CellHeight(高度)
            foreach (Row row in m_Rows)
                row.AdjustSize(
                    tableWidth: rectTransform.rect.width,
                    allocation: WidthAllocation
                ); //VLayout會幫我處理高度
        }
#endif
        public enum Column {
            Name = 0,
            Data = 1
        }
        [Serializable]
        private class Row {
            [SerializeField] private RowInfo m_Info;
            [SerializeField] private RowSceneObj2Col m_SceneObj;
            [SerializeField] private TextHD m_NameText, m_DataText;
            public bool IsValid {
                get {
                    if (!(m_Info == null || m_SceneObj == null) && m_Info?.IsValid == true) return true;
                    else {
                        string msg = "invalid row. null:";
                        if (m_Info == null) msg += " m_Info";
                        else {
                            if (m_Info.DataRef == null) msg += " m_Info.DataRef";
                            if (m_Info.Name == null) msg += " m_Info.Name";
                        }
                        if (m_SceneObj == null) msg += " m_SceneObj";
                        Debugg.LogError(this, msg);
                        return false;
                    }
                }
            }
            public void UpdateText(object data) {
                if (!IsValid) {
                    Debugg.LogError(this, "invalid row.");
                    return;
                }
                //Name
                m_SceneObj.name = m_Info.Name;
                m_NameText.Text = m_Info.Name;
                //Data
                m_SceneObj.DataText.text =
                    (data == null) ? "" :
                    (m_Info.DataRef is FieldInfo) ?
                        (string)(m_Info.DataRef as FieldInfo).GetValue(data)
                        :
                        (string)(m_Info.DataRef as PropertyInfo).GetValue(data); //如果不是PropertyInfo就該出事
            }
            public void ApplyHorizontalSpacing() {

            }
            public void AdjustSize(float tableWidth, float allocation) {
                if (m_SceneObj == null || tableWidth < 0 || allocation < 0) return;
                float spacing = m_SceneObj.HLayout.spacing;
                m_NameText.RectTransform.sizeDelta = new((tableWidth - spacing) * allocation, 0);
                m_DataText.RectTransform.sizeDelta = new((tableWidth - spacing) * (1 - allocation), 0);
            }
            public void SetSprite(Column col, Sprite newSprite) {
                (TextHD text, Image image) = GetColumnOfIndex(col);
                if (text == null || image == null) return;
                image.sprite = newSprite;
            }
            public void ConfigureFont(Column col, Font font = null, FontStyle? fontStyle = null, TextAnchor? textAlign = null, int fontSize = -1, float padding = -1) {
                (TextHD text, Image image) = GetColumnOfIndex(col);
                if (text == null || image == null) return;
                if (font != null) text.Font = font;
                if (fontStyle != null) text.FontStyle = (FontStyle)fontStyle;
                if (textAlign != null) text.Alignment = (TextAnchor)textAlign;
                if (fontSize > 0) text.FontSize = fontSize;
                if (padding >= 0) text.Padding = padding;
            }
            public void ConfigureColor(Column col, Color? textColor = null, Color? bgColor = null) {
                (TextHD text, Image image) = GetColumnOfIndex(col);
                if (text == null || image == null) return;
                if (textColor == null) text.Color = (Color)textColor;
                if (bgColor == null) image.color = (Color)bgColor;
            }
            public (TextHD text, Image image) GetColumnOfIndex(Column col) {
                if (!IsValid) {
                    Debugg.LogError(this, "invalid row.");
                    return (null, null);
                }
                switch (col) {
                    case Column.Name:
                        return (m_NameText, m_SceneObj.NameCell);
                    case Column.Data:
                        return (m_DataText, m_SceneObj.DataCell);
                    default:
                        Debugg.LogError(this, $"unexist column '{col}'.");
                        return (null, null);
                }
            }
            public Row(RowInfo info, RowSceneObj2Col sceneObj = null) {
                m_Info = info;
                m_SceneObj = sceneObj != null ? sceneObj : RowSceneObj2Col.New();
                m_NameText = new(m_SceneObj.NameText);
                m_DataText = new(m_SceneObj.DataText);
            }
        }

        #endregion TableControl
#if UNITY_EDITOR
        #region Function for Tests
        // public void ValueCheckment() {
        //     Debugg.Log($"rect={rectTransform.rect} sizeDelta={rectTransform.sizeDelta}");
        //     Debugg.Log($"DataType={DataType}, List<RowInfo> has {Rows?.Count} rows, Rows.HashCode()={Rows?.GetHashCode()}.");
        //     for (int i = 0; i < Rows?.Count; i++) {
        //         Row r = m_rows[i];
        //         string msg = $"row_{i + 1}: ";
        //         if (r == null) msg += "null.";
        //         else {
        //             msg += $"Name={r.Name}, DataRef={r.}";
        //             if (r.SceneObj == null) msg += ", SceneObj=null";
        //             else if (r.SceneObj.NameText == null) msg += ", SceneObj.NameText=null";
        //             else msg += $", NameText={r.SceneObj.NameText.text}";
        //             msg += $", HashCode={r.GetHashCode()}.";
        //         }
        //         Debugg.Log(msg);
        //     }
        // }
        public void AssignTestInstance() {
            Debugg.Log(this, null == null);
        }
        #endregion
#endif
        #endregion DataTable2Col
    }

    #region RowInfo
    [Serializable]
    public class RowInfo { //32byte也算比較大了 就寫成class吧
        [SerializeField] public string Name = "";
        [SerializeField][HideInInspector] private string m_infoString = "";
        //Asset.SharedScripts.Entities.Snapshot>FollowerCount <= like this
        private MemberInfo m_infoCache; //Cache to reduce GetMemberInfo() calls;
        public MemberInfo DataRef {
            get {
                if (m_infoCache == null) {
                    if (m_infoString == null || m_infoString == "") return null;
                    else m_infoCache = GetMemberInfo(m_infoString);
                }
                return m_infoCache;
            }
            set {
                m_infoString = GetMemberInfoString(value);
                m_infoCache = null;
            }
        }
        public bool IsValid => !(DataRef == null || Name == null);
        public bool IsValidAndIsDataOfType(Type t) {
            if (!IsValid) return false;
            return DataRef.ReflectedType == t;
        }
        //public RowSceneObj2Col SceneObj = null;
#if UNITY_EDITOR
        //public RowSceneObj2Col CreateObj() => SceneObj = RowSceneObj2Col.New();
        // public RowSceneObj2Col CreateObj(Action<(Text nameText, Text dataText, HorizontalLayoutGroup hLayout)> config) =>
        //     SceneObj = RowSceneObj2Col.New((obj) => config?.Invoke((obj.NameText, obj.NameText, obj.HLayout)));

        public void Reset() {
            Name = "";
            DataRef = null;
        }
#endif
    }
    #endregion RowInfo 
}
