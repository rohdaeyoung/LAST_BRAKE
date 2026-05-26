using UnityEngine;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private ChoiceSystem choiceSystem;
    [SerializeField] private float typingSpeed = 0.04f;

    /// <summary>대화 시퀀스 전체 완료 시 호출될 콜백 (엔딩 씬 등에서 등록)</summary>
    public System.Action OnSequenceComplete;

    private DialogueData[] sequence;
    private int sequenceIndex;
    private int lineIndex;
    private bool isTyping;
    private Coroutine typingCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Inspector 미연결 시 자동 탐색 (비활성 오브젝트 포함)
        if (dialogueUI == null)
            dialogueUI = UnityEngine.Object.FindFirstObjectByType<DialogueUI>(FindObjectsInactive.Include);
        if (choiceSystem == null)
            choiceSystem = UnityEngine.Object.FindFirstObjectByType<ChoiceSystem>(FindObjectsInactive.Include);
    }

    public void StartSequence(DialogueData[] dialogueSequence, System.Action onComplete = null)
    {
        if (dialogueSequence == null || dialogueSequence.Length == 0)
        {
            Debug.LogWarning("[DialogueManager] StartSequence: 대화 데이터가 비어있습니다!");
            onComplete?.Invoke();
            return;
        }
        if (onComplete != null) OnSequenceComplete = onComplete;
        sequence = dialogueSequence;
        sequenceIndex = 0;
        lineIndex = 0;
        PlayCurrentLine();
    }

    public void OnScreenTapped()
    {
        if (sequence == null) return;
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            if (sequenceIndex < sequence.Length &&
                lineIndex < sequence[sequenceIndex].lines.Length)
                dialogueUI.ShowFullText(sequence[sequenceIndex].lines[lineIndex].text);
            isTyping = false;
            return;
        }
        AdvanceLine();
    }

    private void AdvanceLine()
    {
        if (sequence == null || sequenceIndex >= sequence.Length) return;

        lineIndex++;
        DialogueData current = sequence[sequenceIndex];

        if (current == null || current.lines == null || lineIndex >= current.lines.Length)
        {
            if (current != null && current.choices != null && current.choices.Length > 0)
            {
                dialogueUI.Hide();
                choiceSystem.ShowChoices(current.choices, OnChoiceMade);
            }
            else
            {
                MoveToNextDialogue();
            }
            return;
        }
        PlayCurrentLine();
    }

    private void PlayCurrentLine()
    {
        if (sequence == null || sequenceIndex >= sequence.Length) return;
        var data = sequence[sequenceIndex];
        if (data == null || data.lines == null || lineIndex >= data.lines.Length)
        {
            Debug.LogWarning($"[DialogueManager] 대화 데이터 없음: sequenceIndex={sequenceIndex}, lineIndex={lineIndex}");
            MoveToNextDialogue();
            return;
        }

        // dialogueUI 재탐색 (씬 전환 후 null이 될 수 있음, 비활성 포함)
        if (dialogueUI == null)
            dialogueUI = UnityEngine.Object.FindFirstObjectByType<DialogueUI>(FindObjectsInactive.Include);
        // Bootstrap이 방금 만든 것도 체크
        if (dialogueUI == null && DialogueBootstrap.CreatedDialogueUI != null)
            dialogueUI = DialogueBootstrap.CreatedDialogueUI;
        if (dialogueUI == null)
        {
            Debug.LogWarning("[DialogueManager] DialogueUI 없음 → 대화 건너뛰고 완료 처리");
            // UI 없이 대화 시작 불가 → 완료 콜백 즉시 호출 (엔딩으로 진행)
            var cb = OnSequenceComplete;
            OnSequenceComplete = null;
            if (cb != null) { cb.Invoke(); return; }
            if (StepController.Instance != null) { StepController.Instance.OnDialogueSequenceComplete(); return; }
            return;
        }

        DialogueLine line = data.lines[lineIndex];

        // 캐릭터 표시: 현재 화자만 보이고 나머지 숨김
        if (CharacterAnimator.Instance != null && !line.isMonologue)
        {
            CharacterAnimator.Instance.ShowOnlySpeaker(line.speaker);
            CharacterAnimator.Instance.SetEmotion(line.speaker, line.emotion);
        }
        else if (CharacterAnimator.Instance != null && line.isMonologue)
        {
            CharacterAnimator.Instance.HideAll();
        }

        // ── 효과 트리거 ────────────────────────────────────
        // FX 오버레이
        if (line.fxEffect != FXType.None)
            FXManager.Instance?.TriggerFX(line.fxEffect);

        // CG 이미지 (표시 후 탭하면 대화 재개)
        if (line.cgScene != CGType.None && CGViewer.Instance != null)
        {
            isTyping = true;          // CG 표시 중에는 스킵 방지
            dialogueUI?.Hide();
            var capturedLine = line;  // 람다 값 캡처 (struct이므로 복사)
            CGViewer.Instance.ShowCG(line.cgScene, () =>
            {
                isTyping = false;
                if (dialogueUI != null) dialogueUI.Show(capturedLine);
                typingCoroutine = StartCoroutine(TypeLine(capturedLine.text));
            });
            return; // CG 콜백에서 TypeLine 시작
        }

        // OBJ 소품 컷인 (비동기 — 대화와 동시 진행)
        if (line.objCutIn != ObjectType.None)
            ObjectCutIn.Instance?.ShowObject(line.objCutIn);

        dialogueUI.Show(line);
        typingCoroutine = StartCoroutine(TypeLine(line.text));
    }

    private IEnumerator TypeLine(string text)
    {
        isTyping = true;
        dialogueUI.SetText(string.Empty);
        foreach (char c in text)
        {
            dialogueUI.AppendChar(c);
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    private void OnChoiceMade(ChoiceData choice)
    {
        // StatManager가 없으면(씬 직접 실행 시) 자동 생성
        if (StatManager.Instance == null)
        {
            Debug.LogWarning("[DialogueManager] StatManager 없음 → 자동 생성");
            new GameObject("StatManager").AddComponent<StatManager>();
        }
        StatManager.Instance.ApplyChoice(choice.intDelta, choice.riskDelta, choice.addictDelta);

        if (!string.IsNullOrEmpty(choice.nextScene))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.LoadScene(choice.nextScene, 0.5f);
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene(choice.nextScene);
            return;
        }
        MoveToNextDialogue();
    }

    private void MoveToNextDialogue()
    {
        sequenceIndex++;
        lineIndex = 0;

        if (sequenceIndex >= sequence.Length)
        {
            // 1순위: 외부에서 등록한 콜백 (엔딩 씬 등)
            if (OnSequenceComplete != null)
            {
                var cb = OnSequenceComplete;
                OnSequenceComplete = null;   // 중복 호출 방지
                cb.Invoke();
                return;
            }
            // 2순위: StepController (Step 씬)
            if (StepController.Instance != null)
            {
                StepController.Instance.OnDialogueSequenceComplete();
                return;
            }
            Debug.LogWarning("[DialogueManager] 대화 완료 - 등록된 완료 핸들러가 없음");
            return;
        }
        PlayCurrentLine();
    }
}
