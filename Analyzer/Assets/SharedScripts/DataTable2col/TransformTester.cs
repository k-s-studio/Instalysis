using UnityEngine;
using System.Linq;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public class TransformTester : MonoBehaviour {
    private RectTransform m_rectTransformCache;
    private RectTransform rectTransform {
        get {
            if (m_rectTransformCache == null) m_rectTransformCache = GetComponent<RectTransform>();
            return m_rectTransformCache;
        }
    }
    public Vector2 AnchorMin, AnchorMax, Pivot;
    public Vector3 LocalPosition, LocalScale;
    public Vector2 SizeDelta, OffsetMin, OffsetMax;
    private bool[] m_Dirty = { false, false, false, false, false, false, false, false };
    private void GetValues() { 
        AnchorMin = rectTransform.anchorMin;
        AnchorMax = rectTransform.anchorMax;
        Pivot = rectTransform.pivot;

        LocalPosition = rectTransform.localPosition;
        LocalScale = rectTransform.localScale;

        SizeDelta = rectTransform.sizeDelta;
        OffsetMin = rectTransform.offsetMin;
        OffsetMax = rectTransform.offsetMax;
    } 
    public void ShowRect() => Debugg.Log(this, $"rect={rectTransform.rect}");
    void OnEnable() {
        GetValues();
    }
    void OnValidate() {
        if (AnchorMin != rectTransform.anchorMin) m_Dirty[0] = true;
        if (AnchorMax != rectTransform.anchorMax) m_Dirty[1] = true;
        if (Pivot != rectTransform.pivot) m_Dirty[2] = true;

        if (LocalPosition != rectTransform.localPosition) m_Dirty[3] = true;
        if (LocalScale != rectTransform.localScale) m_Dirty[4] = true;

        if (SizeDelta != rectTransform.sizeDelta) m_Dirty[5] = true;
        if (OffsetMin != rectTransform.offsetMin) m_Dirty[6] = true;
        if (OffsetMax != rectTransform.offsetMax) m_Dirty[7] = true;
        if (m_Dirty.Count(b => b) > 1) Debugg.Log(this, $"muilty dirty properties decteded.");
    }
    void Update() {
        for (int i = 0; i < 8; i++) {
            if (m_Dirty[i]) {
                m_Dirty[i] = false;
                switch (i) {
                    case 0:
                        Debugg.Log(this, $"Set anchorMin= {rectTransform.anchorMin} -> {AnchorMin}");
                        rectTransform.anchorMin = AnchorMin;
                        break;
                    case 1:
                        Debugg.Log(this, $"Set anchorMax= {rectTransform.anchorMax} -> {AnchorMax}");
                        rectTransform.anchorMax = AnchorMax;
                        break;
                    case 2:
                        Debugg.Log(this, $"Set pivot= {rectTransform.pivot} -> {Pivot}");
                        rectTransform.pivot = Pivot;
                        break;
                    case 3:
                        Debugg.Log(this, $"Set localPosition= {rectTransform.localPosition} -> {LocalPosition}");
                        rectTransform.localPosition = LocalPosition;
                        break;
                    case 4:
                        Debugg.Log(this, $"Set localScale= {rectTransform.localScale} -> {LocalScale}");
                        rectTransform.localScale = LocalScale;
                        break;
                    case 5:
                        Debugg.Log(this, $"Set sizeDelta= {rectTransform.sizeDelta} -> {SizeDelta}");
                        rectTransform.sizeDelta = SizeDelta;
                        break;
                    case 6:
                        Debugg.Log(this, $"Set offsetMin= {rectTransform.offsetMin} -> {OffsetMin}");
                        rectTransform.offsetMin = OffsetMin;
                        break;
                    case 7:
                        Debugg.Log(this, $"Set offsetMax= {rectTransform.offsetMax} -> {OffsetMax}");
                        rectTransform.offsetMax = OffsetMax;
                        break;
                    default: break;
                }
                GetValues();
            }
        }
    }
    void OnRectTransformDimensionsChange() {
        GetValues();
    }
}