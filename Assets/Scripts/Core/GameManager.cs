using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Scene name constants
    public const string SCENE_MAIN_MENU    = "00_MainMenu";
    public const string SCENE_STEP1        = "01_Step1_Prologue";
    public const string SCENE_STEP2        = "02_Step2_Club";
    public const string SCENE_STEP3        = "03_Step3_Morning";
    public const string SCENE_STEP4        = "04_Step4_Party";
    public const string SCENE_STEP5        = "05_Step5_Collapse";
    public const string SCENE_REPORT       = "06_EndingReport";
    public const string SCENE_GOOD_END     = "07_GoodEnd";
    public const string SCENE_NORMAL_END   = "08_NormalEnd";
    public const string SCENE_BAD_END      = "09_BadEnd";
    public const string SCENE_TRUE_END     = "10_TrueEnd";

    // PlayerPrefs keys for TRUE END unlock
    private const string PREF_GOOD_CLEARED   = "cleared_good";
    private const string PREF_NORMAL_CLEARED = "cleared_normal";
    private const string PREF_BAD_CLEARED    = "cleared_bad";

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

    public void StartNewGame()
    {
        StatManager.Instance.ResetStats();
        LoadScene(SCENE_STEP1);
    }

    public void LoadScene(string sceneName, float delay = 0f)
    {
        StartCoroutine(LoadSceneRoutine(sceneName, delay));
    }

    private IEnumerator LoadSceneRoutine(string sceneName, float delay)
    {
        if (delay > 0f)
        {
            ScreenEffects.Instance?.FadeOut(delay * 0.5f);
            yield return new WaitForSeconds(delay);
        }
        SceneManager.LoadScene(sceneName);
    }

    public void GoToEnding()
    {
        EndingType ending = StatManager.Instance.CalculateEnding();
        MarkEndingCleared(ending);

        // 엔딩 씬 직접 로드 — 수치는 FourthWallBreak 이후에 표시
        PlayerPrefs.SetInt("pending_ending", (int)ending);
        PlayerPrefs.Save();

        if (IsTrueEndUnlocked() && IsNewGame())
        { LoadScene(SCENE_TRUE_END, 1f); return; }

        switch (ending)
        {
            case EndingType.Good:   LoadScene(SCENE_GOOD_END, 1f);   break;
            case EndingType.Normal: LoadScene(SCENE_NORMAL_END, 1f); break;
            case EndingType.Bad:    LoadScene(SCENE_BAD_END, 1f);    break;
            default:                LoadScene(SCENE_NORMAL_END, 1f); break;
        }
    }

    public void LoadPendingEnding()
    {
        EndingType ending = (EndingType)PlayerPrefs.GetInt("pending_ending", 0);

        if (IsTrueEndUnlocked() && IsNewGame())
        {
            LoadScene(SCENE_TRUE_END, 0.5f);
            return;
        }

        switch (ending)
        {
            case EndingType.Good:   LoadScene(SCENE_GOOD_END, 0.5f);   break;
            case EndingType.Normal: LoadScene(SCENE_NORMAL_END, 0.5f); break;
            case EndingType.Bad:    LoadScene(SCENE_BAD_END, 0.5f);    break;
        }
    }

    private void MarkEndingCleared(EndingType ending)
    {
        switch (ending)
        {
            case EndingType.Good:   PlayerPrefs.SetInt(PREF_GOOD_CLEARED, 1);   break;
            case EndingType.Normal: PlayerPrefs.SetInt(PREF_NORMAL_CLEARED, 1); break;
            case EndingType.Bad:    PlayerPrefs.SetInt(PREF_BAD_CLEARED, 1);    break;
        }
        PlayerPrefs.Save();
    }

    public bool IsTrueEndUnlocked()
    {
        return PlayerPrefs.GetInt(PREF_GOOD_CLEARED, 0) == 1
            && PlayerPrefs.GetInt(PREF_NORMAL_CLEARED, 0) == 1
            && PlayerPrefs.GetInt(PREF_BAD_CLEARED, 0) == 1;
    }

    // TRUE END는 세 엔딩 클리어 후 새 게임 시작 시 진입
    private bool IsNewGame() => StatManager.Instance.INT == 100
                             && StatManager.Instance.RISK == 0
                             && StatManager.Instance.ADDICT == 0;

    public void QuitGame() => Application.Quit();
}
