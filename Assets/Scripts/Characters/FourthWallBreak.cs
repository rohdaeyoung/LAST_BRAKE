using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 모든 엔딩의 마지막 장면 — 두 단계로 구성
///
/// [Phase 1] 메시지 화면 (기존 엔딩 씬)
///   BGM 페이드 → 도윤 응시 애니메이션 → 흑백화 → CORE_MESSAGE 페이드인 → 암전
///
/// [Phase 2] 결말 화면 (런타임 빌드)
///   수치 바(판단력/위험도/의존도) 공개 → 캐릭터 초상 → "이래도 계속 하겠습니까?" → 선택지 3개
///     ① 다시 시작한다
///     ② 누군가에게 연락한다
///     ③ 게임 종료
/// </summary>
public class FourthWallBreak : MonoBehaviour
{
    [Header("Phase 1 — 도윤 스프라이트 (선택)")]
    [SerializeField] private SpriteRenderer doyunRenderer;
    [SerializeField] private Sprite[]       stareFrames;
    [SerializeField] private float          frameInterval      = 0.18f;

    [Header("Phase 1 — UI")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private CanvasGroup     messageCanvasGroup;

    [Header("Phase 1 — 타이밍")]
    [SerializeField] private float bgmFadeOutDuration    = 0.7f;
    [SerializeField] private float desaturateDuration    = 0.8f;
    [SerializeField] private float messageFadeInDuration = 0.4f;
    [SerializeField] private float messageHoldDuration   = 0.9f;  // 메시지 유지 시간
    [SerializeField] private float transitionDuration    = 0.4f;  // 암전 전환 (+ 0.3 대기 = 0.7s)

    [Header("Phase 2 — 결말 화면 (Inspector 연결 없어도 런타임 빌드)")]
    [SerializeField] private Sprite doyunPortrait;         // 캐릭터 초상 (없으면 텍스트만)
    [SerializeField] private Sprite finalScreenBackground; // 최종 화면 배경 (미설정 시 씬 배경 자동 사용)

    private const string CORE_MESSAGE =
        "중독은 특별한 사람이 아니라,\n반복된 선택에서 시작됩니다.";
    private const string FINAL_QUESTION =
        "이래도 계속 하겠습니까?";
    private const string CONTACT_URL =
        "tel:1393";  // 정신건강위기상담전화 1393

    // Phase 2 런타임 오브젝트
    private GameObject  finalScreen;
    private Button      btnRestart;
    private Button      btnContact;
    private Button      btnQuit;

    private void Start()
    {
        AutoWirePhase1Refs();
        if (messageCanvasGroup != null) messageCanvasGroup.alpha = 0f;

        // 씬에 이미 있는 선택지 패널 / 기존 FinalChoicePanel 숨기기 (Phase 2로 대체)
        foreach (var name in new[] { "FinalChoicePanel", "ChoicePanel", "ButtonPanel" })
        {
            var go = SceneFind(name);
            if (go != null) go.SetActive(false);
        }

        StartCoroutine(FourthWallSequence());
    }

    // ══════════════════════════════════════════════════════════════════════
    //  메인 시퀀스
    // ══════════════════════════════════════════════════════════════════════
    private IEnumerator FourthWallSequence()
    {
        // ── Phase 1 ───────────────────────────────────────────────────────
        BGMController.Instance?.FadeOut(bgmFadeOutDuration);
        yield return new WaitForSeconds(bgmFadeOutDuration);

        yield return PlayStareAnimation();

        PostProcessingController.Instance?.DesaturateScreen(desaturateDuration);
        yield return new WaitForSeconds(desaturateDuration);

        // CORE_MESSAGE 페이드인
        if (messageText != null) messageText.text = CORE_MESSAGE;
        yield return FadeCanvasGroup(messageCanvasGroup, 0f, 1f, messageFadeInDuration);

        // 메시지 유지
        yield return new WaitForSeconds(messageHoldDuration);

        // 암전 전환
        ScreenEffects.Instance?.FadeOut(transitionDuration);
        yield return new WaitForSeconds(transitionDuration + 0.3f);

        // ── Phase 2 ───────────────────────────────────────────────────────
        BuildFinalScreen();

        // FadeIn
        ScreenEffects.Instance?.FadeIn(0.8f);
        yield return new WaitForSeconds(0.8f);

        // 수치 공개
        yield return RevealStats();

        // "이래도 계속 하겠습니까?" 등장
        yield return ShowFinalQuestion();

        // 선택지 등장
        yield return new WaitForSeconds(0.5f);
        ShowChoices();
    }

    // ══════════════════════════════════════════════════════════════════════
    //  Phase 1 헬퍼
    // ══════════════════════════════════════════════════════════════════════
    private void AutoWirePhase1Refs()
    {
        if (doyunRenderer == null)
        {
            var go = SceneFind("Doyun_Character");
            if (go != null) doyunRenderer = go.GetComponent<SpriteRenderer>();
        }

        if (messageText == null)
        {
            foreach (var name in new[] { "CoreMessage", "MessageText" })
            {
                var go = SceneFind(name);
                if (go != null) { messageText = go.GetComponent<TextMeshProUGUI>(); break; }
            }
        }
        if (messageText == null)
        {
            var all = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var t in all)
                if (t.GetComponentInParent<Button>() == null) { messageText = t; break; }
        }

        if (messageCanvasGroup == null)
        {
            var go = SceneFind("MessageGroup");
            if (go != null) messageCanvasGroup = go.GetComponent<CanvasGroup>();
        }
        if (messageCanvasGroup == null && messageText != null)
            messageCanvasGroup = messageText.GetComponentInParent<CanvasGroup>(true);
    }

