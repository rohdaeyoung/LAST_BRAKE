using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Step 씬 로드 시 대화 UI를 보장합니다.
/// - 기존 DialogueUI가 있으면 폰트·크기·색상·레이아웃을 전부 보강
/// - 없으면 전체 런타임 생성
/// - 씬 내 모든 TextMeshProUGUI에 NotoSansKR-Regular 강제 적용
/// </summary>
public class DialogueBootstrap : MonoBehaviour
{
    public static DialogueUI CreatedDialogueUI { get; private set; }
    public static ChoiceUI   CreatedChoiceUI   { get; private set; }

    // ─── 디자인 상수 ──────────────────────────────────────────────────────
    const string FONT_PATH        = "Fonts & Materials/NotoSansKR-Regular SDF";

    // 참조 해상도 (PC 1080p)
    const float REF_W = 1920f;
    const float REF_H = 1080f;

    // 대화 패널
    const float PANEL_HEIGHT      = 280f;   // 대화창 높이
    const float PANEL_MARGIN      = 30f;    // 좌우/하단 여백
    const float PANEL_ALPHA       = 0.93f;
    const float DIALOGUE_FONTSIZE = 34f;    // 대화 텍스트 크기
    const float SPEAKER_FONTSIZE  = 28f;    // 화자 이름 크기

    // 선택지 패널
    const float CHOICE_BTN_W      = 860f;
    const float CHOICE_BTN_H      = 90f;
    const float CHOICE_BTN_GAP    = 16f;
    const float CHOICE_FONTSIZE   = 28f;

    // ─── 색상 ─────────────────────────────────────────────────────────────
    static readonly Color PanelBG      = new Color(0.05f, 0.05f, 0.13f, PANEL_ALPHA);
    static readonly Color SpeakerColor = new Color(1f,    0.85f, 0.30f, 1f);
    static readonly Color DialogColor  = Color.white;
    static readonly Color BtnBG        = new Color(0.10f, 0.10f, 0.22f, 1f);
    static readonly Color BtnBGHover   = new Color(0.18f, 0.18f, 0.35f, 1f);

    // ──────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        // 0) 핵심 싱글톤 자동 생성 (씬 직접 실행 시 MainMenu 없어도 동작)
        EnsureManagers();

        // 1) 한글 폰트 로드
        TMP_FontAsset font = Resources.Load<TMP_FontAsset>(FONT_PATH);
        if (font == null)
            Debug.LogWarning("[Bootstrap] NotoSansKR-Regular SDF 로드 실패 — " +
                             "TextMesh Pro/Resources/Fonts & Materials/ 경로 확인");

        // 2) 기존 DialogueUI 탐색 (비활성 포함)
        var existingDUI = FindFirstObjectByType<DialogueUI>(FindObjectsInactive.Include);
        if (existingDUI != null)
        {
            RepairDialogueUI(existingDUI, font);
            CreatedDialogueUI = existingDUI;
        }
        else
        {
            BuildFullUI(font);
        }

        // 3) 기존 ChoiceUI 탐색 & 보강
        var existingCUI = FindFirstObjectByType<ChoiceUI>(FindObjectsInactive.Include);
        if (existingCUI != null)
            RepairChoiceUI(existingCUI, font);

        // 4) 씬 내 모든 TMP에 한글 폰트 적용 (박스 완전 차단)
        if (font != null)
            ApplyFontToAll(font);

        // 5) Canvas 해상도 보강
        FixCanvasScalers();

