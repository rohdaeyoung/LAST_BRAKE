using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class ScreenEffects : MonoBehaviour
{
    public static ScreenEffects Instance { get; private set; }

    [SerializeField] private Image fadePanel;
    [SerializeField] private Image noiseOverlay;

    private Camera mainCam;
    private Vector3 camOrigin;

    // 퍼시스턴트 오버레이 캔버스 (씬 전환 후에도 유지)
    private Canvas  overlayCanvas;
    private Image   runtimeFade;
    private Image   runtimeNoise;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);   // ← 씬 전환 후에도 유지

        RefreshCamera();
        BuildPersistentOverlay();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // Instance가 파괴된 객체를 가리키지 않도록 정리
        if (Instance == this) Instance = null;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 로드 후 카메라 참조 갱신
        RefreshCamera();
        // FadeOut 후 새 씬에서 자동 FadeIn (검은 화면 유지 방지)
        StartCoroutine(AutoFadeIn());
    }

    private IEnumerator AutoFadeIn()
    {
        // 씬 Start() 완료될 때까지 한 프레임 대기
        yield return null;
        yield return null;
        var img = FadeImg;
        if (img != null && img.color.a > 0.1f)
            FadeIn(0.5f);
    }

    private void RefreshCamera()
    {
        mainCam = Camera.main;
        if (mainCam != null) camOrigin = mainCam.transform.localPosition;
    }

    // ── 런타임에 퍼시스턴트 오버레이 Canvas 생성 ─────────────────────────
    private void BuildPersistentOverlay()
    {
        var go     = new GameObject("ScreenEffects_Overlay");
        DontDestroyOnLoad(go);
        overlayCanvas              = go.AddComponent<Canvas>();
        overlayCanvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = 999;   // 최상위 레이어
        go.AddComponent<UnityEngine.UI.CanvasScaler>();
        go.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // 페이드 패널 (검정)
        var fadeGO  = new GameObject("FadePanel");
        fadeGO.transform.SetParent(go.transform, false);
        runtimeFade = fadeGO.AddComponent<Image>();
        runtimeFade.color = new Color(0, 0, 0, 0);
        StretchFull(runtimeFade.rectTransform);
        runtimeFade.raycastTarget = false;
        runtimeFade.gameObject.SetActive(false);

        // 노이즈 오버레이 (붉은)
        var noiseGO = new GameObject("NoiseOverlay");
        noiseGO.transform.SetParent(go.transform, false);
        runtimeNoise = noiseGO.AddComponent<Image>();
        runtimeNoise.color = new Color(0.6f, 0f, 0f, 0f);
        StretchFull(runtimeNoise.rectTransform);
        runtimeNoise.raycastTarget = false;
        runtimeNoise.gameObject.SetActive(false);
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    // Inspector 연결된 패널 우선, 없으면 런타임 패널 사용
    private Image FadeImg  => (fadePanel   != null && fadePanel  ) ? fadePanel   : runtimeFade;
    private Image NoiseImg => (noiseOverlay!= null && noiseOverlay) ? noiseOverlay : runtimeNoise;

    // ── 카메라 흔들림 ─────────────────────────────────────────────────────
    public void TriggerShake(float duration, float magnitude)
    {
        if (mainCam == null) RefreshCamera();
        if (mainCam != null) StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (mainCam == null) break;
            float x = Mathf.PerlinNoise(Time.time * 50f, 0f) * 2f - 1f;
            float y = Mathf.PerlinNoise(0f, Time.time * 50f) * 2f - 1f;
            mainCam.transform.localPosition = camOrigin + new Vector3(x, y, 0) * magnitude;
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (mainCam != null) mainCam.transform.localPosition = camOrigin;
    }

    // ── 노이즈 오버레이 ───────────────────────────────────────────────────
    public void ShowNoise(float alpha)
    {
        var img = NoiseImg;
        if (img == null) return;
        Color c = img.color;
        c.a = alpha;
        img.color = c;
        img.gameObject.SetActive(alpha > 0f);
    }

    // ── 화면 암전 / 밝기 ─────────────────────────────────────────────────
    public void FadeOut(float duration) => StartCoroutine(FadeRoutine(0f, 1f, duration));
    public void FadeIn(float duration)  => StartCoroutine(FadeRoutine(1f, 0f, duration));

    private IEnumerator FadeRoutine(float from, float to, float duration)
    {
        var img = FadeImg;
        if (img == null) yield break;
        img.gameObject.SetActive(true);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Color c = img.color;
            c.a = Mathf.Lerp(from, to, elapsed / duration);
            img.color = c;
            yield return null;
        }
        Color final = img.color;
        final.a = to;
        img.color = final;
        if (to <= 0f) img.gameObject.SetActive(false);
    }
}
