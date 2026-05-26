using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// LAST BRAKE - FX 오버레이 관리자 (PDF 스펙 기반)
/// FX_RedWarningOverlay / FX_GlitchOverlay / FX_DarkVignette / FX_BlurPulse
/// DontDestroyOnLoad — 씬 전환 후에도 유지
/// </summary>
public class FXManager : MonoBehaviour
{
    public static FXManager Instance { get; private set; }

    // ── Inspector / Editor 연결용 스프라이트 ─────────────────────
    [Header("FX 스프라이트 (EffectLinker가 자동 연결)")]
    [SerializeField] public Sprite sprRedWarning;
    [SerializeField] public Sprite sprGlitch;
    [SerializeField] public Sprite sprVignette;
    [SerializeField] public Sprite sprBlurPulse;

    // ── 런타임 Image 레퍼런스 ────────────────────────────────────
    private Image imgRedWarning;
    private Image imgGlitch;
    private Image imgVignette;
    private Image imgBlurPulse;

    private Coroutine glitchRoutine;
    private Coroutine blurRoutine;

    // ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildOverlayCanvas();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ── 퍼시스턴트 Canvas + Image 오브젝트 생성 ─────────────────
    private void BuildOverlayCanvas()
    {
        var canvasGO = new GameObject("FXManager_Canvas");
        DontDestroyOnLoad(canvasGO);
        canvasGO.transform.SetParent(transform);

        var canvas           = canvasGO.AddComponent<Canvas>();
        canvas.renderMode    = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder  = 998;   // ScreenEffects(999) 바로 아래
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // 순서: 비네트(항상 존재) → 글리치 → 블러 → 붉은경고
        imgVignette   = CreateFXImage(canvasGO.transform, "FX_Vignette",
                            sprVignette,    new Color(0f, 0f, 0f, 0f));
        imgGlitch     = CreateFXImage(canvasGO.transform, "FX_Glitch",
                            sprGlitch,      new Color(1f, 1f, 1f, 0f));
        imgBlurPulse  = CreateFXImage(canvasGO.transform, "FX_BlurPulse",
                            sprBlurPulse,   new Color(1f, 1f, 1f, 0f));
        imgRedWarning = CreateFXImage(canvasGO.transform, "FX_RedWarning",
                            sprRedWarning,  new Color(0.9f, 0f, 0f, 0f));

        // 비네트: 기본 낮은 강도로 항상 표시
        SetVignetteIntensity(0.25f);
    }

    private Image CreateFXImage(Transform parent, string name, Sprite sprite, Color startColor)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.sprite         = sprite;
        img.color          = startColor;
        img.raycastTarget  = false;
        img.preserveAspect = false;