        // 6) EventSystem 보장
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  기존 DialogueUI 보강
    // ══════════════════════════════════════════════════════════════════════
    void RepairDialogueUI(DialogueUI dui, TMP_FontAsset font)
    {
        Transform panel = dui.transform;

        // ① 패널 Image 배경
        var img = panel.GetComponent<Image>() ?? panel.gameObject.AddComponent<Image>();
        img.color = PanelBG;

        // ② RectTransform — 화면 하단, 전체 너비
        var rt = panel.GetComponent<RectTransform>();
        rt.anchorMin         = new Vector2(0f, 0f);
        rt.anchorMax         = new Vector2(1f, 0f);
        rt.pivot             = new Vector2(0.5f, 0f);
        rt.anchoredPosition  = new Vector2(0f, PANEL_MARGIN);
        rt.sizeDelta         = new Vector2(-PANEL_MARGIN * 2f, PANEL_HEIGHT);

        // ③ SpeakerName
        var spTf = panel.Find("SpeakerName");
        TextMeshProUGUI spTMP;
        if (spTf == null)
        {
            var go = new GameObject("SpeakerName");
            go.transform.SetParent(panel, false);
            spTMP = go.AddComponent<TextMeshProUGUI>();
            spTf  = go.transform;
        }
        else spTMP = spTf.GetComponent<TextMeshProUGUI>()
                     ?? spTf.gameObject.AddComponent<TextMeshProUGUI>();

        if (font != null) spTMP.font = font;
        spTMP.fontSize  = SPEAKER_FONTSIZE;
        spTMP.fontStyle = FontStyles.Bold;
        spTMP.color     = SpeakerColor;
        var spRT        = spTf.GetComponent<RectTransform>();
        spRT.anchorMin        = new Vector2(0f, 1f);
        spRT.anchorMax        = new Vector2(0.5f, 1f);
        spRT.pivot            = new Vector2(0f, 0f);
        spRT.anchoredPosition = new Vector2(24f, 4f);
        spRT.sizeDelta        = new Vector2(0f, SPEAKER_FONTSIZE + 8f);

        // ④ DialogueText
        var txtTf = panel.Find("DialogueText");
        TextMeshProUGUI txtTMP;
        if (txtTf == null)
        {
            var go = new GameObject("DialogueText");
            go.transform.SetParent(panel, false);
            txtTMP = go.AddComponent<TextMeshProUGUI>();
            txtTf  = go.transform;
        }
        else txtTMP = txtTf.GetComponent<TextMeshProUGUI>()
                      ?? txtTf.gameObject.AddComponent<TextMeshProUGUI>();

        if (font != null) txtTMP.font = font;
        txtTMP.fontSize           = DIALOGUE_FONTSIZE;
        txtTMP.color              = DialogColor;
        txtTMP.lineSpacing        = 6f;
        txtTMP.enableWordWrapping = true;
        txtTMP.overflowMode       = TextOverflowModes.Truncate;
        var txtRT = txtTf.GetComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = new Vector2(24f, 16f);
        txtRT.offsetMax = new Vector2(-24f, -(SPEAKER_FONTSIZE + 20f));

        // ⑤ Reflection으로 private 필드 주입
        SetField(dui, "panel",        panel.gameObject);
        SetField(dui, "speakerText",  spTMP);
        SetField(dui, "dialogueText", txtTMP);

        // ⑥ 패널 비활성 (Show()가 켜줌)
        panel.gameObject.SetActive(false);

        // ⑦ 클릭 영역
        EnsureClickArea(panel);

        Debug.Log("[Bootstrap] DialogueUI 보강 완료");
    }

    // ══════════════════════════════════════════════════════════════════════
    //  기존 ChoiceUI 보강
    // ══════════════════════════════════════════════════════════════════════
    void RepairChoiceUI(ChoiceUI cui, TMP_FontAsset font)
    {
        Transform cpTf = cui.transform;

        // 선택지 패널 RectTransform — 화면 중앙
        var rt = cpTf.GetComponent<RectTransform>();
        if (rt != null)
        {
            int btnCount = cpTf.childCount > 0 ? cpTf.childCount : 3;
            float totalH = btnCount * CHOICE_BTN_H + (btnCount - 1) * CHOICE_BTN_GAP + 40f;
            rt.anchorMin         = new Vector2(0.5f, 0.5f);
            rt.anchorMax         = new Vector2(0.5f, 0.5f);
            rt.pivot             = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition  = Vector2.zero;
            rt.sizeDelta         = new Vector2(CHOICE_BTN_W + 60f, totalH);
        }

        // 패널 배경
        var img = cpTf.GetComponent<Image>() ?? cpTf.gameObject.AddComponent<Image>();
        img.color = new Color(0.03f, 0.03f, 0.10f, 0.90f);

        // 각 버튼 보강
        int idx = 0;
        foreach (Transform child in cpTf)
        {
            var btn = child.GetComponent<Button>();
            if (btn == null) continue;

            // 버튼 크기 & 위치
            var bRT = child.GetComponent<RectTransform>();
            if (bRT != null)
            {
                float yPos = (idx - 1) * -(CHOICE_BTN_H + CHOICE_BTN_GAP);
                bRT.anchoredPosition = new Vector2(0f, yPos);
                bRT.sizeDelta        = new Vector2(CHOICE_BTN_W, CHOICE_BTN_H);
            }

            // 버튼 배경 색상
            var btnImg = child.GetComponent<Image>() ?? child.gameObject.AddComponent<Image>();
            btnImg.color = BtnBG;

            // 버튼 내 Label 폰트 크기
            var label = child.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null)
            {
                if (font != null) label.font = font;
                label.fontSize  = CHOICE_FONTSIZE;
                label.color     = Color.white;
                label.alignment = TextAlignmentOptions.Center;
                var lRT = label.GetComponent<RectTransform>();
                if (lRT != null)
                {
                    lRT.anchorMin = Vector2.zero;
                    lRT.anchorMax = Vector2.one;
                    lRT.offsetMin = new Vector2(16f, 8f);
                    lRT.offsetMax = new Vector2(-16f, -8f);
                }
            }
            idx++;
        }

