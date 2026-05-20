using UnityEngine;
using System;
using System.Collections;

public class ChoiceSystem : MonoBehaviour
{
    public static ChoiceSystem Instance { get; private set; }

    [SerializeField] private ChoiceUI choiceUI;
    [SerializeField] private float forcedChoiceDelay = 1.2f;

    private Action<ChoiceData> onChoiceMade;
    private ChoiceData[] currentChoices;
    private bool isForcedMode;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Inspector 미연결 시 자동 탐색 (비활성 오브젝트 포함)
        if (choiceUI == null)
            choiceUI = UnityEngine.Object.FindFirstObjectByType<ChoiceUI>(FindObjectsInactive.Include);
        if (choiceUI == null && DialogueBootstrap.CreatedChoiceUI != null)
            choiceUI = DialogueBootstrap.CreatedChoiceUI;
    }

    public void ShowChoices(ChoiceData[] choices, Action<ChoiceData> callback)
    {
        // 씬 전환 후 null이 됐을 수 있으므로 재탐색
        if (choiceUI == null)
            choiceUI = UnityEngine.Object.FindFirstObjectByType<ChoiceUI>(FindObjectsInactive.Include);
        if (choiceUI == null && DialogueBootstrap.CreatedChoiceUI != null)
            choiceUI = DialogueBootstrap.CreatedChoiceUI;
        if (choiceUI == null) { Debug.LogError("[ChoiceSystem] ChoiceUI 없음!"); return; }

        currentChoices = choices;
        onChoiceMade = callback;
        isForcedMode = StatManager.Instance != null && StatManager.Instance.ADDICT >= 60;

        choiceUI.Render(choices, OnButtonClicked, isForcedMode);

        if (isForcedMode)
            StartCoroutine(ForceBadChoiceRoutine(choices));
    }

    private IEnumerator ForceBadChoiceRoutine(ChoiceData[] choices)
    {
        // 올바른 선택지 버튼 비활성화 연출 후 나쁜 선택지 강제 클릭
        choiceUI.DisableGoodChoices();
        yield return new WaitForSeconds(forcedChoiceDelay);

        for (int i = 0; i < choices.Length; i++)
        {
            if (choices[i].isForcedBad)
            {
                choiceUI.AnimateForcedClick(i);
                yield return new WaitForSeconds(0.6f);
                SelectChoice(choices[i]);
                yield break;
            }
        }
        // isForcedBad 없으면 마지막 선택지 강제
        SelectChoice(choices[choices.Length - 1]);
    }

    private void OnButtonClicked(int index)
    {
        if (isForcedMode) return; // 강제 모드 중 수동 클릭 무시
        SelectChoice(currentChoices[index]);
    }

    private void SelectChoice(ChoiceData choice)
    {
        // INT 조건 미달 선택지 잠금 처리
        if (choice.requiresMinINT && StatManager.Instance.INT < choice.minINTValue)
            return;

        choiceUI.Hide();
        onChoiceMade?.Invoke(choice);
    }

    public void EnableForcedChoice()
    {
        isForcedMode = true;
        choiceUI.ApplyRedTint(); // 선택지 텍스트 적색 처리
    }
}