        var rt = img.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        // 스프라이트가 없어도 오브젝트는 생성해둠 (나중에 연결 가능)
        go.SetActive(false);
        return img;
    }

    // ═════════════════════════════════════════════════════════════
    //  퍼블릭 API
    // ═════════════════════════════════════════════════════════════

    /// <summary>DialogueLine의 fxEffect 값으로 호출</summary>
    public void TriggerFX(FXType fx)
    {
        switch (fx)
        {
            case FXType.RedWarning: PlayRedWarning();          break;
            case FXType.Glitch:     SetGlitchActive(true);     break;
            case FXType.GlitchOff:  SetGlitchActive(false);    break;
            case FXType.Vignette:   SetVignetteIntensity(0.8f); break;
            case FXType.BlurPulse:  PlayBlurPulse();           break;
            // None: 글리치 해제, 비네트 기본값 복원
            case FXType.None:
            default:
                SetGlitchActive(false);
                SetVignetteIntensity(0.25f);
                break;
        }
    }

    // ── 붉은 경고 플래시 (위험한 선택 시) ─────────────────────
    public void PlayRedWarning()
    {
        if (imgRedWarning == null) return;
        StartCoroutine(FlashRoutine(imgRedWarning,
            new Color(0.9f, 0f, 0f, 0.7f), 0.15f, 0.45f));
    }

    // ── 글리치 ON/OFF (약한 배경 정적 모드) ────────────────────
    public void SetGlitchActive(bool on)
    {
        if (imgGlitch == null) return;
        if (on)
        {
            imgGlitch.gameObject.SetActive(true);
            if (glitchRoutine != null) StopCoroutine(glitchRoutine);
            glitchRoutine = StartCoroutine(GlitchRoutine());
        }
        else
        {
            if (glitchRoutine != null) { StopCoroutine(glitchRoutine); glitchRoutine = null; }
            imgGlitch.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 강한 글리치를 duration 초 동안만 번쩍이고 자동 종료.
    /// 대화 중 중요한 순간(선택지, 충격 대사 등)에 사용.
    /// </summary>
    public void PlayGlitchBurst(float duration = 1.5f)
    {
        if (imgGlitch == null) return;
        if (glitchRoutine != null) StopCoroutine(glitchRoutine);
        glitchRoutine = StartCoroutine(GlitchBurstRoutine(duration));
    }

    // ── 비네트 강도 (0=없음, 1=최대) ───────────────────────────
    public void SetVignetteIntensity(float t)
    {
        if (imgVignette == null) return;
        float alpha = Mathf.Clamp01(t) * 0.9f;
        var c = imgVignette.color;
        c.a = alpha;
        imgVignette.color = c;
        imgVignette.gameObject.SetActive(alpha > 0.01f);
    }

    // ── 블러 펄스 (정신 혼란) ──────────────────────────────────
    public void PlayBlurPulse(int count = 3)
    {
        if (imgBlurPulse == null) return;
        if (blurRoutine != null) StopCoroutine(blurRoutine);
        blurRoutine = StartCoroutine(BlurPulseRoutine(count));
    }

    // ── StatManager 연동: RISK/ADDICT 상승 시 호출 ──────────────
    public void OnRiskIncreased(int riskValue)
    {
        // RISK 60 이상: 비네트 강화
        float intensity = Mathf.Clamp01(riskValue / 100f) * 0.6f + 0.25f;
        SetVignetteIntensity(intensity);
    }

    public void OnAddictIncreased(int addictValue)
    {
        // ADDICT 60 이상: 연속 글리치 대신 짧은 버스트로 교체
        // → 대화 텍스트를 가리지 않음
        if (addictValue >= 60)
            PlayGlitchBurst(1.5f);
        else if (addictValue >= 80)
            PlayGlitchBurst(2.5f);  // 더 심한 경우 조금 더 길게
    }

    // ═════════════════════════════════════════════════════════════
    //  코루틴
    // ═════════════════════════════════════════════════════════════

    private IEnumerator FlashRoutine(Image img, Color peak, float inTime, float outTime)
    {
        img.gameObject.SetActive(true);
        float t = 0f;
        while (t < inTime)
        {
            t += Time.deltaTime;
            var c = peak; c.a = Mathf.Lerp(0f, peak.a, t / inTime);
            img.color = c;
            yield return null;
        }
        t = 0f;
        while (t < outTime)
        {
            t += Time.deltaTime;
            var c = peak; c.a = Mathf.Lerp(peak.a, 0f, t / outTime);
            img.color = c;
            yield return null;
        }
        var off = img.color; off.a = 0f; img.color = off;
        img.gameObject.SetActive(false);
    }

    /// <summary>약한 배경 정적 — 대화 중 켜두어도 텍스트를 가리지 않음</summary>
    private IEnumerator GlitchRoutine()
    {
        while (true)
        {
            // alpha 0.02~0.06: 겨우 보일 정도의 노이즈
            float alpha = Random.Range(0.02f, 0.06f);
            var c = imgGlitch.color; c.a = alpha; imgGlitch.color = c;

            // 수평 이동: 매우 드물게(4%), 아주 작게(±4px)
            if (Random.value < 0.04f)
            {
                var rt  = imgGlitch.rectTransform;
                float x = Random.Range(-4f, 4f);
                rt.offsetMin = new Vector2(x, 0f);
                rt.offsetMax = new Vector2(x, 0f);
                yield return new WaitForSeconds(0.03f);
                rt.offsetMin = rt.offsetMax = Vector2.zero;
            }

            // 프레임 간격 길게: 화면이 덜 번쩍임
            yield return new WaitForSeconds(Random.Range(0.15f, 0.5f));
        }
    }

    /// <summary>
    /// 강한 글리치를 duration 초 동안 번쩍이다가 자동으로 꺼짐.
    /// 선택지 결과, 충격적인 대사 등 극적 순간 전용.
    /// </summary>
    private IEnumerator GlitchBurstRoutine(float duration)
    {
        if (imgGlitch == null) { glitchRoutine = null; yield break; }
        imgGlitch.gameObject.SetActive(true);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // 페이드인(전반)→페이드아웃(후반) 곡선
            float progress = elapsed / duration;
            float envelope = progress < 0.5f
                ? Mathf.Lerp(0f, 1f, progress * 2f)
                : Mathf.Lerp(1f, 0f, (progress - 0.5f) * 2f);

            float alpha = Random.Range(0.15f, 0.45f) * envelope;
            var c = imgGlitch.color; c.a = alpha; imgGlitch.color = c;

            // 수평 이동: 20% 확률, ±20px
            if (Random.value < 0.20f)
            {
                var rt  = imgGlitch.rectTransform;
                float x = Random.Range(-20f, 20f);
                rt.offsetMin = new Vector2(x, 0f);
                rt.offsetMax = new Vector2(x, 0f);
                yield return new WaitForSeconds(0.04f);
                rt.offsetMin = rt.offsetMax = Vector2.zero;
                elapsed += 0.04f;
            }

            yield return new WaitForSeconds(Random.Range(0.04f, 0.12f));
            elapsed += 0.08f; // 루프 대기 시간 보정
        }

        // 자동 종료
        var off = imgGlitch.color; off.a = 0f; imgGlitch.color = off;
        imgGlitch.gameObject.SetActive(false);
        glitchRoutine = null;
    }

    private IEnumerator BlurPulseRoutine(int count)
    {
        if (imgBlurPulse == null) yield break;
        imgBlurPulse.gameObject.SetActive(true);
        for (int i = 0; i < count; i++)
        {
            yield return FadeAlpha(imgBlurPulse, 0f, 0.65f, 0.2f);
            yield return FadeAlpha(imgBlurPulse, 0.65f, 0f, 0.35f);
            yield return new WaitForSeconds(0.1f);
        }
        imgBlurPulse.gameObject.SetActive(false);
        blurRoutine = null;
    }

    private static IEnumerator FadeAlpha(Image img, float from, float to, float dur)
    {
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            var c = img.color; c.a = Mathf.Lerp(from, to, t / dur);
            img.color = c;
            yield return null;
        }
    }
}