        Debug.Log("[Bootstrap] ChoiceUI 보강 완료");
    }

    // ══════════════════════════════════════════════════════════════════════
    //  UI 전체 런타임 생성
    // ══════════════════════════════════════════════════════════════════════
    void BuildFullUI(TMP_FontAsset font)
    {
        var canvasGO = new GameObject("DialogueCanvas_Runtime");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(REF_W, REF_H);
        scaler.matchWidthOrHeight  = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // 대화 패널
        var panelGO  = new GameObject("DialoguePanel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        panelGO.AddComponent<Image>().color = PanelBG;
        var panelRT  = panelGO.GetComponent<RectTransform>();
        panelRT.anchorMin        = new Vector2(0f, 0f);
        panelRT.anchorMax        = new Vector2(1f, 0f);
        panelRT.pivot            = new Vector2(0.5f, 0f);
        panelRT.anchoredPosition = new Vector2(0f, PANEL_MARGIN);
        panelRT.sizeDelta        = new Vector2(-PANEL_MARGIN * 2f, PANEL_HEIGHT);

        // 화자 이름
        var spGO = new GameObject("SpeakerName");
        spGO.transform.SetParent(panelGO.transform, false);
        var spTMP = spGO.AddComponent<TextMeshProUGUI>();
        if (font != null) spTMP.font = font;
        spTMP.fontSize = SPEAKER_FONTSIZE; spTMP.fontStyle = FontStyles.Bold; spTMP.color = SpeakerColor;
        var spRT = spGO.GetComponent<RectTransform>();
        spRT.anchorMin = new Vector2(0f, 1f); spRT.anchorMax = new Vector2(0.5f, 1f);
        spRT.pivot = new Vector2(0f, 0f);
        spRT.anchoredPosition = new Vector2(24f, 4f);
        spRT.sizeDelta = new Vector2(0f, SPEAKER_FONTSIZE + 8f);

        // 대화 텍스트
        var txtGO = new GameObject("DialogueText");
        txtGO.transform.SetParent(panelGO.transform, false);
        var txtTMP = txtGO.AddComponent<TextMeshProUGUI>();
        if (font != null) txtTMP.font = font;
        txtTMP.fontSize = DIALOGUE_FONTSIZE; txtTMP.color = DialogColor;
        txtTMP.lineSpacing = 6f; txtTMP.enableWordWrapping = true;
        txtTMP.overflowMode = TextOverflowModes.Truncate;
        var txtRT = txtGO.GetComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero; txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = new Vector2(24f, 16f);
        txtRT.offsetMax = new Vector2(-24f, -(SPEAKER_FONTSIZE + 20f));

        var dui = panelGO.AddComponent<DialogueUI>();
        SetField(dui, "panel", panelGO); SetField(dui, "speakerText", spTMP); SetField(dui, "dialogueText", txtTMP);
        panelGO.SetActive(false);
        CreatedDialogueUI = dui;

        EnsureClickArea(panelGO.transform);

        // 선택지 패널
        BuildChoicePanel(canvasGO.transform, font);

        Debug.Log("[Bootstrap] 대화창 런타임 생성 완료");
    }

    void BuildChoicePanel(Transform parent, TMP_FontAsset font)
    {
        float totalH = 3 * CHOICE_BTN_H + 2 * CHOICE_BTN_GAP + 40f;
        var cpGO = new GameObject("ChoicePanel");
        cpGO.transform.SetParent(parent, false);
        cpGO.AddComponent<Image>().color = new Color(0.03f, 0.03f, 0.10f, 0.90f);
        var cpRT = cpGO.GetComponent<RectTransform>();
        cpRT.anchorMin = new Vector2(0.5f, 0.5f); cpRT.anchorMax = new Vector2(0.5f, 0.5f);
        cpRT.pivot = new Vector2(0.5f, 0.5f);
        cpRT.anchoredPosition = Vector2.zero;
        cpRT.sizeDelta = new Vector2(CHOICE_BTN_W + 60f, totalH);
        cpGO.SetActive(false);

        for (int i = 0; i < 3; i++)
        {
            float yPos = (1 - i) * (CHOICE_BTN_H + CHOICE_BTN_GAP);
            var btnGO = new GameObject($"ChoiceButton_{i}");
            btnGO.transform.SetParent(cpGO.transform, false);
            btnGO.AddComponent<Image>().color = BtnBG;
            btnGO.AddComponent<Button>();
            var bRT = btnGO.GetComponent<RectTransform>();
            bRT.anchoredPosition = new Vector2(0f, yPos);
            bRT.sizeDelta = new Vector2(CHOICE_BTN_W, CHOICE_BTN_H);

            var lblGO = new GameObject("Label");
            lblGO.transform.SetParent(btnGO.transform, false);
            var lbl = lblGO.AddComponent<TextMeshProUGUI>();
            if (font != null) lbl.font = font;
            lbl.fontSize = CHOICE_FONTSIZE; lbl.color = Color.white;
            lbl.alignment = TextAlignmentOptions.Center;
            lbl.enableWordWrapping = true;
            var lRT = lblGO.GetComponent<RectTransform>();
            lRT.anchorMin = Vector2.zero; lRT.anchorMax = Vector2.one;
            lRT.offsetMin = new Vector2(16f, 8f); lRT.offsetMax = new Vector2(-16f, -8f);
        }

        CreatedChoiceUI = cpGO.AddComponent<ChoiceUI>();
    }

    // ══════════════════════════════════════════════════════════════════════
    //  헬퍼
    // ══════════════════════════════════════════════════════════════════════
    void EnsureClickArea(Transform parent)
    {
        if (parent.Find("DialogueClickArea") != null) return;
        var go = new GameObject("DialogueClickArea");
        go.transform.SetParent(parent, false);
        go.AddComponent<Image>().color = Color.clear;
        var btn = go.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;
        btn.onClick.AddListener(() => DialogueManager.Instance?.OnScreenTapped());
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        go.AddComponent<DialogueClickHandler>();
    }

    void ApplyFontToAll(TMP_FontAsset font)
    {
        var all = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var t in all) t.font = font;
        Debug.Log($"[Bootstrap] TMP {all.Length}개 → NotoSansKR-Regular 적용");
    }

    void FixCanvasScalers()
    {
        var scalers = FindObjectsByType<CanvasScaler>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var s in scalers)
        {
            if (s.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                s.referenceResolution = new Vector2(REF_W, REF_H);
                s.matchWidthOrHeight  = 0.5f;
            }
        }
    }

    static void SetField(object target, string name, object value)
    {
        var f = target.GetType().GetField(name,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (f != null) f.SetValue(target, value);
        else Debug.LogWarning($"[Bootstrap] 필드 '{name}' 없음");
    }

    // ══════════════════════════════════════════════════════════════════════
    //  핵심 싱글톤 자동 생성
    //  씬을 직접 Play 할 때 MainMenu를 거치지 않아도 동작하도록 보장
    // ══════════════════════════════════════════════════════════════════════
    static void EnsureManagers()
    {
        EnsureManager<StatManager>("StatManager");
        EnsureManager<GameManager>("GameManager");
        EnsureManager<ScreenEffects>("ScreenEffects");
        // BGMController·PostProcessingController는 AudioSource/Volume 의존성이 있어
        // 없으면 경고만 출력 (기능 없어도 대화 진행에는 무관)
        if (FindFirstObjectByType<BGMController>() == null)
            Debug.Log("[Bootstrap] BGMController 없음 (BGM 없이 진행)");
    }

    static void EnsureManager<T>(string goName) where T : MonoBehaviour
    {
        if (FindFirstObjectByType<T>(FindObjectsInactive.Include) != null) return;
        var go = new GameObject(goName);
        go.AddComponent<T>();
        DontDestroyOnLoad(go);
        Debug.Log($"[Bootstrap] {goName} 자동 생성");
    }
}
