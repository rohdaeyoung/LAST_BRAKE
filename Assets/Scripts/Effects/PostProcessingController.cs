using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

// URP Global Volume이 씬에 있어야 함
// Volume에 Vignette, Chromatic Aberration, Color Adjustments 추가
public class PostProcessingController : MonoBehaviour
{
    public static PostProcessingController Instance { get; private set; }

    [SerializeField] private Volume globalVolume;

    private Vignette          vignette;
    private ChromaticAberration chromatic;
    private ColorAdjustments  colorAdj;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        // Inspector 미연결 시 씬에서 자동 탐색
        if (globalVolume == null)
            globalVolume = UnityEngine.Object.FindFirstObjectByType<Volume>();

        if (globalVolume != null && globalVolume.profile != null)
        {
            globalVolume.profile.TryGet(out vignette);
            globalVolume.profile.TryGet(out chromatic);
            globalVolume.profile.TryGet(out colorAdj);
        }
    }

    // ADDICT 수치에 비례해 Vignette 강도 조절 (0~100 → 0.2~0.7)
    public void UpdateAddictEffect(float addictNormalized)
    {
        if (vignette  != null) vignette.intensity.value  = Mathf.Lerp(0.2f, 0.7f, addictNormalized);
        if (chromatic != null) chromatic.intensity.value = Mathf.Lerp(0f,   1f,   addictNormalized);
    }

    // 메타 엔딩: 화면 전체 흑백 (도윤만 컬러는 Sprite Layer로 분리)
    public void DesaturateScreen(float duration)
    {
        StartCoroutine(DesaturateRoutine(duration));
    }

    private IEnumerator DesaturateRoutine(float duration)
    {
        if (colorAdj == null) yield break;
        float elapsed = 0f;
        float startSat = colorAdj.saturation.value;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            colorAdj.saturation.value = Mathf.Lerp(startSat, -100f, elapsed / duration);
            yield return null;
        }
        colorAdj.saturation.value = -100f;
    }

    public void ResetEffects()
    {
        if (vignette  != null) vignette.intensity.value  = 0.2f;
        if (chromatic != null) chromatic.intensity.value = 0f;
        if (colorAdj  != null) colorAdj.saturation.value = 0f;
    }
}
