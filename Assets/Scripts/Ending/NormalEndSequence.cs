using UnityEngine;
using System.Collections;

// 씬: 08_NormalEnd — 「연락처 삭제」
// 조건: INT <= 50, RISK >= 60
public class NormalEndSequence : MonoBehaviour
{
    [SerializeField] private DialogueData[]  endingDialogue;
    [SerializeField] private FourthWallBreak fourthWallBreak;
    [SerializeField] private float           pauseDuration = 1.5f;  // 대화 종료 후 FourthWallBreak 전환 대기

    private void Awake()
    {
        EnsureManagers();
        EnsureDialogueSystem();
    }

    private void Start()
    {
        BGMController.Instance?.PlayNormal();

        // FourthWallBreak Inspector 미연결 시 씬에서 자동 탐색
        if (fourthWallBreak == null)
            fourthWallBreak = FindFirstObjectByType<FourthWallBreak>(FindObjectsInactive.Include);

        if (endingDialogue == null || endingDialogue.Length == 0)
        {
            var data = Resources.Load<DialogueData>("Dialogues/NormalEnd_Dialogue");
            if (data != null) endingDialogue = new[] { data };
        }

        if (DialogueManager.Instance != null && endingDialogue != null && endingDialogue.Length > 0)
            DialogueManager.Instance.StartSequence(endingDialogue, OnEndingDialogueComplete);
        else
            StartCoroutine(TransitionToFourthWall());
    }

    public void OnEndingDialogueComplete() => StartCoroutine(TransitionToFourthWall());

    // 07_GoodEnd와 동일한 매끄러운 전환 패턴
    private IEnumerator TransitionToFourthWall()
    {
        yield return new WaitForSeconds(pauseDuration);
        if (fourthWallBreak != null)
            fourthWallBreak.gameObject.SetActive(true);
        else
            Debug.LogWarning("[NormalEnd] FourthWallBreak를 씬에서 찾을 수 없습니다.");
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
            Debug.Log("[NormalEnd] DialogueBootstrap 런타임 생성");
        }
        if (DialogueManager.Instance == null)
        {
            var go = new GameObject("DialogueManager_Runtime");
            go.AddComponent<DialogueManager>();
            Debug.Log("[NormalEnd] DialogueManager 런타임 생성");
        }
    }
}
