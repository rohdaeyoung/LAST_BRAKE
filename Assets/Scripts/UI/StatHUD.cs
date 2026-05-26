using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// LAST BRAKE - 인게임 스탯 HUD
/// Step 씬 상단에 항상 표시. 선택 후 수치 변화 애니메이션.
/// Slider 대신 anchorMax.x 제어 방식 → Unity 레이아웃 충돌 없음.
/// </summary>
public class StatHUD : MonoBehaviour
{
    public static StatHUD Instance { get; private set; }

    [Header("스탯 아이콘 (EffectLinker 자동 연결)")]
    [SerializeField] public Sprite iconReason;
    [SerializeField] public Sprite iconMental;
    [SerializeField] public Sprite iconDependency;
    [SerializeField] public Sprite iconRelationship;
    [SerializeField] public Sprite iconReality;

    // 런타임 슬롯
    private HudSlot _slotInt;
    private HudSlot _slotRisk;
    private HudSlot _slotAddict;
    private GameObject _hudCanvas;

    // ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        BuildHUD();
        Refresh();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ══════════════════════════════════════════════════════════════
    //  HUD 생성
    // ══════════════════════════════════════════════════════════════
    private void BuildHUD()
    {
        if (_hudCanvas != null) return;

        _hudCanvas = new GameObject("StatHUD_Canvas");
        DontDestroyOnLoad(_hudCanvas);
        _hudCanvas.transform.SetParent(transform);

        var canvas          = _hudCanvas.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var sc = _hudCanvas.AddComponent<CanvasScaler>();
        sc.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920, 1080);
        sc.matchWidthOrHeight  = 0.5f;
        _hudCanvas.AddComponent<GraphicRaycaster>();

        // 상단 바 컨테이너
        var bar = new GameObject("HUDBar");
        bar.transform.SetParent(_hudCanvas.transform, false);
        var barImg = bar.AddComponent<Image>();
        barImg.color         = new Color(0f, 0f, 0f, 0.60f);
        barImg.raycastTarget = false;
        var barRT = bar.GetComponent<RectTransform>();
        barRT.anchorMin        = new Vector2(0f, 1f);
        barRT.anchorMax        = new Vector2(1f, 1f);
        barRT.pivot            = new Vector2(0.5f, 1f);
        barRT.anchoredPosition = new Vector2(0f, 0f);
        barRT.sizeDelta        = new Vector2(0f, 56f);

        // 세 슬롯 — 각각 1/3 너비로 배치
        _slotInt    = CreateSlot(bar.transform, "이성",   iconReason,
            new Vector2(0f,    0f), new Vector2(0.333f, 1f),
            new Color(0.3f, 0.85f, 0.4f, 1f));

        _slotRisk   = CreateSlot(bar.transform, "위험도", iconMental,
            new Vector2(0.333f, 0f), new Vector2(0.666f, 1f),
            new Color(0.9f, 0.45f, 0.2f, 1f));

