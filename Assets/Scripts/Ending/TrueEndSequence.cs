using UnityEngine;
using TMPro;
using System.Collections;

// 씬: 10_TrueEnd — 메타 엔딩 (세 엔딩 모두 클리어 후 새 게임 시 자동 진입)
// 도윤이 카메라를 응시하며 시작 — FourthWallBreak가 메인 시퀀스를 담당
public class TrueEndSequence : MonoBehaviour
{
    [SerializeField] private FourthWallBreak fourthWallBreak;
    [SerializeField] private TextMeshProUGUI preMessageText;  // "...기억나?"
    [SerializeField] private float           preMessageDuration = 3.0f;
    [SerializeField] private CanvasGroup     preMessageGroup;

    private void Awake()
    {
        EnsureManagers();
        EnsureDialogueSystem();
    }

    private void Start()
    {
        BGMController.Instance?.FadeOut(0.5f);

        // FourthWallBreak Inspector 미연결 시 씬에서 자동 탐색
        if (fourthWallBreak == null)
            fourthWallBreak = FindFirstObjectByType<FourthWallBreak>(FindObjectsInactive.Include);

        StartCoroutine(TrueEndRoutine());
    }

    private IEnumerator TrueEndRoutine()
    {
        // 짧은 도입 메시지: 도윤이 먼저 말을 건넴
        if (preMessageGroup != null && preMessageText != null)
        {
            preMessageGroup.alpha = 0f;
            preMessageText.text = "...처음부터 다시 해봤어?\n그래도 여기 다시 왔구나.";
            yield return FadeGroup(preMessageGroup, 0f, 1f, 1.0f);
            yield return new WaitForSeconds(preMessageDuration);
            yield return FadeGroup(preMessageGroup, 1f, 0f, 0.8f);
        }

        // FourthWallBreak 시퀀스 시작 (도윤 응시 → 흑백 → 메시지 → 선택지)
        if (fourthWallBreak != null)
            fourthWallBreak.gameObject.SetActive(true);
        else
            Debug.LogWarning("[TrueEnd] FourthWallBreak를 씬에서 찾을 수 없습니다. 씬 세팅 확인 필요.");
    }

    private IEnumerator FadeGroup(CanvasGroup group, float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        group.alpha = to;
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
            Debug.Log("[TrueEnd] DialogueBootstrap 런타임 생성");
        }
        if (DialogueManager.Instance == null)
        {
            var go = new GameObject("DialogueManager_Runtime");
            go.AddComponent<DialogueManager>();
            Debug.Log("[TrueEnd] DialogueManager 런타임 생성");
        }
    }
}
