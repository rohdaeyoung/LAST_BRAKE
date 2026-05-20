using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private TextMeshProUGUI trueEndHint; // TRUE END 해금 힌트 텍스트

    private void Start()
    {
        // Inspector 미연결 시 이름으로 자동 탐색
        if (startButton == null)
        {
            var go = GameObject.Find("StartButton");
            if (go != null) startButton = go.GetComponent<Button>();
        }
        if (quitButton == null)
        {
            var go = GameObject.Find("QuitButton");
            if (go != null) quitButton = go.GetComponent<Button>();
        }

        if (startButton != null) startButton.onClick.AddListener(OnStart);
        else Debug.LogWarning("[MainMenuUI] StartButton을 찾을 수 없습니다!");

        if (quitButton != null) quitButton.onClick.AddListener(() => GameManager.Instance.QuitGame());

        // TRUE END 해금 여부 표시
        if (trueEndHint != null)
            trueEndHint.gameObject.SetActive(
                GameManager.Instance != null && GameManager.Instance.IsTrueEndUnlocked());
    }

    private void OnStart()
    {
        Debug.Log("[MainMenuUI] 게임 시작 버튼 클릭!");

        // GameManager 없으면 직접 씬 로드 (폴백)
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[MainMenuUI] GameManager.Instance가 없어서 직접 씬 로드");
            UnityEngine.SceneManagement.SceneManager.LoadScene("01_Step1_Prologue");
            return;
        }

        // 항상 Step1부터 시작 — TrueEnd 분기는 게임 마지막(GoToEnding)에서만 처리
        GameManager.Instance.StartNewGame();
    }
}
