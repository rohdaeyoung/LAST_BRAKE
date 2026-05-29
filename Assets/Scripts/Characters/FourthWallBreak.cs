using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 모든 엔딩의 마지막 장면 — 세 단계
///
/// [Phase 1] 메시지 화면
///   BGM 페이드 → 전신 응시 애니메이션 → 흑백화 → 메시지 → 암전
///
/// [Phase FACE] 얼굴 응시 (TrueEnd 전용)
///   도윤 얼굴이 화면 가득 채우며 플레이어를 바라봄
///   글리치 + 비네트 + 눈 깜빡임 → 탭하면 Phase 2로
///
/// [Phase 2] 결말 화면
///   스탯 공개 → 얼굴(엔딩별 표정) → "이래도 계속 하겠습니까?" → 선택지
///
/// 엔딩별 얼굴 표정:
///   07 GoodEnd   → faceNormal   (회복, 차분)
///   08 NormalEnd → faceAnxious  (불안, 혼란)
///   09 BadEnd    → faceRegret   (후회, 고통)
///   10 TrueEnd   → faceSurprised (응시, 4th wall)
/// </summary>
public class FourthWallBreak : MonoBehaviour
{
    // ── Phase 1 ──────────────────────────────────────────────────
    [Header("Phase 1 — 전신 응시")]
    [SerializeField] private SpriteRenderer  doyunRenderer;
    [SerializeField] private Sprite[]        stareFrames;
    [SerializeField] private float           frameInterval      = 0.18f;

