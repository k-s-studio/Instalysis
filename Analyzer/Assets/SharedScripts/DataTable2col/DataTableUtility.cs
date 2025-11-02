using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Assets.DataTable2col;


namespace Assets.DataTable2col {
    public static class Utility {

    }
    public class TextHD {
        //父座標 => 圖釘框 => offset => 在自己座標釘Pivot(以Pivot的worldspace座標為(0,0)算出rect) => 自身Pivot和父物件Pivot的位置差=position => 以自身Pivot為基準拉伸
        //rect使用坐標系和底下物件使用的座標系是一樣的，可以說每個Pivot都帶著一張座標網格，並且xyz單位長受該Transform.LocalScale拉伸?
        private const int MAXFONTSIZE = 300;
        private readonly Text m_text;
        private int m_fontSize = 1;
        private float m_padding = 0;
        private float Muiltiplier => m_fontSize / (float)MAXFONTSIZE;  //the revised localscale to let font look like fontsize
        public int FontSize {
            get => (int)(MAXFONTSIZE * Muiltiplier);
            set {
                if (value <= 0) return;
                m_fontSize = value;
                UpdateSize();
            }
        }
        public string Text {
            get => m_text.text;
            set => m_text.text = value;
        }
        public float Padding {
            get => m_padding;
            set {
                m_padding = value;
                ResponsePadding();
            }
        }
        public Font Font {
            get => m_text.font;
            set => m_text.font = value;
        }
        public FontStyle FontStyle {
            get => m_text.fontStyle;
            set => m_text.fontStyle = value;
        }
        public Color Color {
            get => m_text.color;
            set => m_text.color = value;
        }
        public TextAnchor Alignment {
            get => m_text.alignment;
            set => m_text.alignment = value;
        }
        public VerticalWrapMode VerticalOverflow => m_text.verticalOverflow; //fixed to Overflow
        public HorizontalWrapMode HorizontalWrapMode => m_text.horizontalOverflow; //fixed to Overflow
        public TextHD(Text text) {
            m_text = text;
            m_fontSize = text.fontSize;
            m_padding = text.rectTransform.offsetMin.x; //預設為依存父物件的模式
            Init();
            UpdateSize();
            ResponsePadding();
        }
        private void Init() {
            if (m_text == null) {
                Debug.LogError("TextHD >> null m_text to Init().");
                return;
            }
            m_text.verticalOverflow = VerticalWrapMode.Overflow;
            m_text.horizontalOverflow = HorizontalWrapMode.Overflow;
            m_text.fontSize = MAXFONTSIZE;
            m_text.rectTransform.anchorMin = Vector2.zero;
            m_text.rectTransform.anchorMax = Vector2.one;
            m_text.rectTransform.pivot = Vector2.one * 0.5f;
            m_text.rectTransform.offsetMax = Vector2.zero;
            m_text.rectTransform.offsetMin = Vector2.zero;
        }
        /// <summary>
        /// 修改Text.rectTransformg使得視覺大小=TextHD.m_fontSize
        /// </summary>
        private void UpdateSize() {
            //Vector2 size = m_text.rectTransform.sizeDelta;
            m_text.rectTransform.anchorMin = (1 - 1 / Muiltiplier) * 0.5f * Vector2.one;
            m_text.rectTransform.anchorMax = (1 + 1 / Muiltiplier) * 0.5f * Vector2.one;
            m_text.rectTransform.localScale = Vector3.one * Muiltiplier;
            //m_text.rectTransform.sizeDelta = size; //這應該本來就不會被改到吧
        }
        private void ResponsePadding() {
            Vector2 newPadding = Vector2.one * m_padding / Muiltiplier;
            m_text.rectTransform.offsetMax = -newPadding;
            m_text.rectTransform.offsetMin = newPadding;
        }
        public GameObject GameObject => m_text.gameObject;
        public Transform Transform => m_text.transform;
        public RectTransform RectTransform => m_text.rectTransform;
    }

