using UnityEngine;
using System.Collections;

// 씬: 07_GoodEnd — 「새벽의 선택」
// 조건: INT >= 75, ADDICT <= 40
public class GoodEndSequence : MonoBehaviour
{
    [SerializeField] private DialogueData[]  endingDialogue;
    [SerializeField] private FourthWallBreak fourthWallBreak;
    [SerializeField] private float           blackoutDuration = 1.0f;  // BadEnd와 동일

    private void Awake()
    {
        EnsureManagers();
        EnsureDialogueSystem();
    }

    private void Start()
    {
        // 씬 시작 시 노이즈 효과 (BadEnd와 동일)
        ScreenEffects.Instance?.ShowNoise(0.4f);

        // FourthWallBreak Inspector 미연결 시 씬에서 자동 탐색
        if (fourthWallBreak == null)
            fourthWallBreak = FindFirstObjectByType<FourthWallBreak>(FindObjectsInactive.Include);

        // endingDialogue 없으면 Resources 자동 로드
        if (endingDialogue == null || endingDialogue.Length == 0)
        {
            var data = Resources.Load<DialogueData>("Dialogues/GoodEnd_Dialogue");
            if (data != null) endingDialogue = new[] { data };
            else Debug.LogWarning("[GoodEnd] GoodEnd_Dialogue 에셋 없음");
        }

        if (DialogueManager.Instance != null && endingDialogue != null && endingDialogue.Length > 0)
            DialogueManager.Instance.StartSequence(endingDialogue, OnEndingDialogueComplete);
        else
            StartCoroutine(TransitionToFourthWall());
    }

    public void OnEndingDialogueComplete() => StartCoroutine(TransitionToFourthWall());

    // BadEnd와 동일한 전환 패턴
    private IEnumerator TransitionToFourthWall()
    {
        ScreenEffects.Instance?.ShowNoise(0f);          // 노이즈 해제
        yield return new WaitForSeconds(blackoutDuration);
        if (fourthWallBreak != null)
            fourthWallBreak.gameObject.SetActive(true);
        else
            Debug.LogWarning("[GoodEnd] FourthWallBreak를 씬에서 찾을 수 없습니다.");
    }

    static void EnsureManagers()
    {
        if (StatManager.Instance == null)
            new GameObject("StatManager").AddComponent<StatManager>();
        if (GameManager.Instance == null)
            new GameObject("GameManager").AddComponent<GameManager>();
    }

    static void EnsureDialogueSystem()
    {
        if (FindFirstObjectByType<DialogueBootstrap>(FindObjectsInactive.Include) == null)
        {
            var go = new GameObject("DialogueBootstrap_Runtime");
            go.AddComponent<DialogueBootstrap>();
        }
        if (DialogueManager.Instance == null)
        {
            var go = new GameObject("DialogueManager_Runtime");
            go.AddComponent<DialogueManager>();
        }
    }
}