    [Header("Phase 1 — 메시지 UI")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private CanvasGroup     messageCanvasGroup;

    [Header("Phase 1 — 타이밍")]
    [SerializeField] private float bgmFadeOutDuration    = 0.7f;
    [SerializeField] private float desaturateDuration    = 0.8f;
    [SerializeField] private float messageFadeInDuration = 0.4f;
    [SerializeField] private float messageHoldDuration   = 0.9f;
    [SerializeField] private float transitionDuration    = 0.4f;

    // ── Phase FACE / Phase 2 공통 스프라이트 ─────────────────────
    [Header("얼굴 스프라이트 (EffectLinker 자동 연결)")]
    [SerializeField] public Sprite faceNormal;     // GoodEnd
    [SerializeField] public Sprite faceAnxious;    // NormalEnd
    [SerializeField] public Sprite faceSurprised;  // TrueEnd 응시
    [SerializeField] public Sprite faceRegret;     // BadEnd

    [Header("Phase 2 배경 (미설정 시 씬 배경 자동 탐색)")]
    [SerializeField] private Sprite finalScreenBackground;

    // ── 상수 ─────────────────────────────────────────────────────
    private const string CORE_MESSAGE =
        "중독은 특별한 사람이 아니라,\n반복된 선택에서 시작됩니다.";
    private const string FINAL_QUESTION = "이래도 계속 하겠습니까?";
    private const string CONTACT_URL    = "tel:1393";

    // ── 런타임 ───────────────────────────────────────────────────
    private CanvasGroup _finalQuestionCG;
    private GameObject  _choicePanel;
    private EndingKind  _endingKind;

    private enum EndingKind { Good, Normal, Bad, True }

    // ─────────────────────────────────────────────────────────────
    private void Start()
    {
        _endingKind = DetectEndingKind();
        AutoWirePhase1Refs();
        if (messageCanvasGroup != null) messageCanvasGroup.alpha = 0f;

        // 기존 씬의 선택지 패널 숨기기
        foreach (var n in new[] { "FinalChoicePanel", "ChoicePanel", "ButtonPanel" })
        {
            var go = SceneFind(n);
            if (go != null) go.SetActive(false);
        }

        StartCoroutine(MainSequence());
    }

    // ── 엔딩 종류 감지 ───────────────────────────────────────────
    private static EndingKind DetectEndingKind()
    {
        string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (scene.Contains("Good"))   return EndingKind.Good;
        if (scene.Contains("Normal")) return EndingKind.Normal;
        if (scene.Contains("Bad"))    return EndingKind.Bad;
        if (scene.Contains("True") || scene.Contains("10_")) return EndingKind.True;
        return EndingKind.Normal;
    }

    // ── 엔딩별 얼굴 스프라이트 ───────────────────────────────────
    private Sprite GetEndingFace(EndingKind kind)
    {
        return kind switch
        {
            EndingKind.Good   => faceNormal    ?? faceSurprised,
            EndingKind.Normal => faceAnxious   ?? faceNormal,
            EndingKind.Bad    => faceRegret    ?? faceAnxious,
            EndingKind.True   => faceSurprised ?? faceNormal,
            _                 => faceNormal,
        };
    }

    // ══════════════════════════════════════════════════════════════
    //  메인 시퀀스
    // ══════════════════════════════════════════════════════════════
    private IEnumerator MainSequence()
    {
        // ── Phase 1 ───────────────────────────────────────────
        BGMController.Instance?.FadeOut(bgmFadeOutDuration);
        yield return new WaitForSeconds(bgmFadeOutDuration);

        yield return PlayStareAnimation();

        PostProcessingController.Instance?.DesaturateScreen(desaturateDuration);
        SFXManager.Instance?.PlayDesaturate();
        yield return new WaitForSeconds(desaturateDuration);

        if (messageText != null) messageText.text = CORE_MESSAGE;
        yield return FadeCG(messageCanvasGroup, 0f, 1f, messageFadeInDuration);
        SFXManager.Instance?.PlayMessageReveal();
        yield return new WaitForSeconds(messageHoldDuration);

        ScreenEffects.Instance?.FadeOut(transitionDuration);
        yield return new WaitForSeconds(transitionDuration + 0.3f);

        // ── Phase FACE (TrueEnd 전용) ─────────────────────────
        if (_endingKind == EndingKind.True)
            yield return FaceStarePhase();

        // ── Phase 2 ───────────────────────────────────────────
        // 글리치 반드시 끄기 (게임 중 ADDICT≥60에서 켜졌을 수 있음)
        FXManager.Instance?.SetGlitchActive(false);
        FXManager.Instance?.SetVignetteIntensity(0.35f);

        BuildFinalScreen();
        // FadeIn: ScreenEffects가 없어도 결과 화면 즉시 표시
        ScreenEffects.Instance?.FadeIn(0.5f);
        yield return new WaitForSeconds(0.5f);

        // 호흡하는 비네트 효과 시작 (글리치 대신 조용한 분위기 연출)
        StartCoroutine(BreathingVignette());

        // 대화창은 이미 alpha=1로 보임 — 스탯 애니메이션만 진행
        yield return RevealStats();
        yield return ShowFinalQuestion();
        yield return new WaitForSeconds(0.3f);
        if (_choicePanel != null) _choicePanel.SetActive(true);
    }

    // ══════════════════════════════════════════════════════════════
    //  Phase FACE — 얼굴 응시 (TrueEnd)
    // ══════════════════════════════════════════════════════════════
    private bool _tapReceived;

    private IEnumerator FaceStarePhase()
    {
        _tapReceived = false;

        // ── 캔버스 빌드 ───────────────────────────────────────
        var canvasGO = new GameObject("FaceStare_Canvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 950;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // 검정 배경
        var bgImg = MakeImage(canvasGO.transform, "BG", Color.black);
        StretchFull(bgImg.rectTransform);
        bgImg.raycastTarget = false;

        // 얼굴 이미지 (화면보다 약간 크게 → 꽉 찬 느낌)
        var faceGO  = new GameObject("FaceImage");
        faceGO.transform.SetParent(canvasGO.transform, false);
        var faceImg = faceGO.AddComponent<Image>();
        faceImg.sprite         = faceSurprised ?? faceNormal;
        faceImg.color          = new Color(1f, 1f, 1f, 0f);
        faceImg.preserveAspect = true;
        faceImg.raycastTarget  = false;
        var fRT = faceImg.rectTransform;
        fRT.anchorMin = Vector2.zero;
        fRT.anchorMax = Vector2.one;
        fRT.offsetMin = new Vector2(-50f, -50f);
        fRT.offsetMax = new Vector2( 50f,  50f);

        // "탭하여 계속" 힌트
        var hintGO = new GameObject("Hint");
        hintGO.transform.SetParent(canvasGO.transform, false);
        var hintCG  = hintGO.AddComponent<CanvasGroup>();
        hintCG.alpha = 0f;
        var hintTMP = hintGO.AddComponent<TextMeshProUGUI>();
        hintTMP.text          = "탭하여 계속";
        hintTMP.fontSize      = 24f;
        hintTMP.color         = new Color(1f, 1f, 1f, 0.5f);
        hintTMP.alignment     = TextAlignmentOptions.Center;
        hintTMP.raycastTarget = false;
        var hRT = hintGO.GetComponent<RectTransform>();
        hRT.anchorMin        = new Vector2(0f, 0f);
        hRT.anchorMax        = new Vector2(1f, 0f);
        hRT.pivot            = new Vector2(0.5f, 0f);
        hRT.anchoredPosition = new Vector2(0f, 40f);
        hRT.sizeDelta        = new Vector2(0f, 50f);

        // ── 페이드 인 ─────────────────────────────────────────
        ScreenEffects.Instance?.FadeIn(0.6f);
        yield return FadeImg(faceImg, 0f, 1f, 0.6f);
        SFXManager.Instance?.StartTrueEndStare();

        // 글리치 + 비네트 강화
        FXManager.Instance?.SetGlitchActive(true);
        FXManager.Instance?.SetVignetteIntensity(0.9f);

        // ── 응시 루프 ─────────────────────────────────────────
        // 표정 전환 순서 (긴장 고조)
        Sprite[] faceSeq = {
            faceSurprised ?? faceNormal,
            faceSurprised ?? faceNormal,
            faceAnxious   ?? faceNormal,
            faceSurprised ?? faceNormal,
            faceRegret    ?? faceNormal,
        };
        int   seqIdx     = 0;
        float elapsed    = 0f;
        float nextChange = 2.0f;
        float nextBlink  = 1.0f;

        while (!_tapReceived)
        {
            elapsed     += Time.deltaTime;
            nextChange  -= Time.deltaTime;
            nextBlink   -= Time.deltaTime;

            // 탭 / 클릭 감지
            if (Input.GetMouseButtonDown(0) ||
                (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
            {
                SFXManager.Instance?.PlayTrueEndTap();
                SFXManager.Instance?.FadeOutLoop(0.5f);
                _tapReceived = true;
                break;
            }

            // 표정 변화 (점점 빨라짐)
            if (nextChange <= 0f && seqIdx < faceSeq.Length)
            {
                if (faceImg != null && faceSeq[seqIdx] != null)
                    faceImg.sprite = faceSeq[seqIdx];
                seqIdx++;
                nextChange = Mathf.Max(0.5f, 2.0f - seqIdx * 0.25f);
            }

            // 눈 깜빡임
            if (nextBlink <= 0f && faceImg != null)
            {
                StartCoroutine(BlinkEffect(faceImg));
                nextBlink = Random.Range(1.5f, 3.5f);
            }

            // 힌트 페이드인 (2초 후)
            if (hintCG != null && elapsed > 2f)
                hintCG.alpha = Mathf.MoveTowards(hintCG.alpha, 1f, Time.deltaTime * 0.7f);

            // 미세 흔들림
            if (faceImg != null)
            {
                float wobble = Mathf.Sin(elapsed * 1.3f) * 4f;
                fRT.anchoredPosition = new Vector2(wobble, 0f);
            }

            yield return null;
        }

        // ── 종료 처리 ─────────────────────────────────────────
        FXManager.Instance?.SetGlitchActive(false);
        FXManager.Instance?.SetVignetteIntensity(0.25f);
        // 검은 화면에 갇히지 않도록 FadeOut 없이 0.2초 대기 후 바로 Phase 2
        yield return new WaitForSeconds(0.2f);
        Destroy(canvasGO);
    }

    private IEnumerator BlinkEffect(Image img)
    {
        if (img == null) yield break;
        float t = 0f;
        Vector3 orig = img.transform.localScale;
        while (t < 0.06f)
        {
            t += Time.deltaTime;
            img.transform.localScale = new Vector3(orig.x, Mathf.Lerp(1f, 0.90f, t / 0.06f), 1f);
            yield return null;
        }
        yield return new WaitForSeconds(0.04f);
        t = 0f;
        while (t < 0.08f)
        {
            t += Time.deltaTime;
            img.transform.localScale = new Vector3(orig.x, Mathf.Lerp(0.90f, 1f, t / 0.08f), 1f);
            yield return null;
        }
        img.transform.localScale = orig;
    }

    // ══════════════════════════════════════════════════════════════
    //  Phase 1 헬퍼
    // ══════════════════════════════════════════════════════════════
    private void AutoWirePhase1Refs()
    {
        if (doyunRenderer == null)
        {
            var go = SceneFind("Doyun_Character");
            if (go != null) doyunRenderer = go.GetComponent<SpriteRenderer>();
        }
        if (messageText == null)
        {
            foreach (var n in new[] { "CoreMessage", "MessageText" })
            {
                var go = SceneFind(n);
                if (go != null) { messageText = go.GetComponent<TextMeshProUGUI>(); break; }
            }
        }
        if (messageText == null)
        {
            var all = FindObjectsByType<TextMeshProUGUI>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
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
        if (stareFrames[stareFrames.Length - 1] != null)
            doyunRenderer.sprite = stareFrames[stareFrames.Length - 1];
    }

    // ══════════════════════════════════════════════════════════════
    //  Phase 2 — 결말 화면
    // ══════════════════════════════════════════════════════════════
    static readonly Color C_Panel   = new Color(0.05f, 0.05f, 0.13f, 0.93f);
    static readonly Color C_Gold    = new Color(1f,    0.85f, 0.30f, 1f);
    static readonly Color C_Btn     = new Color(0.10f, 0.10f, 0.22f, 1f);
    static readonly Color C_BtnHov  = new Color(0.18f, 0.18f, 0.38f, 1f);
    static readonly Color C_BarBG   = new Color(0.12f, 0.12f, 0.20f, 1f);
    const float M  = 30f;   // margin
    const float BH = 80f;   // button height
    const float BG = 12f;   // button gap
    const float FS = 32f;   // font main
    const float FS2= 26f;   // font sub

    private void BuildFinalScreen()
    {
        var root = new GameObject("FinalScreen");
        var cv   = root.AddComponent<Canvas>();
        cv.renderMode   = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 900;
        var sc = root.AddComponent<CanvasScaler>();
        sc.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920, 1080);
        sc.matchWidthOrHeight  = 0.5f;
        root.AddComponent<GraphicRaycaster>();

        // ── 배경 — 씬 배경 완전 차단, 엔딩 화면만 보임 ──────────
        var bg = MakeImage(root.transform, "BG", Color.black);
        StretchFull(bg.rectTransform);
        bg.raycastTarget = false;
        // Inspector에서 finalScreenBackground를 지정했을 때만 스프라이트 사용
        // (씬 배경 자동 탐색 제거 → 뒤 장면이 비치지 않음)
        if (finalScreenBackground != null)
        {
            bg.sprite = finalScreenBackground;
            bg.color  = new Color(1f, 1f, 1f, 1f);
        }
        else
        {
            // 기본: 진한 네이비 단색 — 완전 불투명
            bg.color = new Color(0.05f, 0.05f, 0.14f, 1f);
        }

        // ── ① 스탯 카드 (우측 상단) ───────────────────────────
        var statCard = MakePanel(root.transform, "StatCard",
            new Vector2(0.38f, 1f), new Vector2(0.98f, 1f),
            new Vector2(0.5f,  1f), new Vector2(0f, -M), new Vector2(0f, 260f));
        BuildStatCardTitle(statCard.transform);
        BuildStatBars(statCard.transform);

        // ── ② 얼굴 패널 (좌측) ────────────────────────────────
        BuildFacePanel(root.transform);

        // ── ③ 대화창 (하단) ────────────────────────────────────
        var dlgPanel = MakePanel(root.transform, "DialogPanel",
            new Vector2(0f, 0f), new Vector2(1f, 0f),
            new Vector2(0.5f, 0f), new Vector2(0f, M), new Vector2(-M * 2, 170f));

        // 화자명 (패널 내부 상단)
        var spk = dlgPanel.transform.AddTMP("Speaker", "도윤", FS2, C_Gold,
            TextAlignmentOptions.Left, FontStyles.Bold);
        var spkRT = spk.rectTransform;
        spkRT.anchorMin        = new Vector2(0f, 1f);   // 상단 기준
        spkRT.anchorMax        = new Vector2(0.5f, 1f);
        spkRT.pivot            = new Vector2(0f, 1f);   // 피벗도 상단
        spkRT.anchoredPosition = new Vector2(24f, -(8f));   // 패널 위에서 8px 아래
        spkRT.sizeDelta        = new Vector2(0f, FS2 + 8f); // (0, 34)

        // 질문 텍스트 (화자명 아래)
        var q = dlgPanel.transform.AddTMP("Question", FINAL_QUESTION, FS, Color.white,
            TextAlignmentOptions.Left, FontStyles.Normal);
        var qRT = q.rectTransform;
        qRT.anchorMin = Vector2.zero; qRT.anchorMax = Vector2.one;
        qRT.offsetMin = new Vector2(24f,  12f);
        qRT.offsetMax = new Vector2(-24f, -(FS2 + 16f));   // 화자명 높이(34)+여백
        q.textWrappingMode = TextWrappingModes.Normal;

        // CanvasGroup으로 지연 페이드 대신 즉시 표시 → 대화창이 항상 보임
        var dlgCG = dlgPanel.AddComponent<CanvasGroup>();
        dlgCG.alpha      = 1f;   // 처음부터 보임
        _finalQuestionCG = dlgCG;

        // ── ④ 선택지 패널 ──────────────────────────────────────
        _choicePanel = BuildChoicePanel(root.transform);
        _choicePanel.SetActive(false);
    }

    // ── 얼굴 패널 ────────────────────────────────────────────────
    private void BuildFacePanel(Transform parent)
    {
        // 프레임 (좌측 34% 영역)
        var frame    = new GameObject("FaceFrame");
        frame.transform.SetParent(parent, false);
        var frameImg = frame.AddComponent<Image>();
        frameImg.color = new Color(0.04f, 0.04f, 0.12f, 0.92f);
        var fRT      = frame.GetComponent<RectTransform>();
        fRT.anchorMin = new Vector2(0.02f, 0f);
        fRT.anchorMax = new Vector2(0.36f, 1f);
        fRT.offsetMin = new Vector2(0f,  170f + M * 2 + 10f);
        fRT.offsetMax = new Vector2(0f, -M * 3);

        // 얼굴 이미지
        var imgGO  = new GameObject("FaceImage");
        imgGO.transform.SetParent(frame.transform, false);
        var img    = imgGO.AddComponent<Image>();
        var iRT    = imgGO.GetComponent<RectTransform>();
        iRT.anchorMin = Vector2.zero;
        iRT.anchorMax = Vector2.one;
        iRT.offsetMin = new Vector2(0f, 48f);   // 하단 배지 공간 확보
        iRT.offsetMax = Vector2.zero;

        Sprite face = GetEndingFace(_endingKind);
        if (face != null)
        {
            img.sprite         = face;
            img.preserveAspect = true;
            img.color          = Color.white;
        }
        else
        {
            img.color = new Color(0.10f, 0.10f, 0.22f, 0.6f);
        }

        // 엔딩별 표정 설명 텍스트 (배지 위)
        string faceLabel = _endingKind switch
        {
            EndingKind.Good   => "도윤",
            EndingKind.Normal => "도윤",
            EndingKind.Bad    => "도윤",
            EndingKind.True   => "도윤",
            _                 => "도윤",
        };

        // 하단 이름 배지
        var badge    = new GameObject("Badge");
        badge.transform.SetParent(frame.transform, false);
        var badgeImg = badge.AddComponent<Image>();
        badgeImg.color = new Color(0.05f, 0.05f, 0.13f, 0.95f);
        var bRT      = badge.GetComponent<RectTransform>();
        bRT.anchorMin = new Vector2(0f, 0f); bRT.anchorMax = new Vector2(1f, 0f);
        bRT.pivot     = new Vector2(0.5f, 0f);
        bRT.anchoredPosition = Vector2.zero;
        bRT.sizeDelta        = new Vector2(0f, 48f);

        badge.transform.AddTMP("Name", faceLabel, 22f, C_Gold,
            TextAlignmentOptions.Center, FontStyles.Bold);
    }

    // ── 스탯 카드 ─────────────────────────────────────────────────
    private void BuildStatCardTitle(Transform parent)
    {
        var t = parent.AddTMP("Title", "— 게임 결과 —", FS2, C_Gold,
            TextAlignmentOptions.Center, FontStyles.Normal);
        var rt = t.rectTransform;
        rt.anchorMin = new Vector2(0f, 1f); rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot     = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -12f);
        rt.sizeDelta        = new Vector2(-M * 2, 36f);
    }

    private void BuildStatBars(Transform parent)
    {
        string[] labels = { "판단력", "위험도", "의존도" };
        string[] ids    = { "Stat_0", "Stat_1", "Stat_2" };
        string[] barIds = { "StatBar_0", "StatBar_1", "StatBar_2" };
        // 타이틀 하단: anchoredPosition.y(-12) + sizeDelta.y(36) = 48px 아래에서 시작
        const float titleBottomY = 48f;
        const float topPad       = 8f;   // 타이틀 아래 여백
        const float rowH         = 48f;  // 행 높이
        const float rowGap       = 10f;  // 행 간 여백

        for (int i = 0; i < 3; i++)
        {
            // 상단 앵커 기준: 타이틀 하단 + 여백 + 이전 행들 누적
            float topY = -(titleBottomY + topPad + i * (rowH + rowGap));
            var   row  = new GameObject($"StatRow_{i}");
            row.transform.SetParent(parent, false);
            var rRT = EnsureRT(row);
            // 중앙 앵커 대신 상단 앵커 사용 → 아래로 넘치지 않음
            rRT.anchorMin        = new Vector2(0f, 1f);    // 상단 고정
            rRT.anchorMax        = new Vector2(1f, 1f);    // 상단 고정
            rRT.pivot            = new Vector2(0.5f, 1f);  // 피벗 상단-중앙
            rRT.anchoredPosition = new Vector2(0f, topY);
            rRT.sizeDelta        = new Vector2(-M * 2, rowH);

            // 라벨 (왼쪽 25%)
            var lbl = row.transform.AddTMP(ids[i], labels[i] + "  ???",
                FS2, Color.gray, TextAlignmentOptions.Left, FontStyles.Normal);
            var lRT = lbl.rectTransform;
            lRT.anchorMin = new Vector2(0f, 0f); lRT.anchorMax = new Vector2(0.25f, 1f);
            lRT.offsetMin = new Vector2(6f, 0f); lRT.offsetMax = Vector2.zero;

            // 바 컨테이너 (오른쪽 75%)
            var barGO = new GameObject(barIds[i]);
            barGO.transform.SetParent(row.transform, false);
            var barRT = EnsureRT(barGO);
            barRT.anchorMin = new Vector2(0.26f, 0.15f);
            barRT.anchorMax = new Vector2(1f,    0.85f);
            barRT.offsetMin = barRT.offsetMax = Vector2.zero;

            // Slider
            var slider = barGO.AddComponent<Slider>();
            slider.minValue    = 0f;
            slider.maxValue    = 1f;
            slider.value       = 0f;
            slider.interactable = false;

            // Background
            var bgGO  = new GameObject("Background");
            bgGO.transform.SetParent(barGO.transform, false);
            var bgImg = bgGO.AddComponent<Image>();
            bgImg.color = C_BarBG;
            StretchFull(bgGO.GetComponent<RectTransform>());
            slider.targetGraphic = bgImg;

            // Fill Area
            var faGO = new GameObject("Fill Area");
            faGO.transform.SetParent(barGO.transform, false);
            var faRT = EnsureRT(faGO);
            faRT.anchorMin = Vector2.zero; faRT.anchorMax = Vector2.one;
            faRT.offsetMin = faRT.offsetMax = Vector2.zero;

            // Fill
            var fillGO  = new GameObject("Fill");
            fillGO.transform.SetParent(faGO.transform, false);
            var fillImg = fillGO.AddComponent<Image>();
            fillImg.color = Color.gray;
            var fillRT  = EnsureRT(fillGO);
            fillRT.anchorMin = Vector2.zero; fillRT.anchorMax = Vector2.one;
            fillRT.offsetMin = fillRT.offsetMax = Vector2.zero;
            slider.fillRect  = fillRT;
        }

        if (StatReportUI.Instance == null)
            parent.gameObject.AddComponent<StatReportUI>();
    }

    // ── 선택지 패널 ───────────────────────────────────────────────
    private GameObject BuildChoicePanel(Transform parent)
    {
        const float BW    = 860f;
        float totalH      = BH * 3 + BG * 2 + M * 2;

        var panel = MakePanel(parent, "FinalChoicePanel",
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0f, 170f + M + 10f),
            new Vector2(BW + M * 2, totalH));

        string[] btns = { "다시 시작한다", "누군가에게 연락한다", "게임 종료" };
        string[] nums = { "①", "②", "③" };

        for (int i = 0; i < 3; i++)
        {
            float yPos = M + (2 - i) * (BH + BG);
            var btnGO  = new GameObject($"Btn_{i}");
            btnGO.transform.SetParent(panel.transform, false);
            var btnImg = btnGO.AddComponent<Image>(); btnImg.color = C_Btn;
            var btn    = btnGO.AddComponent<Button>();
            var cs     = btn.colors;
            cs.normalColor      = C_Btn;
            cs.highlightedColor = C_BtnHov;
            cs.pressedColor     = C_BtnHov * 0.8f;
            cs.selectedColor    = C_Btn;
            btn.colors          = cs;
            var bRT = btnGO.GetComponent<RectTransform>();
            bRT.anchorMin = new Vector2(0.5f, 0f); bRT.anchorMax = new Vector2(0.5f, 0f);
            bRT.pivot     = new Vector2(0.5f, 0f);
            bRT.anchoredPosition = new Vector2(0f, yPos);
            bRT.sizeDelta        = new Vector2(BW, BH);

            var num = btnGO.transform.AddTMP("Num", nums[i], FS2, C_Gold,
                TextAlignmentOptions.Center, FontStyles.Normal);
            var nRT = num.rectTransform;
            nRT.anchorMin = new Vector2(0f, 0f); nRT.anchorMax = new Vector2(0.08f, 1f);
            nRT.offsetMin = nRT.offsetMax = Vector2.zero;

            var lbl = btnGO.transform.AddTMP("Label", btns[i], FS, Color.white,
                TextAlignmentOptions.Left, FontStyles.Normal);
            var lRT = lbl.rectTransform;
            lRT.anchorMin = new Vector2(0.09f, 0f); lRT.anchorMax = Vector2.one;
            lRT.offsetMin = new Vector2(0f, 6f); lRT.offsetMax = new Vector2(-12f, -6f);

            int idx = i;
            btn.onClick.AddListener(() => OnChoice(idx));
        }
        return panel;
    }

    // ── Phase 2 코루틴 ────────────────────────────────────────────
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
        // alpha=1로 이미 보이면 페이드 건너뜀
        if (_finalQuestionCG != null && _finalQuestionCG.alpha < 1f)
            yield return FadeCG(_finalQuestionCG, _finalQuestionCG.alpha, 1f, 0.5f);
        yield return new WaitForSeconds(0.3f);
    }

    /// <summary>
    /// Phase 2 전용 — 글리치 없이 비네트가 숨쉬듯 천천히 변화.
    /// 중독의 무게감을 시각적으로 표현하면서 집중을 방해하지 않음.
    /// </summary>
    private IEnumerator BreathingVignette()
    {
        const float MIN_V  = 0.25f;  // 가장 밝을 때 (숨 들이쉼)
        const float MAX_V  = 0.50f;  // 가장 어두울 때 (숨 내쉼)
        const float PERIOD = 5.0f;   // 1회 호흡 주기 (초)

        float t = 0f;
        while (true)
        {
            t += Time.deltaTime;
            // sin 곡선: 0→1→0 으로 자연스럽게 출렁임
            float ratio    = (1f - Mathf.Cos(t / PERIOD * Mathf.PI * 2f)) * 0.5f;
            float intensity = Mathf.Lerp(MIN_V, MAX_V, ratio);
            FXManager.Instance?.SetVignetteIntensity(intensity);
            yield return null;
        }
    }

    // ── 선택지 처리 ───────────────────────────────────────────────
    private void OnChoice(int idx)
    {
        PostProcessingController.Instance?.ResetEffects();
        switch (idx)
        {
            case 0:
                SFXManager.Instance?.PlayEndingRestart();
                if (GameManager.Instance != null) GameManager.Instance.StartNewGame();
                else UnityEngine.SceneManagement.SceneManager.LoadScene("00_MainMenu");
                break;
            case 1:
                SFXManager.Instance?.PlayDial1393();
                Application.OpenURL(CONTACT_URL);
                break;
            case 2:
                SFXManager.Instance?.PlayEndingQuit();
                if (GameManager.Instance != null) GameManager.Instance.QuitGame();
                else
                {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                }
                break;
        }
    }

    // ══════════════════════════════════════════════════════════════
    //  공통 유틸
    // ══════════════════════════════════════════════════════════════
    private static IEnumerator FadeCG(CanvasGroup cg, float from, float to, float dur)
    {
        if (cg == null) yield break;
        float t = 0f;
        while (t < dur) { t += Time.deltaTime; cg.alpha = Mathf.Lerp(from, to, t / dur); yield return null; }
        cg.alpha = to;
    }

    private static IEnumerator FadeImg(Image img, float from, float to, float dur)
    {
        if (img == null) yield break;
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            var c = img.color; c.a = Mathf.Lerp(from, to, t / dur); img.color = c;
            yield return null;
        }
        var cf = img.color; cf.a = to; img.color = cf;
    }

    private static Image MakeImage(Transform parent, string name, Color color)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>(); img.color = color; return img;
    }

    private static GameObject MakePanel(Transform parent, string name,
        Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>(); img.color = C_Panel;
        var rt  = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax;
        rt.pivot     = pivot; rt.anchoredPosition = pos; rt.sizeDelta = size;
        return go;
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    private static RectTransform EnsureRT(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        return rt != null ? rt : go.AddComponent<RectTransform>();
    }

    private static GameObject SceneFind(string goName)
    {
        var all = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var t in all) if (t.name == goName) return t.gameObject;
        return null;
    }

    // FindSceneBackgroundSprite 제거됨 — 씬 배경이 엔딩 화면에 비치는 문제 방지
}

// ── Transform 확장 — TMP 생성 헬퍼 ─────────────────────────────
internal static class TransformTMPExt
{
    public static TextMeshProUGUI AddTMP(this Transform parent, string name, string text,
        float fontSize, Color color, TextAlignmentOptions align, FontStyles style)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text       = text;
        tmp.fontSize   = fontSize;
        tmp.color      = color;
        tmp.alignment  = align;
        tmp.fontStyle  = style;
        tmp.raycastTarget = false;
        return tmp;
    }
}
