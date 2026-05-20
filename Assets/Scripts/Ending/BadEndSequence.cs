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
            StartCoroutine(BadEndRoutine());
    }

    public void OnEndingDialogueComplete() => StartCoroutine(BadEndRoutine());

    private IEnumerator BadEndRoutine()
    {
        // sirenSFX / sfxSource 미할당(Inspector 연결 안 됨)이어도 안전하게 처리
        try
        {
            if (sirenSFX != null && sfxSource != null)
                sfxSource.PlayOneShot(sirenSFX);
        }
        catch { /* 효과음 없이 진행 */ }

        ScreenEffects.Instance?.FadeOut(blackoutDuration);
        yield return new WaitForSeconds(blackoutDuration + 0.5f);
        ScreenEffects.Instance?.ShowNoise(0f);

        if (fourthWallBreak != null)
            fourthWallBreak.gameObject.SetActive(true);
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