    #region TablizableAttribute
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class TablizableAttribute : Attribute {
        public static List<Type> RegisteredTypes { get; private set; } = new(); //應該可以減少很多效能開銷的做法，但就是使用上欠缺美感
        public TablizableAttribute(Type t) {
            if (!RegisteredTypes.Contains(t)) RegisteredTypes.Add(t);
        }
    }
    #endregion TablizableAttribute
}

public static class DataTableUtility {
    public const string CELLBG_DEFAULT = "Assets/SharedScripts/DataTable2col/cell_large.png"; //"DataTable2col/cell_large"
    private const char FieldSeperator = '/', PropertySeperator = '>';

    #region UnityObjectExtension
    public static T NewChild<T, T_parent>(this T_parent parent, T objRef) where T_parent : Component where T : Component {
        T child = objRef;
        child.transform.SetParent(parent.transform);
        return child;
    }
    public static GameObject NewChild<T_parent>(this T_parent parent, string name)
        where T_parent : Component => parent.transform.NewChild(new GameObject(name));

    public static GameObject NewChild(this Transform parent, GameObject objRef) {
        GameObject child = objRef;
        child.transform.parent = parent;
        return child;
    }

    public static void DestroyAllChild<T>(this T gameObject) where T : Component => gameObject.transform.DestroyAllChild();
    public static void DestroyAllChild(this Transform transform) {
        while (transform.childCount > 0)
            GameObject.DestroyImmediate(transform.GetChild(0).gameObject);
        // foreach (Transform child in transform) DestroyImmediate(child.gameObject); //會造成隔一個刪不掉，因為transform中刪掉child向前遞補，同時索引值仍往後移
    }
    #endregion UnityObjectExtension
    #region Table
#if UNITY_EDITOR
    public static Sprite DefaultSprite =>
        UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(CELLBG_DEFAULT);
    //Resources.Load<Sprite>(CELLBG_DEFAULT); 
#endif
    #endregion Table
    #region Row
    /// <summary>
    /// 使用到Type.GetType()取得字串代表的FieldInfo或PropertyInfo
    /// </summary>
    /// <returns>MemberInfo. 無效字串或找不到Type則return null.</returns>
    public static MemberInfo GetMemberInfo(string infoString) {
        string[] strArr = infoString.Split(FieldSeperator);
        if (strArr.Length == 2) return Type.GetType(strArr[0])?.GetField(strArr[1]);

        strArr = infoString.Split(PropertySeperator);
        if (strArr.Length == 2) return Type.GetType(strArr[0])?.GetProperty(strArr[1]);
        // 1~2次整數比較,1~2次陣列分割,1次GetType(),2次索引取值 vs.1~2次遍歷集合比較,1次GetType(),2次陣列分割,2次索引取值
        // if (infoString.Contains(FieldSeperator)) return Type.GetType(infoString.Split(FieldSeperator)[0])?.GetField(infoString.Split(FieldSeperator)[1]);
        // if (infoString.Contains(PropertySeperator)) return Type.GetType(infoString.Split(PropertySeperator)[0])?.GetProperty(infoString.Split(PropertySeperator)[1]);
        return null;
    }
    public static string GetMemberInfoString(MemberInfo memInfo) => (memInfo == null) ? "" : memInfo.ReflectedType.FullName + GetSeperator(memInfo) + memInfo.Name;
    private static char GetSeperator(MemberInfo info) {
        if (info is FieldInfo) return FieldSeperator;
        if (info is PropertyInfo) return PropertySeperator;
        return ' ';
    }
    #endregion Row
    #region RowSceneObj
    public static void SetSizeHD(this Text text, int size) {
        text.fontSize = 300;
        text.transform.localScale = Vector3.one * size / text.fontSize;
    }
    #endregion RowSceneObj
}
#region Debug
class Debugg {
    public static void Log(object o, object msg) => Debug.Log($"{o.GetType().Name} >> {msg}");
    public static void LogWarning(object o, string msg) => Debug.LogWarning($"{o.GetType().Name} >> {msg}");
    public static void LogError(object o, string msg) => Debug.LogError($"{o.GetType().Name} >> {msg}");
}
#endregion Debug