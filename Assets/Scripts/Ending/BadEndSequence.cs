using UnityEngine;
using System.Collections;

// 씬: 09_BadEnd — 「끝나지 않는 밤」
// 조건: ADDICT >= 80
public class BadEndSequence : MonoBehaviour
{
    [SerializeField] private DialogueData[]  endingDialogue;
    [SerializeField] private FourthWallBreak fourthWallBreak;
    // sirenSFX / sfxSource — Inspector 연결 선택 사항 (미연결 시 효과음 없이 진행)
    [SerializeField] private AudioClip       sirenSFX;
    [SerializeField] private AudioSource     sfxSource;
    [SerializeField] private float           blackoutDuration = 1.0f;

    private void Awake()
    {
        EnsureManagers();
        EnsureDialogueSystem();
    }

    private void Start()
    {
        ScreenEffects.Instance?.ShowNoise(0.4f);

        // FourthWallBreak Inspector 미연결 시 씬에서 자동 탐색
        if (fourthWallBreak == null)
            fourthWallBreak = FindFirstObjectByType<FourthWallBreak>(FindObjectsInactive.Include);

        if (endingDialogue == null || endingDialogue.Length == 0)
        {
            var data = Resources.Load<DialogueData>("Dialogues/BadEnd_Dialogue");
            if (data != null) endingDialogue = new[] { data };
        }

        if (DialogueManager.Instance != null && endingDialogue != null && endingDialogue.Length > 0)
            DialogueManager.Instance.StartSequence(endingDialogue, OnEndingDialogueComplete);
        else
            StartCoroutine(TransitionToFourthWall());
    }

    public void OnEndingDialogueComplete() => StartCoroutine(TransitionToFourthWall());

    // 07_GoodEnd와 동일한 매끄러운 전환 패턴 (배드엔딩 분위기는 FourthWallBreak Phase1에서 처리)
    private IEnumerator TransitionToFourthWall()
    {
        // 효과음만 재생 (화면 암전 없이 자연스럽게 전환)
        try
        {
            if (sirenSFX != null && sfxSource != null)
                sfxSource.PlayOneShot(sirenSFX);
        }
        catch { /* 효과음 없이 진행 */ }

        ScreenEffects.Instance?.ShowNoise(0f);  // 노이즈 해제
        yield return new WaitForSeconds(blackoutDuration);
        if (fourthWallBreak != null)
            fourthWallBreak.gameObject.SetActive(true);
        else
            Debug.LogWarning("[BadEnd] FourthWallBreak를 씬에서 찾을 수 없습니다.");
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
            Debug.Log("[BadEnd] DialogueBootstrap 런타임 생성");
        }
        if (DialogueManager.Instance == null)
        {
            var go = new GameObject("DialogueManager_Runtime");
            go.AddComponent<DialogueManager>();
            Debug.Log("[BadEnd] DialogueManager 런타임 생성");
        }
    }
}