    private IEnumerator PlayStareAnimation()
    {
        if (doyunRenderer == null || stareFrames == null || stareFrames.Length == 0)
        { yield return new WaitForSeconds(0.5f); yield break; }

        foreach (var frame in stareFrames)
        {
            if (frame != null) doyunRenderer.sprite = frame;
            yield return new WaitForSeconds(frameInterval);
        }
        if (stareFrames[stareFrames.Length - 1] != null) doyunRenderer.sprite = stareFrames[stareFrames.Length - 1];
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        if (cg == null) yield break;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        cg.alpha = to;
    }

    // ══════════════════════════════════════════════════════════════════════
    //  Phase 2 — 결말 화면 런타임 빌드
    // ══════════════════════════════════════════════════════════════════════
    // ── 대화창 스타일 디자인 상수 (DialogueBootstrap과 동일) ─────────────
    static readonly Color S_PanelBG     = new Color(0.05f, 0.05f, 0.13f, 0.93f);
    static readonly Color S_SpeakerCol  = new Color(1f,    0.85f, 0.30f, 1f);
    static readonly Color S_BtnBG       = new Color(0.10f, 0.10f, 0.22f, 1f);
    static readonly Color S_BtnHover    = new Color(0.18f, 0.18f, 0.38f, 1f);
    static readonly Color S_BarBG       = new Color(0.12f, 0.12f, 0.20f, 1f);
    const float S_Margin   = 30f;
    const float S_BtnH     = 80f;
    const float S_BtnGap   = 12f;
    const float S_FontMain = 32f;
    const float S_FontSub  = 26f;

