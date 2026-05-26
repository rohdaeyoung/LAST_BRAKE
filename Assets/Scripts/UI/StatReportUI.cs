using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class StatReportUI : MonoBehaviour
{
    public static StatReportUI Instance { get; private set; }

    [Header("수치 슬롯")]
    [SerializeField] private TextMeshProUGUI intLabel;
    [SerializeField] private TextMeshProUGUI riskLabel;
    [SerializeField] private TextMeshProUGUI addictLabel;

    [SerializeField] private Slider intBar;
    [SerializeField] private Slider riskBar;
    [SerializeField] private Slider addictBar;

    [Header("연출")]
    [SerializeField] private float revealInterval = 0.8f;
    [SerializeField] private float barFillDuration = 1.0f;
    [SerializeField] private Color dangerColor  = new Color(0.9f, 0.2f, 0.2f);
    [SerializeField] private Color safeColor    = new Color(0.3f, 0.85f, 0.4f);

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Inspector 미연결 시 씬 이름으로 자동 탐색
        AutoWireRefs();

        // 초기 상태: 모두 "???" — null 레퍼런스 방지
        if (intLabel    != null) SetPlaceholder(intLabel,    "판단력  ???");
        if (riskLabel   != null) SetPlaceholder(riskLabel,   "위험도  ???");
        if (addictLabel != null) SetPlaceholder(addictLabel, "의존도  ???");
        if (intBar    != null) intBar.value    = 0;
        if (riskBar   != null) riskBar.value   = 0;
        if (addictBar != null) addictBar.value = 0;
    }

    private void AutoWireRefs()
    {
        // 라벨: 씬 내 TextMeshProUGUI 중 이름으로 매핑
        // Stat_0=판단력, Stat_1=위험도, Stat_2=의존도
        if (intLabel    == null) intLabel    = FindTMP("Stat_0");
        if (riskLabel   == null) riskLabel   = FindTMP("Stat_1");
        if (addictLabel == null) addictLabel = FindTMP("Stat_2");

        // 슬라이더: StatBar_0~2
        if (intBar    == null) intBar    = FindSlider("StatBar_0");
        if (riskBar   == null) riskBar   = FindSlider("StatBar_1");
        if (addictBar == null) addictBar = FindSlider("StatBar_2");

        // 이름으로 못 찾으면 씬 내 순서대로 할당
        if (intLabel == null || riskLabel == null || addictLabel == null)
        {
            var allTMP = FindObjectsByType<TMPro.TextMeshProUGUI>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            int idx = 0;
            foreach (var t in allTMP)
            {
                if (t.gameObject.name.StartsWith("Stat") || t.text.Contains("???"))
                {
                    if      (idx == 0 && intLabel    == null) intLabel    = t;
                    else if (idx == 1 && riskLabel   == null) riskLabel   = t;
                    else if (idx == 2 && addictLabel == null) addictLabel = t;
                    idx++;
                }
            }
        }
    }

    private static TMPro.TextMeshProUGUI FindTMP(string goName)
    {
        var go = GameObject.Find(goName);
        return go != null ? go.GetComponent<TMPro.TextMeshProUGUI>() : null;
    }

    private static UnityEngine.UI.Slider FindSlider(string goName)
    {
        var go = GameObject.Find(goName);
        return go != null ? go.GetComponent<UnityEngine.UI.Slider>() : null;
    }

    public void RevealStats(int intVal, int riskVal, int addictVal, Action onComplete)
    {
        StartCoroutine(RevealSequence(intVal, riskVal, addictVal, onComplete));
    }

    private IEnumerator RevealSequence(int intVal, int riskVal, int addictVal, Action onComplete)
    {
        yield return new WaitForSeconds(0.5f);

        yield return RevealStat(intLabel, intBar,
            $"판단력  {intVal}", intVal / 100f, intVal < 50 ? dangerColor : safeColor);

        yield return new WaitForSeconds(revealInterval);

        yield return RevealStat(riskLabel, riskBar,
            $"위험도  {riskVal}", riskVal / 100f, riskVal > 60 ? dangerColor : safeColor);

        yield return new WaitForSeconds(revealInterval);

        yield return RevealStat(addictLabel, addictBar,
            $"의존도  {addictVal}", addictVal / 100f, addictVal > 60 ? dangerColor : safeColor);

        yield return new WaitForSeconds(revealInterval);
        onComplete?.Invoke();
    }

    private IEnumerator RevealStat(TextMeshProUGUI label, Slider bar, string text, float targetValue, Color color)
    {
        if (label != null) { label.text = text; label.color = color; }
        // Slider의 Fill Area > Fill Image에 색상 적용 (null-safe)
        if (bar != null)
        {
            var fillImg = bar.fillRect?.GetComponent<Image>();
            if (fillImg != null) fillImg.color = color;
        }

        if (bar != null)
        {
            float elapsed = 0f;
            while (elapsed < barFillDuration)
            {
                elapsed += Time.deltaTime;
                bar.value = Mathf.Lerp(0f, targetValue, elapsed / barFillDuration);
                yield return null;
            }
            bar.value = targetValue;
        }
    }

    private void SetPlaceholder(TextMeshProUGUI label, string text)
    {
        label.text  = text;
        label.color = Color.gray;
    }
}
