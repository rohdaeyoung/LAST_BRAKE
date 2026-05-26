using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// LAST BRAKE - 전체화면 CG 이벤트 뷰어 (PDF 스펙 기반)
/// CG_Club_Offer / CG_Seoyeon_Confrontation / CG_Collapse
/// CG_Hospital_Recovery / CG_BadEnd_Arrest
/// 탭하면 닫히고 대화가 재개됨
/// </summary>
public class CGViewer : MonoBehaviour
{
    public static CGViewer Instance { get; private set; }

    // ── Inspector / Editor 연결용 스프라이트 ─────────────────────
    [Header("CG 스프라이트 (EffectLinker가 자동 연결)")]
    [SerializeField] public Sprite cgClubOffer;
    [SerializeField] public Sprite cgSeoyeonConfrontation;
    [SerializeField] public Sprite cgCollapse;
    [SerializeField] public Sprite cgHospitalRecovery;
    [SerializeField] public Sprite cgBadEndArrest;

    // ── 런타임 UI ────────────────────────────────────────────────
    private GameObject canvasGO;
    private Image      cgImage;
    private Image      cgFade;
    private bool       isShowing;

    private System.Action onCloseCB;

    // ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildCGCanvas();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void BuildCGCanvas()
    {
        canvasGO = new GameObject("CGViewer_Canvas");
        DontDestroyOnLoad(canvasGO);
        canvasGO.transform.SetParent(transform);

        var canvas          = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 990;   // FXManager(998)보다 아래, 대화 UI보다 위
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // 어두운 배경 + 탭 감지
        var fadeGO = new GameObject("CGFade");
        fadeGO.transform.SetParent(canvasGO.transform, false);
        cgFade = fadeGO.AddComponent<Image>();
        cgFade.color = new Color(0f, 0f, 0f, 0f);
        StretchFull(cgFade.rectTransform);
        cgFade.raycastTarget = true;

        // 탭하면 닫기
        var btn = fadeGO.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;
        btn.onClick.AddListener(OnTap);

        // CG 이미지
        var imgGO = new GameObject("CGImage");
        imgGO.transform.SetParent(canvasGO.transform, false);
        cgImage = imgGO.AddComponent<Image>();
        cgImage.color          = new Color(1f, 1f, 1f, 0f);
        cgImage.preserveAspect = true;
        StretchFull(cgImage.rectTransform);
        cgImage.raycastTarget  = false;

        // "탭해서 계속" 안내 텍스트
        var hintGO = new GameObject("CGHint");
        hintGO.transform.SetParent(canvasGO.transform, false);
        var hintTMP = hintGO.AddComponent<TMPro.TextMeshProUGUI>();
        hintTMP.text      = "탭하여 계속";
        hintTMP.fontSize  = 22;
        hintTMP.color     = new Color(1f, 1f, 1f, 0.6f);
        hintTMP.alignment = TMPro.TextAlignmentOptions.Center;
        var hintRT = hintTMP.rectTransform;
        hintRT.anchorMin       = new Vector2(0f, 0f);
        hintRT.anchorMax       = new Vector2(1f, 0f);
        hintRT.pivot           = new Vector2(0.5f, 0f);
        hintRT.anchoredPosition = new Vector2(0f, 40f);
        hintRT.sizeDelta       = new Vector2(0f, 50f);
        hintTMP.raycastTarget  = false;

        canvasGO.SetActive(false);
    }

    // ═════════════════════════════════════════════════════════════
    //  퍼블릭 API
    // ═════════════════════════════════════════════════════════════

    /// <summary>CG 표시. 탭 후 onClose 호출</summary>
    public void ShowCG(CGType type, System.Action onClose = null)
    {
        if (type == CGType.None) { onClose?.Invoke(); return; }
        var sprite = GetSprite(type);
        if (sprite == null) { onClose?.Invoke(); return; }

        onCloseCB = onClose;
        cgImage.sprite = sprite;
        canvasGO.SetActive(true);
        isShowing = true;

        if (showRoutine != null) StopCoroutine(showRoutine);
        showRoutine = StartCoroutine(FadeIn());
    }

    private Coroutine showRoutine;

    private void OnTap()
    {
        if (!isShowing) return;
        isShowing = false;
        if (showRoutine != null) { StopCoroutine(showRoutine); showRoutine = null; }
        showRoutine = StartCoroutine(FadeOutClose());
    }

    // ─────────────────────────────────────────────────────────────
    private Sprite GetSprite(CGType type)
    {
        return type switch
        {
            CGType.ClubOffer            => cgClubOffer,
            CGType.SeoyeonConfrontation => cgSeoyeonConfrontation,
            CGType.Collapse             => cgCollapse,
            CGType.HospitalRecovery     => cgHospitalRecovery,
            CGType.BadEndArrest         => cgBadEndArrest,
            _                           => null,
        };
    }

    private IEnumerator FadeIn()
    {
        float t = 0f;
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            float a = Mathf.SmoothStep(0f, 1f, t / 0.5f);
            cgFade.color  = new Color(0f, 0f, 0f, a * 0.85f);
            cgImage.color = new Color(1f, 1f, 1f, a);
            yield return null;
        }
        cgFade.color  = new Color(0f, 0f, 0f, 0.85f);
        cgImage.color = Color.white;
    }

    private IEnumerator FadeOutClose()
    {
        float t = 0f;
        while (t < 0.35f)
        {
            t += Time.deltaTime;
            float a = Mathf.SmoothStep(1f, 0f, t / 0.35f);
            cgFade.color  = new Color(0f, 0f, 0f, a * 0.85f);
            cgImage.color = new Color(1f, 1f, 1f, a);
            yield return null;
        }
        canvasGO.SetActive(false);
        showRoutine = null;

        var cb = onCloseCB;
        onCloseCB = null;
        cb?.Invoke();
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }
}