    private void BuildFinalScreen()
    {
        // ── Canvas ───────────────────────────────────────────────────────
        finalScreen = new GameObject("FinalScreen");
        var canvas = finalScreen.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 900;
        var scaler = finalScreen.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;
        finalScreen.AddComponent<GraphicRaycaster>();

        // ── 전체 배경 (씬 배경 스프라이트 재사용 → 스토리 분위기 유지) ─────
        var bg = MakeImage(finalScreen.transform, "BG", Color.black);
        Stretch(bg.GetComponent<RectTransform>());

        // 씬 배경 스프라이트 탐색: Inspector 할당 우선, 없으면 씬에서 자동 탐색
        Sprite bgSprite = finalScreenBackground;
        if (bgSprite == null)
            bgSprite = FindSceneBackgroundSprite();

        if (bgSprite != null)
        {
            bg.sprite         = bgSprite;
            bg.type           = Image.Type.Simple;
            bg.preserveAspect = false;   // 화면 전체 채우기
            bg.color          = new Color(1f, 1f, 1f, 0.85f);  // 약간 어둡게
        }
        else
        {
            bg.color = new Color(0.04f, 0.04f, 0.10f, 1f);  // 폴백: 짙은 남색
        }

        // 반투명 그라디언트 오버레이 — 가독성 확보
        var overlay = MakeImage(finalScreen.transform, "DarkOverlay", new Color(0f, 0f, 0f, 0.55f));
        Stretch(overlay.GetComponent<RectTransform>());

        // ── ① 수치 패널 (오른쪽 상단, 대화창 스타일) ─────────────────────
        //   화면 우측 60% 영역 상단에 배치 (캐릭터 왼쪽 공간 확보)
        var statCard = MakePanelCard(finalScreen.transform, "StatCard",
            anchorMin:   new Vector2(0.38f, 1f),
            anchorMax:   new Vector2(0.98f, 1f),
            pivot:       new Vector2(0.5f, 1f),
            anchoredPos: new Vector2(0f, -S_Margin),
            size:        new Vector2(0f, 230f));

        // 타이틀 "— 게임 결과 —"
        var titleGO = new GameObject("StatTitle");
        titleGO.transform.SetParent(statCard.transform, false);
        var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        titleTMP.text      = "— 게임 결과 —";
        titleTMP.fontSize  = S_FontSub;
        titleTMP.color     = S_SpeakerCol;
        titleTMP.alignment = TextAlignmentOptions.Center;
        var titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0f, 1f); titleRT.anchorMax = new Vector2(1f, 1f);
        titleRT.pivot     = new Vector2(0.5f, 1f);
        titleRT.anchoredPosition = new Vector2(0f, -12f);
        titleRT.sizeDelta        = new Vector2(-S_Margin * 2, 36f);

        // 수치 바 3개
        BuildStatBars(statCard.transform);

        // ── ② 캐릭터 이미지 패널 (화면 왼쪽, 세로형) ─────────────────────
        BuildCharacterPanel(finalScreen.transform);

        // ── ③ 대화창 — "이래도 계속 하겠습니까?" (화면 하단, 기존 스타일) ─
        var dialogPanel = MakePanelCard(finalScreen.transform, "FinalDialogPanel",
            anchorMin:   new Vector2(0f, 0f),
            anchorMax:   new Vector2(1f, 0f),
            pivot:       new Vector2(0.5f, 0f),
            anchoredPos: new Vector2(0f, S_Margin),
            size:        new Vector2(-S_Margin * 2, 170f));

        // 화자명 "도윤"
        var spkGO = new GameObject("SpeakerName");
        spkGO.transform.SetParent(dialogPanel.transform, false);
        var spkTMP = spkGO.AddComponent<TextMeshProUGUI>();
        spkTMP.text      = "도윤";
        spkTMP.fontSize  = S_FontSub;
        spkTMP.fontStyle = FontStyles.Bold;
        spkTMP.color     = S_SpeakerCol;
        var spkRT = spkGO.GetComponent<RectTransform>();
        spkRT.anchorMin = new Vector2(0f, 1f); spkRT.anchorMax = new Vector2(0.5f, 1f);
        spkRT.pivot     = new Vector2(0f, 0f);
        spkRT.anchoredPosition = new Vector2(24f, 4f);
        spkRT.sizeDelta        = new Vector2(0f, S_FontSub + 8f);

        // 대화 텍스트
        var qGO = new GameObject("FinalQuestion");
        qGO.transform.SetParent(dialogPanel.transform, false);
        var qTMP = qGO.AddComponent<TextMeshProUGUI>();
        qTMP.text             = FINAL_QUESTION;
        qTMP.fontSize         = S_FontMain;
        qTMP.color            = Color.white;
        qTMP.textWrappingMode = TextWrappingModes.Normal;
        var qRT = qGO.GetComponent<RectTransform>();
        qRT.anchorMin = Vector2.zero; qRT.anchorMax = Vector2.one;
        qRT.offsetMin = new Vector2(24f, 16f);
        qRT.offsetMax = new Vector2(-24f, -(S_FontSub + 20f));

