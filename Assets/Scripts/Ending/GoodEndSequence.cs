using UnityEngine;
using System.Collections;

// 씬: 07_GoodEnd — 「아침은 다시 온다」
// 조건: INT >= 75, ADDICT <= 40
public class GoodEndSequence : MonoBehaviour
{
    [SerializeField] private DialogueData[] endingDialogue;
    [SerializeField] private FourthWallBreak fourthWallBreak;
    [SerializeField] private float endingDialoguePauseDuration = 2f;

    private void Awake()
    {
        EnsureManagers();
        // 엔딩 씬엔 DialogueBootstrap이 없으므로 런타임에 자동 추가
        EnsureDialogueSystem();
    }

    private void Start()
    {
        BGMController.Instance?.PlayNormal();

        // FourthWallBreak Inspector 미연결 시 씬에서 자동 탐색
        if (fourthWallBreak == null)
            fourthWallBreak = FindFirstObjectByType<FourthWallBreak>(FindObjectsInactive.Include);

        // endingDialogue 없으면 Resources 자동 로드
        if (endingDialogue == null || endingDialogue.Length == 0)
        {
            var data = Resources.Load<DialogueData>("Dialogues/GoodEnd_Dialogue");
            if (data != null) endingDialogue = new[] { data };
            else Debug.LogWarning("[GoodEnd] GoodEnd_Dialogue 에셋 없음 → 메뉴 11 실행 필요");
        }

        if (DialogueManager.Instance != null && endingDialogue != null && endingDialogue.Length > 0)
            DialogueManager.Instance.StartSequence(endingDialogue, OnEndingDialogueComplete);
        else
            StartCoroutine(TransitionToFourthWall());
    }

    public void OnEndingDialogueComplete() => StartCoroutine(TransitionToFourthWall());

    private IEnumerator TransitionToFourthWall()
    {
        yield return new WaitForSeconds(endingDialoguePauseDuration);
        if (fourthWallBreak != null)
            fourthWallBreak.gameObject.SetActive(true);
        else
            Debug.LogWarning("[GoodEnd] FourthWallBreak를 씬에서 찾을 수 없습니다. 씬 세팅 확인 필요.");
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
        // 1) DialogueBootstrap → DialogueUI 런타임 생성
        if (FindFirstObjectByType<DialogueBootstrap>(FindObjectsInactive.Include) == null)
        {
            var go = new GameObject("DialogueBootstrap_Runtime");
            go.AddComponent<DialogueBootstrap>();
            Debug.Log("[GoodEnd] DialogueBootstrap 런타임 생성");
        }
        // 2) DialogueManager — DialogueBootstrap이 UI를 만든 직후 생성해야 UI를 Awake에서 찾음
        if (DialogueManager.Instance == null)
        {
            var go = new GameObject("DialogueManager_Runtime");
            go.AddComponent<DialogueManager>();
            Debug.Log("[GoodEnd] DialogueManager 런타임 생성");
        }
    }
}