        _slotAddict = CreateSlot(bar.transform, "의존도", iconDependency,
            new Vector2(0.666f, 0f), new Vector2(1f, 1f),
            new Color(0.8f, 0.2f, 0.75f, 1f));
    }

    // ── 슬롯 1개 생성 ────────────────────────────────────────────
    private HudSlot CreateSlot(Transform parent, string label, Sprite icon,
        Vector2 anchorMin, Vector2 anchorMax, Color barColor)
    {
        // 슬롯 컨테이너
        var slot = new GameObject($"Slot_{label}");
        slot.transform.SetParent(parent, false);
        var slotRT = slot.AddComponent<RectTransform>();
        slotRT.anchorMin = anchorMin; slotRT.anchorMax = anchorMax;
        slotRT.offsetMin = new Vector2(8f, 0f); slotRT.offsetMax = new Vector2(-8f, 0f);

        // 아이콘 (왼쪽 고정, 40x40)
        Image iconImg = null;
        if (icon != null)
        {
            var iGO = new GameObject("Icon");
            iGO.transform.SetParent(slot.transform, false);
            iconImg = iGO.AddComponent<Image>();
            iconImg.sprite         = icon;
            iconImg.preserveAspect = true;
            iconImg.raycastTarget  = false;
            var iRT = iconImg.rectTransform;
            iRT.anchorMin = new Vector2(0f, 0.5f); iRT.anchorMax = new Vector2(0f, 0.5f);
            iRT.pivot     = new Vector2(0f, 0.5f);
            iRT.anchoredPosition = new Vector2(4f, 0f);
            iRT.sizeDelta        = new Vector2(32f, 32f);
        }

        // 라벨 텍스트
        var lGO  = new GameObject("Label");
        lGO.transform.SetParent(slot.transform, false);
        var lTMP = lGO.AddComponent<TextMeshProUGUI>();
        lTMP.text          = label;
        lTMP.fontSize      = 16f;
        lTMP.color         = new Color(0.85f, 0.85f, 0.85f, 1f);
        lTMP.alignment     = TextAlignmentOptions.Left;
        lTMP.raycastTarget = false;
        var lRT = lGO.GetComponent<RectTransform>();
        lRT.anchorMin = new Vector2(0f, 0.5f); lRT.anchorMax = new Vector2(0f, 0.5f);
        lRT.pivot     = new Vector2(0f, 0.5f);
        lRT.anchoredPosition = new Vector2(40f, 4f);
        lRT.sizeDelta        = new Vector2(60f, 24f);

        // 바 배경
        var bgGO  = new GameObject("BarBG");
        bgGO.transform.SetParent(slot.transform, false);
        var bgImg = bgGO.AddComponent<Image>();
        bgImg.color        = new Color(0.15f, 0.15f, 0.20f, 1f);
        bgImg.raycastTarget = false;
        var bgRT  = bgGO.GetComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(0f, 0f); bgRT.anchorMax = new Vector2(1f, 0f);
        bgRT.pivot     = new Vector2(0f, 0f);
        bgRT.anchoredPosition = new Vector2(104f, 8f);
        bgRT.sizeDelta        = new Vector2(-112f, 10f);

        // 바 Fill — anchorMax.x로 비율 제어
        var fillGO  = new GameObject("BarFill");
        fillGO.transform.SetParent(bgGO.transform, false);
        var fillImg = fillGO.AddComponent<Image>();
        fillImg.color        = barColor;
        fillImg.raycastTarget = false;
        var fillRT  = fillGO.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = new Vector2(0f, 1f);   // ← x만 변경해서 길이 조절
        fillRT.offsetMin = fillRT.offsetMax = Vector2.zero;

        // 수치 텍스트
        var vGO  = new GameObject("Value");
        vGO.transform.SetParent(slot.transform, false);
        var vTMP = vGO.AddComponent<TextMeshProUGUI>();
        vTMP.text          = "---";
        vTMP.fontSize      = 18f;
        vTMP.color         = Color.white;
        vTMP.alignment     = TextAlignmentOptions.Right;
        vTMP.raycastTarget = false;
        var vRT  = vGO.GetComponent<RectTransform>();
        vRT.anchorMin = new Vector2(1f, 0.5f); vRT.anchorMax = new Vector2(1f, 0.5f);
        vRT.pivot     = new Vector2(1f, 0.5f);
        vRT.anchoredPosition = new Vector2(-4f, 0f);
        vRT.sizeDelta        = new Vector2(42f, 28f);

        return new HudSlot
        {
            fillRT    = fillRT,
            fillImage = fillImg,
            valueTMP  = vTMP,
            baseColor = barColor,
        };
    }

    // ══════════════════════════════════════════════════════════════
    //  퍼블릭 API
    // ══════════════════════════════════════════════════════════════

    /// <summary>StatManager 값으로 즉시 갱신</summary>
    public void Refresh()
    {
        if (StatManager.Instance == null) return;
        SetSlot(_slotInt,    StatManager.Instance.INT,    false);
        SetSlot(_slotRisk,   StatManager.Instance.RISK,   true);
        SetSlot(_slotAddict, StatManager.Instance.ADDICT, true);
    }

    /// <summary>선택 후 수치 변화 애니메이션</summary>
    public void AnimateChange(int newInt, int newRisk, int newAddict)
    {
        StartCoroutine(AnimSlot(_slotInt,    newInt,    false));
        StartCoroutine(AnimSlot(_slotRisk,   newRisk,   true));
        StartCoroutine(AnimSlot(_slotAddict, newAddict, true));
    }

    public void Show(bool visible)
    {
        if (_hudCanvas != null) _hudCanvas.SetActive(visible);
    }

    // ─────────────────────────────────────────────────────────────
    private static void SetSlot(HudSlot s, int value, bool higherIsBad)
    {
        if (s == null) return;
        float ratio = value / 100f;
        if (s.fillRT  != null) s.fillRT.anchorMax  = new Vector2(ratio, 1f);
        if (s.valueTMP != null) s.valueTMP.text     = value.ToString();
        ApplyDangerColor(s, value, higherIsBad);
    }

    private static IEnumerator AnimSlot(HudSlot s, int target, bool higherIsBad)
    {
        if (s == null) yield break;
        float from = s.fillRT != null
            ? s.fillRT.anchorMax.x * 100f
            : 0f;
        float dur = 0.6f, t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float v = Mathf.Lerp(from, target, t / dur);
            if (s.fillRT  != null) s.fillRT.anchorMax  = new Vector2(v / 100f, 1f);
            if (s.valueTMP != null) s.valueTMP.text     = Mathf.RoundToInt(v).ToString();
            yield return null;
        }
        SetSlot(s, target, higherIsBad);
    }

    private static void ApplyDangerColor(HudSlot s, int value, bool higherIsBad)
    {
        if (s?.fillImage == null) return;
        bool danger = higherIsBad ? value > 60 : value < 40;
        s.fillImage.color = danger
            ? Color.Lerp(s.baseColor, new Color(0.9f, 0.15f, 0.15f, 1f), 0.7f)
            : s.baseColor;
    }

    // ── 내부 데이터 ───────────────────────────────────────────────
    private class HudSlot
    {
        public RectTransform     fillRT;
        public Image             fillImage;
        public TextMeshProUGUI   valueTMP;
        public Color             baseColor;
    }
}
