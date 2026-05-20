using UnityEngine;

// EndingReport 씬에 붙임. 수치를 공개하고 적절한 엔딩 씬으로 이동.
public class EndingCalculator : MonoBehaviour
{
    [SerializeField] private float revealDelay = 3f;

    private void Start()
    {
        // StatManager가 없으면(씬 직접 실행 시) 자동 생성
        if (StatManager.Instance == null)
        {
            Debug.LogWarning("[EndingCalculator] StatManager 없음 → 자동 생성 (기본값 사용)");
            new UnityEngine.GameObject("StatManager").AddComponent<StatManager>();
        }

        int intVal    = StatManager.Instance.INT;
        int riskVal   = StatManager.Instance.RISK;
        int addictVal = StatManager.Instance.ADDICT;

        if (StatReportUI.Instance != null)
        {
            StatReportUI.Instance.RevealStats(intVal, riskVal, addictVal,
                onComplete: () => Invoke(nameof(ProceedToEnding), revealDelay));
        }
        else
        {
            // StatReportUI 없으면 바로 엔딩으로 이동
            Invoke(nameof(ProceedToEnding), revealDelay);
        }
    }

    private void ProceedToEnding()
    {
        if (GameManager.Instance != null) GameManager.Instance.LoadPendingEnding();
        else
        {
            // GameManager 없으면 수치로 직접 계산
            var ending = StatManager.Instance?.CalculateEnding() ?? EndingType.Normal;
            string scene = ending switch {
                EndingType.Good   => "07_GoodEnd",
                EndingType.Bad    => "09_BadEnd",
                _                 => "08_NormalEnd"
            };
            UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
        }
    }
}
