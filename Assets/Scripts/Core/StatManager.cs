using UnityEngine;

public class StatManager : MonoBehaviour
{
    public static StatManager Instance { get; private set; }

    [HideInInspector] public int INT = 100;
    [HideInInspector] public int RISK = 0;
    [HideInInspector] public int ADDICT = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ApplyChoice(int intDelta, int riskDelta, int addictDelta)
    {
        INT = Mathf.Clamp(INT + intDelta, 0, 100);
        RISK = Mathf.Clamp(RISK + riskDelta, 0, 100);
        ADDICT = Mathf.Clamp(ADDICT + addictDelta, 0, 100);

        if (INT < 40)
            ScreenEffects.Instance?.TriggerShake(0.3f, 0.15f);

        if (RISK >= 60)
            BGMController.Instance?.SetDistorted(true);

        if (ADDICT >= 60)
            ChoiceSystem.Instance?.EnableForcedChoice();
    }

    public void ResetStats()
    {
        INT = 100;
        RISK = 0;
        ADDICT = 0;
    }

    // GOOD END: INT >= 75, ADDICT <= 40
    // NORMAL END: INT <= 50, RISK >= 60
    // BAD END: ADDICT >= 80
    public EndingType CalculateEnding()
    {
        if (INT >= 75 && ADDICT <= 40)
            return EndingType.Good;
        if (ADDICT >= 80)
            return EndingType.Bad;
        if (INT <= 50 && RISK >= 60)
            return EndingType.Normal;
        return EndingType.Normal;
    }
}

public enum EndingType { Good, Normal, Bad, True }
