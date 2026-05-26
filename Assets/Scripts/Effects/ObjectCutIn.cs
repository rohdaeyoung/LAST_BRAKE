using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// LAST BRAKE - 소품 컷인 (PDF 스펙 기반)
/// OBJ_Smartphone / OBJ_Clock0730 / OBJ_PillBottle
/// OBJ_ChatWindow / OBJ_ReportPaper
/// 화면 우측 하단에서 슬라이드 인/아웃
/// </summary>
public class ObjectCutIn : MonoBehaviour
{
    public static ObjectCutIn Instance { get; private set; }

    // ── Inspector / Editor 연결용 스프라이트 ─────────────────────
    [Header("OBJ 스프라이트 (EffectLinker가 자동 연결)")]
    [SerializeField] public Sprite objSmartphone;
    [SerializeField] public Sprite objClock0730;
    [SerializeField] public Sprite objPillBottle;
    [SerializeField] public Sprite objChatWindow;
    [SerializeField] public Sprite objReportPaper;

    [Header("표시 크기 / 위치")]
    [SerializeField] private Vector2 cutInSize        = new Vector2(360f, 360f);
    [SerializeField] private Vector2 anchoredPosition = new Vector2(-100f, 220f);
    [SerializeField] private float   defaultDuration  = 2.5f;

    // ── 런타임 ───────────────────────────────────────────────────
    private GameObject canvasGO;
    private Image      objImage;
    private Coroutine  showRoutine;

    private Vector2 shownPos;
    private Vector2 hiddenPos;  // 오른쪽 밖으로 숨김

    // ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildCanvas();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void BuildCanvas()
    {
        canvasGO = new GameObject("ObjectCutIn_Canvas");
        DontDestroyOnLoad(canvasGO);
        canvasGO.transform.SetParent(transform);

        var canvas          = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 985;   // CGViewer(990)보다 아래
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // 오브젝트 이미지 — 우측 하단 고정
        var imgGO = new GameObject("ObjImage");
        imgGO.transform.SetParent(canvasGO.transform, false);
        objImage = imgGO.AddComponent<Image>();
        objImage.preserveAspect = true;
        objImage.color          = new Color(1f, 1f, 1f, 0f);
        objImage.raycastTarget  = false;

        var rt        = objImage.rectTransform;
        rt.anchorMin  = new Vector2(1f, 0f);
        rt.anchorMax  = new Vector2(1f, 0f);
        rt.pivot      = new Vector2(1f, 0f);
        rt.sizeDelta  = cutInSize;

        shownPos  = anchoredPosition;
        hiddenPos = anchoredPosition + new Vector2(cutInSize.x + 50f, 0f);
        rt.anchoredPosition = hiddenPos;

        canvasGO.SetActive(false);
    }

    // ═════════════════════════════════════════════════════════════
    //  퍼블릭 API
    // ═════════════════════════════════════════════════════════════

    /// <summary>소품 컷인 표시. duration 후 자동 숨김</summary>
    public void ShowObject(ObjectType type, float duration = -1f)
    {
        if (type == ObjectType.None) return;
        var sprite = GetSprite(type);
        if (sprite == null) return;

        float dur = duration > 0f ? duration : defaultDuration;
        objImage.sprite = sprite;
        canvasGO.SetActive(true);

        if (showRoutine != null) StopCoroutine(showRoutine);
        showRoutine = StartCoroutine(ShowAndHide(dur));
    }

    public void HideImmediate()
    {
        if (showRoutine != null) { StopCoroutine(showRoutine); showRoutine = null; }
        canvasGO.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────
    private Sprite GetSprite(ObjectType type)
    {
        return type switch
        {
            ObjectType.Smartphone  => objSmartphone,
            ObjectType.Clock0730   => objClock0730,
            ObjectType.PillBottle  => objPillBottle,
            ObjectType.ChatWindow  => objChatWindow,
            ObjectType.ReportPaper => objReportPaper,
            _                      => null,
        };
    }

    private IEnumerator ShowAndHide(float duration)
    {
        var rt = objImage.rectTransform;

        // ── 슬라이드 인 (0.3초)
        float t = 0f;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / 0.3f);
            rt.anchoredPosition = Vector2.Lerp(hiddenPos, shownPos, p);
            var c = objImage.color; c.a = p; objImage.color = c;
            yield return null;
        }
        rt.anchoredPosition = shownPos;
        objImage.color = Color.white;

        // ── 표시 유지
        yield return new WaitForSeconds(duration);

        // ── 슬라이드 아웃 (0.3초)
        t = 0f;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / 0.3f);
            rt.anchoredPosition = Vector2.Lerp(shownPos, hiddenPos, p);
            var c = objImage.color; c.a = 1f - p; objImage.color = c;
            yield return null;
        }

        canvasGO.SetActive(false);
        showRoutine = null;
    }
}
