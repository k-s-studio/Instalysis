using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;

namespace Assets.TextHD.V1 {
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class TextHD : MonoBehaviour {
        private readonly Stopwatch stopwatch = new();
        private const int MAXFONTSIZE = 300;
        private static Text m_TextCache = null; //for Init
        private bool m_AbandonedFlag = false;

        //inner fields and property
        [HideInInspector][SerializeField] private Text m_Text;
        [HideInInspector][SerializeField] private float m_FontSize = -1; //暫時兼任 m_InitFlag
        [HideInInspector][SerializeField] private float m_Padding = 0;
        //[HideInInspector][SerializeField] private bool m_InitFlag = false; 
        private float Muiltiplier => m_FontSize / (float)MAXFONTSIZE;

        #region Inspector
        [TextArea(3, 10)] public string text;
        public Font font;
        public FontStyle fontStyle = FontStyle.Normal;
        public float fontSize = 36;
        public float padding = 0;
        public TextAnchor textAlign = TextAnchor.MiddleLeft;
        public Color textColor = Color.black;
        #endregion Inspector
        #region Text properties
        public float FontSize {
            get => m_FontSize;
            set {
                if (value <= 0) return;
                m_FontSize = value;
                UpdateSize();
            }
        }
        public string Text {
            get => m_Text.text;
            set => m_Text.text = value;
        }
        public float Padding {
            get => m_Padding;
            set {
                m_Padding = value;
                ResponsePadding();
            }
        }
        public Font Font {
            get => m_Text.font;
            set => m_Text.font = value;
        }
        public FontStyle FontStyle {
            get => m_Text.fontStyle;
            set => m_Text.fontStyle = value;
        }
        public Color Color {
            get => m_Text.color;
            set => m_Text.color = value;
        }
        public TextAnchor Alignment {
            get => m_Text.alignment;
            set => m_Text.alignment = value;
        }
        public VerticalWrapMode VerticalOverflow => m_Text.verticalOverflow; //fixed to Overflow
        public HorizontalWrapMode HorizontalWrapMode => m_Text.horizontalOverflow; //fixed to Overflow
        #endregion Text properties
        #region Text contorl funcs
        private void Init() { //Get NullRefException when m_Text is still null
            //cache origin value
            m_FontSize = m_Text.fontSize;
            m_Padding = 0;// m_Text.rectTransform.offsetMin.x; //當anchor不是(0,0)~(1,1)時 這個值不是padding，想其他方法
            //configure Text
            ConfigureText();

            //give the value back
            UpdateSize();
            ResponsePadding();
        }
        private void ConfigureText() {
            //set to initial status
            m_Text.verticalOverflow = VerticalWrapMode.Overflow;
            m_Text.horizontalOverflow = HorizontalWrapMode.Overflow;
            m_Text.fontSize = MAXFONTSIZE;
            m_Text.rectTransform.anchorMin = Vector2.zero;
            m_Text.rectTransform.anchorMax = Vector2.one;
            m_Text.rectTransform.pivot = Vector2.one * 0.5f;
            m_Text.rectTransform.offsetMax = Vector2.zero;
            m_Text.rectTransform.offsetMin = Vector2.zero;
        }
        private Text Configure(Text text = null) {
            if (text == null) text = m_Text;
            //set to initial status
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.fontSize = MAXFONTSIZE;
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.pivot = Vector2.one * 0.5f;
            text.rectTransform.offsetMax = Vector2.zero;
            text.rectTransform.offsetMin = Vector2.zero;
            return text;
        }
        /// <summary>
        /// 修改Text.rectTransformg使得視覺大小=TextHD.m_fontSize
        /// </summary>
        private void UpdateSize() {
            m_Text.rectTransform.anchorMin = (1 - 1 / Muiltiplier) * 0.5f * Vector2.one;
            m_Text.rectTransform.anchorMax = (1 + 1 / Muiltiplier) * 0.5f * Vector2.one;
            m_Text.rectTransform.localScale = Vector3.one * Muiltiplier;
        }
        private void ResponsePadding() {
            Vector2 newPadding = Vector2.one * m_Padding / Muiltiplier;
            Debugg.Log(this, $"new padding={newPadding}");
            m_Text.rectTransform.offsetMax = -newPadding;
            m_Text.rectTransform.offsetMin = newPadding;
        }
        public void Restore() {
            ConfigureText();
            UpdateSize();
            ResponsePadding();
        }
        public void Unbind() => m_Text = null;
        public bool NoBindingText => m_Text == null;

        #endregion contorl funcs
        #region Communication & ConsistencyCheck
        private void UpdateInspector() {
            fontSize = (int)this.FontSize;
            padding = this.Padding;
            font = this.Font;
            fontStyle = this.FontStyle;
            textAlign = this.Alignment;
            textColor = this.Color;
        }
        #endregion Communication
        #region MonoBehaviour
        void Awake() {
            stopwatch.Start();
            //if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (m_FontSize < 0) {
                #region Find & bind the Text component if not init yet
                // 1.我身上有 2.我身上沒有但cache有 3.我身上沒有而且cache也沒有
                // 最常見的情況是3>1>2，但3不可能只判斷一次，而所有情況最多都只要兩次條件判斷
                if (TryGetComponent<Text>(out var txt)) {
                    m_TextCache = txt;
                    new GameObject("TextHD").AddComponent<TextHD>(); //then the new TextHD take over the works
                    m_AbandonedFlag = true;
                    return;
                }
                else if (m_TextCache != null) {
                    txt = m_TextCache;
                    m_TextCache = null;
                    transform.SetParent(txt.transform.parent);
                    txt.transform.SetParent(this.transform);
                }
                else {
                    // if not found, create a child and attach one.
                    txt = new GameObject("rawText").AddComponent<Text>();
                    txt.transform.SetParent(this.transform);
                    txt.text = "Hello world!";
                }
                m_Text = txt;
                #endregion
                Init();
                UpdateInspector();
            }
            else if (m_Text==null) Debugg.LogWarning(this, "binding Text is missesd.");
            stopwatch.Stop();
            Debugg.Log(this, $"Awake() uses {stopwatch.ElapsedMilliseconds} ms.");
        }
        private readonly (FieldInfo field, PropertyInfo property, bool dirty)[] ValuePairs = {
            (typeof(TextHD).GetField("fontSize"),typeof(TextHD).GetProperty("FontSize"),false),
            (typeof(TextHD).GetField("padding"),typeof(TextHD).GetProperty("Padding"),false),
            (typeof(TextHD).GetField("font"),typeof(TextHD).GetProperty("Font"),false),
            (typeof(TextHD).GetField("fontStyle"),typeof(TextHD).GetProperty("FontStyle"),false),
            (typeof(TextHD).GetField("textAlign"),typeof(TextHD).GetProperty("Alignment"),false),
            (typeof(TextHD).GetField("textColor"),typeof(TextHD).GetProperty("Color"),false)
        };
        void OnValidate() {
            if (m_AbandonedFlag) return;
            stopwatch.Restart();
            for (int i = 0; i < ValuePairs.Length; i++) {
                var fieldValue = ValuePairs[i].field.GetValue(this).GetHashCode();
                var propertyValue = ValuePairs[i].property.GetValue(this).GetHashCode();
                //Debugg.Log(this, $"{ValuePairs[i].field.Name}={fieldValue}, {ValuePairs[i].property.Name}={propertyValue}, {fieldValue == propertyValue}.");
                if (fieldValue != propertyValue) ValuePairs[i].dirty = true;
            }
            stopwatch.Stop();
            Debugg.Log(this, $"OnValidate() uses {stopwatch.ElapsedMilliseconds} ms.");
            // if (fontSize != this.FontSize) ;
            // if (padding != this.Padding) ;
            // if (font != this.Font) ;
            // if (fontStyle != this.FontStyle) ;
            // if (textAlign != this.Alignment) ;
            // if (textColor != this.Color) ;
            //用struct包起來一起檢查會比較快嗎? 不曉得。
        }
        void Update() {
            if (m_AbandonedFlag) DestroyImmediate(this);
            //if(m_Text.rectTransform.hasChanged) UpdateInspector(); //屬性直接依存於外部Component才有意義
            sbyte j = 0;
            stopwatch.Restart();
            for (int i = 0; i < ValuePairs.Length; i++)
                if (ValuePairs[i].dirty) {
                    ValuePairs[i].dirty = false;
                    ValuePairs[i].property.SetValue(this, ValuePairs[i].field.GetValue(this));
                    UpdateInspector();
                    j++;
                }
            stopwatch.Stop();
            if (j > 0) Debugg.Log(this, $"Update() uses {stopwatch.ElapsedMilliseconds} ms.");
            if (j > 1) Debugg.LogWarning(this, $"muiltiple dirties.");
        }
        void OnGUI() {
            useGUILayout = false;
        }
        #endregion MonoBehaviour
    }
}
//父座標 => 圖釘框 => offset => 在自己座標釘Pivot(以Pivot的worldspace座標為(0,0)算出rect) => 自身Pivot和父物件Pivot的位置差=position => 以自身Pivot為基準拉伸
//rect使用坐標系和底下物件使用的座標系是一樣的，可以說每個Pivot都帶著一張座標網格，並且xyz單位長受該Transform.LocalScale拉伸?