        // 대화 패널 전체를 CanvasGroup으로 페이드 제어
        var dialogCG = dialogPanel.AddComponent<CanvasGroup>();
        dialogCG.alpha = 0f;
        _finalQuestionCG = dialogCG;

        // ── ④ 선택지 (대화창 바로 위, 기존 ChoiceUI 스타일) ───────────────
        var choicePanel = BuildChoicePanel(finalScreen.transform);
        choicePanel.SetActive(false);
        _choicePanel = choicePanel;
    }

    // ── 캐릭터 이미지 패널 (화면 왼쪽 35%) ─────────────────────────────
    private void BuildCharacterPanel(Transform parent)
    {
        // 외곽 프레임 패널 (어두운 테두리)
        var frame = new GameObject("CharacterFrame");
        frame.transform.SetParent(parent, false);
        var frameImg = frame.AddComponent<Image>();
        frameImg.color = new Color(0.07f, 0.07f, 0.18f, 0.85f);
        var frameRT = frame.GetComponent<RectTransform>();
        // 화면 왼쪽 35%, 하단 대화창(170+S_Margin*2) 위 ~ 상단 약 10% 여백
        frameRT.anchorMin        = new Vector2(0.02f, 0f);
        frameRT.anchorMax        = new Vector2(0.36f, 1f);
        frameRT.offsetMin        = new Vector2(0f,  170f + S_Margin * 2 + 10f);
        frameRT.offsetMax        = new Vector2(0f, -S_Margin * 3);

        // 캐릭터 이미지 (Inspector에서 doyunPortrait 할당 시 표시)
        var charImgGO = new GameObject("CharacterImage");
        charImgGO.transform.SetParent(frame.transform, false);
        var charImg = charImgGO.AddComponent<Image>();
        var charRT  = charImgGO.GetComponent<RectTransform>();
        Stretch(charRT);

        // doyunPortrait Inspector 할당 → 실제 이미지
        // 없으면 Resources 에서 자동 탐색
        Sprite portrait = doyunPortrait;
        if (portrait == null)
            portrait = Resources.Load<Sprite>("Characters/Doyun_Neutral")
                    ?? Resources.Load<Sprite>("Characters/Doyun_Determined")
                    ?? Resources.Load<Sprite>("Sprites/Characters/Doyun_Neutral");

        if (portrait != null)
        {
            charImg.sprite             = portrait;
            charImg.preserveAspect     = true;
            charImg.color              = Color.white;
        }
        else
        {
            // 초상화 없을 때: 반투명 플레이스홀더 + 이름 텍스트
            charImg.color = new Color(0.10f, 0.10f, 0.22f, 0.6f);

            var nameLblGO = new GameObject("CharNameLabel");
            nameLblGO.transform.SetParent(frame.transform, false);
            var nameTMP = nameLblGO.AddComponent<TextMeshProUGUI>();
            nameTMP.text      = "도윤";
            nameTMP.fontSize  = 40f;
            nameTMP.color     = new Color(1f, 1f, 1f, 0.25f);
            nameTMP.alignment = TextAlignmentOptions.Center;
            var nameRT = nameLblGO.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0f, 0.4f);
            nameRT.anchorMax = new Vector2(1f, 0.6f);
            nameRT.offsetMin = nameRT.offsetMax = Vector2.zero;
        }

        // 하단 이름 배지 (골든 테두리 라벨)
        var badgeGO = new GameObject("CharBadge");
        badgeGO.transform.SetParent(frame.transform, false);
        var badgeImg = badgeGO.AddComponent<Image>();
        badgeImg.color = new Color(0.05f, 0.05f, 0.13f, 0.92f);
        var badgeRT = badgeGO.GetComponent<RectTransform>();
        badgeRT.anchorMin        = new Vector2(0f,   0f);
        badgeRT.anchorMax        = new Vector2(1f,   0f);
        badgeRT.pivot            = new Vector2(0.5f, 0f);
        badgeRT.anchoredPosition = new Vector2(0f, 0f);
        badgeRT.sizeDelta        = new Vector2(0f, 48f);

        var badgeTxtGO = new GameObject("BadgeText");
        badgeTxtGO.transform.SetParent(badgeGO.transform, false);
        var badgeTMP = badgeTxtGO.AddComponent<TextMeshProUGUI>();
        badgeTMP.text      = "도윤";
        badgeTMP.fontSize  = 22f;
        badgeTMP.color     = S_SpeakerCol;
        badgeTMP.alignment = TextAlignmentOptions.Center;
        badgeTMP.fontStyle = FontStyles.Bold;
        Stretch(badgeTxtGO.GetComponent<RectTransform>());
    }

    /// <summary>대화창 스타일 패널 카드 생성</summary>
    private static GameObject MakePanelCard(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchoredPos, Vector2 size)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = S_PanelBG;
        var rt  = go.GetComponent<RectTransform>();
        rt.anchorMin        = anchorMin;
        rt.anchorMax        = anchorMax;
        rt.pivot            = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta        = size;
        return go;
    }

    // ── 수치 바 3개 ─────────────────────────────────────────────────────
    private void BuildStatBars(Transform parent)
    {
        string[] statNames = { "Stat_0",    "Stat_1",    "Stat_2" };
        string[] barNames  = { "StatBar_0", "StatBar_1", "StatBar_2" };
        string[] labels    = { "판단력", "위험도", "의존도" };
        // 타이틀 아래 36px 이후 공간에 3개 행 배치 (y: 0~160px)
        float rowH   = 52f;
        float startY = -68f;  // 타이틀(36) + 패딩(12) + 절반행높이

        for (int i = 0; i < 3; i++)
        {
            float cy = startY - rowH * i;

            // 행 컨테이너
            var row = new GameObject($"StatRow_{i}");
            row.transform.SetParent(parent, false);
            var rowRT = GetOrAddRT(row);
            rowRT.anchorMin        = new Vector2(0f,   0.5f);
            rowRT.anchorMax        = new Vector2(1f,   0.5f);
            rowRT.pivot            = new Vector2(0.5f, 0.5f);
            rowRT.anchoredPosition = new Vector2(0f, cy);
            rowRT.sizeDelta        = new Vector2(-S_Margin * 2, rowH - 8f);

            // 라벨 (왼쪽 20%)
            var lblGO = new GameObject(statNames[i]);
            lblGO.transform.SetParent(row.transform, false);
            var lbl = lblGO.AddComponent<TextMeshProUGUI>();
            lbl.text      = labels[i] + "  ???";
            lbl.fontSize  = S_FontSub;
            lbl.color     = Color.gray;
            lbl.alignment = TextAlignmentOptions.Left;
            var lblRT = lblGO.GetComponent<RectTransform>();
            lblRT.anchorMin = new Vector2(0f, 0f); lblRT.anchorMax = new Vector2(0.22f, 1f);
            lblRT.offsetMin = new Vector2(4f, 0f); lblRT.offsetMax = Vector2.zero;

            // 슬라이더 (오른쪽 78%)
            var barGO = new GameObject(barNames[i]);
            barGO.transform.SetParent(row.transform, false);
            var slider = barGO.AddComponent<Slider>();
            slider.minValue = 0f; slider.maxValue = 1f; slider.value = 0f;
            var barRT = barGO.GetComponent<RectTransform>();
            barRT.anchorMin = new Vector2(0.23f, 0.15f);
            barRT.anchorMax = new Vector2(1f,    0.85f);
            barRT.offsetMin = barRT.offsetMax = Vector2.zero;

            // 배경
            var bgGO  = new GameObject("Background");
            bgGO.transform.SetParent(barGO.transform, false);
            var bgImg = bgGO.AddComponent<Image>();
            bgImg.color = S_BarBG;
            slider.targetGraphic = bgImg;
            Stretch(bgGO.GetComponent<RectTransform>());

            // Fill Area
            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(barGO.transform, false);
            Stretch(GetOrAddRT(fillArea));

            // Fill
            var fillGO  = new GameObject("Fill");
            fillGO.transform.SetParent(fillArea.transform, false);
            var fillImg = fillGO.AddComponent<Image>();
            fillImg.color = Color.gray;
            Stretch(fillGO.GetComponent<RectTransform>());
            slider.fillRect = fillGO.GetComponent<RectTransform>();
        }

        // StatReportUI 추가 — AutoWireRefs()로 위 오브젝트들 자동 연결
        if (StatReportUI.Instance == null)
            parent.gameObject.AddComponent<StatReportUI>();
    }

    // ── 선택지 패널 ─────────────────────────────────────────────────────
    private GameObject BuildChoicePanel(Transform parent)
    {
        const float btnW   = 860f;
        const float totalH = S_BtnH * 3 + S_BtnGap * 2 + S_Margin * 2;

        // 패널 — 대화창(170) + margin(10) 위
        var panel = new GameObject("FinalChoicePanel");
        panel.transform.SetParent(parent, false);
        var pImg = panel.AddComponent<Image>();
        pImg.color = S_PanelBG;
        var pRT  = panel.GetComponent<RectTransform>();
        pRT.anchorMin        = new Vector2(0.5f, 0f);
        pRT.anchorMax        = new Vector2(0.5f, 0f);
        pRT.pivot            = new Vector2(0.5f, 0f);
        pRT.anchoredPosition = new Vector2(0f, 170f + S_Margin + 10f);
        pRT.sizeDelta        = new Vector2(btnW + S_Margin * 2, totalH);

        string[] btnLabels = { "다시 시작한다", "누군가에게 연락한다", "게임 종료" };
        string[] prefixes  = { "①", "②", "③" };

        for (int i = 0; i < 3; i++)
        {
            float yPos = S_Margin + (2 - i) * (S_BtnH + S_BtnGap);

            var btnGO  = new GameObject($"ChoiceBtn_{i}");
            btnGO.transform.SetParent(panel.transform, false);
            var btnImg = btnGO.AddComponent<Image>();
            btnImg.color = S_BtnBG;
            var btn    = btnGO.AddComponent<Button>();

            // 호버 색상
            var cs        = btn.colors;
            cs.normalColor      = S_BtnBG;
            cs.highlightedColor = S_BtnHover;
            cs.pressedColor     = S_BtnHover * 0.8f;
            cs.selectedColor    = S_BtnBG;
            btn.colors          = cs;

            var bRT = btnGO.GetComponent<RectTransform>();
            bRT.anchorMin        = new Vector2(0.5f, 0f);
            bRT.anchorMax        = new Vector2(0.5f, 0f);
            bRT.pivot            = new Vector2(0.5f, 0f);
            bRT.anchoredPosition = new Vector2(0f, yPos);
            bRT.sizeDelta        = new Vector2(btnW, S_BtnH);

            // 번호 (왼쪽)
            var numGO  = new GameObject("Prefix");
            numGO.transform.SetParent(btnGO.transform, false);
            var numTMP = numGO.AddComponent<TextMeshProUGUI>();
            numTMP.text      = prefixes[i];
            numTMP.fontSize  = S_FontSub;
            numTMP.color     = S_SpeakerCol;
            numTMP.alignment = TextAlignmentOptions.Center;
            var numRT = numGO.GetComponent<RectTransform>();
            numRT.anchorMin = new Vector2(0f, 0f); numRT.anchorMax = new Vector2(0.08f, 1f);
            numRT.offsetMin = numRT.offsetMax = Vector2.zero;

            // 텍스트
            var lblGO  = new GameObject("Label");
            lblGO.transform.SetParent(btnGO.transform, false);
            var lblTMP = lblGO.AddComponent<TextMeshProUGUI>();
            lblTMP.text      = btnLabels[i];
            lblTMP.fontSize  = S_FontMain;
            lblTMP.color     = Color.white;
            lblTMP.alignment = TextAlignmentOptions.Left;
            var lRT = lblGO.GetComponent<RectTransform>();
            lRT.anchorMin = new Vector2(0.09f, 0f); lRT.anchorMax = Vector2.one;
            lRT.offsetMin = new Vector2(0f, 6f);    lRT.offsetMax = new Vector2(-12f, -6f);

            int idx = i;
            btn.onClick.AddListener(() => OnChoiceSelected(idx));

            if (i == 0) btnRestart = btn;
            else if (i == 1) btnContact = btn;
            else btnQuit = btn;
        }
        return panel;
    }

    // Phase 2 코루틴에서 참조
    private CanvasGroup _finalQuestionCG;
    private GameObject  _choicePanel;

    private IEnumerator RevealStats()
    {
        if (StatReportUI.Instance == null || StatManager.Instance == null)
        { yield return new WaitForSeconds(1f); yield break; }

        bool done = false;
        StatReportUI.Instance.RevealStats(
            StatManager.Instance.INT,
            StatManager.Instance.RISK,
            StatManager.Instance.ADDICT,
            () => done = true);

        float timeout = 12f;
        while (!done && timeout > 0f) { timeout -= Time.deltaTime; yield return null; }
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator ShowFinalQuestion()
    {
        yield return FadeCanvasGroup(_finalQuestionCG, 0f, 1f, 1.0f);
        yield return new WaitForSeconds(0.8f);
    }

    private void ShowChoices()
    {
        if (_choicePanel != null) _choicePanel.SetActive(true);
    }

    // ══════════════════════════════════════════════════════════════════════
    //  선택지 처리
    // ══════════════════════════════════════════════════════════════════════
    private void OnChoiceSelected(int index)
    {
        PostProcessingController.Instance?.ResetEffects();
        switch (index)
        {
            case 0: OnRestart();  break;
            case 1: OnContact();  break;
            case 2: OnQuit();     break;
        }
    }

    private void OnRestart()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.StartNewGame();
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("00_MainMenu");
    }

    private void OnContact()
    {
        // 정신건강위기상담전화 1393 (한국 자살·위기상담 전화)
        Application.OpenURL(CONTACT_URL);
    }

    private void OnQuit()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.QuitGame();
        else
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  유틸
    // ══════════════════════════════════════════════════════════════════════
    private static Image MakeImage(Transform parent, string name, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        return img;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    /// <summary>
    /// UI 계층 아래 생성된 GameObject는 AddComponent&lt;RectTransform&gt;()이
    /// "already added" 오류를 낸다. 기존 것을 반환하고 없으면 추가.
    /// </summary>
    private static RectTransform GetOrAddRT(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        if (rt == null) rt = go.AddComponent<RectTransform>();
        return rt;
    }

    private static GameObject SceneFind(string goName)
    {
        var all = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var t in all)
            if (t.name == goName) return t.gameObject;
        return null;
    }

    /// <summary>
    /// 씬에 있는 배경 Image 스프라이트를 자동 탐색.
    /// "Background", "BG_" 로 시작하는 이름 우선, 없으면 전체화면 크기의 Image 중 첫 번째 사용.
    /// </summary>
    private static Sprite FindSceneBackgroundSprite()
    {
        var images = FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        // 1순위: 이름이 BG로 시작하거나 Background인 Image
        foreach (var img in images)
        {
            if (img.sprite == null) continue;
            string n = img.gameObject.name;
            if (n.StartsWith("BG") || n.StartsWith("Background") || n.Contains("background"))
                return img.sprite;
        }

        // 2순위: SpriteRenderer (씬에 배치된 배경 스프라이트)
        var renderers = FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var sr in renderers)
        {
            if (sr.sprite == null) continue;
            string n = sr.gameObject.name;
            if (n.StartsWith("BG") || n.StartsWith("Background"))
                return sr.sprite;
        }

        // 3순위: 앵커가 full-stretch인 Image 중 스프라이트 있는 것
        foreach (var img in images)
        {
            if (img.sprite == null) continue;
            var rt = img.GetComponent<RectTransform>();
            if (rt != null && rt.anchorMin == Vector2.zero && rt.anchorMax == Vector2.one)
                return img.sprite;
        }

        return null;
    }
}
